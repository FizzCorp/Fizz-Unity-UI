using Fizz.Common;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using System;
using System.Threading;
using MQTTnet.Adapter;
using MQTTnet.Client.Disconnecting;
using UnityEngine;

namespace Fizz.Chat.Impl
{
    public class FizzMqttConnectException : Exception
    {
        public byte Code { get; private set; }

        public FizzMqttConnectException (byte code, string message) : base (message)
        {
            Code = code;
        }
    }

    public class FizzMqttAuthException : FizzMqttConnectException
    {
        public FizzMqttAuthException () : base (5, "invalid_credentials")
        { }
    }

    public class FizzMqttDisconnectedArgs
    {
        public bool ClientWasConnected { get; private set; }
        public Exception Exception { get; private set; }

        public FizzMqttDisconnectedArgs (bool clientWasConnected, Exception ex)
        {
            ClientWasConnected = clientWasConnected;
            Exception = ex;
        }
    }

    public interface IFizzMqttConnection
    {
        bool IsConnected { get; }

        Action<object, bool> Connected { set; get; }
        Action<object, FizzMqttDisconnectedArgs> Disconnected { set; get; }
        Action<object, byte[]> MessageReceived { set; get; }

        void ConnectAsync ();
        void DisconnectAsync ();
    }

    public class FizzMQTTConnection : IFizzMqttConnection
    {
        private static readonly FizzException ERROR_INVALID_CLIENT_ID = new FizzException (FizzError.ERROR_BAD_ARGUMENT, "invalid_client_id");
        private static readonly FizzException ERROR_INVALID_USERNAME = new FizzException (FizzError.ERROR_BAD_ARGUMENT, "invalid_username");
        private static readonly FizzException ERROR_INVALID_PASSWORD = new FizzException (FizzError.ERROR_BAD_ARGUMENT, "invalid_password");
        private static readonly FizzException ERROR_INVALID_DISPATCHER = new FizzException (FizzError.ERROR_BAD_ARGUMENT, "invalid_dispatcher");

        // TODO: make this exponential backoff
        private static readonly int RETRY_DELAY_MS = 10 * 1000;

        private readonly IMqttClient _client;
        private readonly IMqttClientOptions _clientOptions;

        private readonly bool _retry;
        private bool _manualDisconnect = false;
        private readonly IFizzActionDispatcher _dispatcher;

        public bool IsConnected => _client?.IsConnected ?? false;

        // TODO: make this thread safe using Interlocked.Add/Interlocked.Remove
        public Action<object, bool> Connected { set; get; }
        public Action<object, FizzMqttDisconnectedArgs> Disconnected { set; get; }
        public Action<object, byte[]> MessageReceived { set; get; }

        public FizzMQTTConnection (string username,
                                  string password,
                                  string clientId,
                                  bool retry,
                                  bool cleanSession,
                                  IFizzActionDispatcher dispatcher)
        {
            if (string.IsNullOrEmpty (clientId))
            {
                throw ERROR_INVALID_CLIENT_ID;
            }
            if (string.IsNullOrEmpty (username))
            {
                throw ERROR_INVALID_USERNAME;
            }
            if (string.IsNullOrEmpty (password))
            {
                throw ERROR_INVALID_PASSWORD;
            }
            if (dispatcher == null)
            {
                throw ERROR_INVALID_DISPATCHER;
            }

            _clientOptions = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(FizzConfig.MQTT_HOST_ENDPOINT)
                .WithCredentials(username, password)
                .WithTls()
                .WithCleanSession(cleanSession)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                .Build();

            _retry = retry;
            _dispatcher = dispatcher;

            var clientFactory = new MqttFactory();
            _client = clientFactory.CreateMqttClient();


            _client.UseDisconnectedHandler(discArgs =>
            {
                if (discArgs.Exception is MqttConnectingFailedException failedException)
                {
                    if (failedException.ResultCode == MqttClientConnectResultCode.BadUserNameOrPassword
                        || failedException.ResultCode == MqttClientConnectResultCode.NotAuthorized)
                        OnDisconnected(discArgs.ClientWasConnected, new FizzMqttAuthException());
                }
                else if (discArgs.Exception != null)
                {
                    OnDisconnected(discArgs.ClientWasConnected, discArgs.Exception);
                }
                else
                {
                    OnDisconnected(discArgs.ClientWasConnected, null);
                }
            });

            _client.UseConnectedHandler(conArgs => {
                if (_manualDisconnect) return;

                if (Connected != null)
                {
                    _dispatcher.Post(() => Connected.Invoke(this, conArgs.AuthenticateResult.IsSessionPresent));
                }
            });

            _client.UseApplicationMessageReceivedHandler(OnMqttMessageReceived);
        }

        public void ConnectAsync ()
        {
            _manualDisconnect = false;
            ConnectInternal ();
        }

        private void ConnectInternal ()
        {
            try
            {
                _client.ConnectAsync(_clientOptions);
            }
            catch (Exception ex)
            {
                OnDisconnected (false, ex);
            }
        }

        public void DisconnectAsync ()
        {
            _manualDisconnect = true;

            if (IsConnected)
            {
                _client.DisconnectAsync();
            }
            else
            {
                if (Disconnected != null)
                {
                    _dispatcher.Post(() => Disconnected.Invoke(this, new FizzMqttDisconnectedArgs(IsConnected, null)));
                }
            }
        }

        private void OnDisconnected (bool clientConnected, Exception ex)
        {
            if (Disconnected != null)
            {
                _dispatcher.Post (() => Disconnected.Invoke (this, new FizzMqttDisconnectedArgs (false, ex)));
            }

            if (_manualDisconnect)
            {
                return; 
            }

            if (!_retry)
            {
                return;
            }

            _dispatcher.Delay(RETRY_DELAY_MS, () => {
                try
                {
                    if (_client != null && !_manualDisconnect)
                    {
                        ConnectInternal ();
                    }
                }
                catch
                {
                    FizzLogger.E ("Unable to reconnect to Fizz event service.");
                }
            });
        }

        private void OnMqttMessageReceived (MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            if (MessageReceived != null)
            {
                _dispatcher.Post (() => MessageReceived.Invoke (this, eventArgs.ApplicationMessage.Payload));
            }
        }
    }
}
