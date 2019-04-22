using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using Fizz.Common;

namespace Fizz.Chat.Impl
{
    public interface IFizzMqttConnection
    {
        int Id { get; }
        Action<int, object, MqttClientConnectedEventArgs> Connected { set; get; }
        Action<int, object, MqttClientDisconnectedEventArgs> Disconnected { set; get; }
        Action<int, object, MqttApplicationMessageReceivedEventArgs> MessageReceived { set; get; }

        Task ConnectAsync();
        Task DisconnectAsync();
    }

    public class FizzMQTTConnection: IFizzMqttConnection
    {
        private static readonly FizzException ERROR_INVALID_CLIENT_ID = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_client_id");
        private static readonly FizzException ERROR_INVALID_USERNAME = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_username");
        private static readonly FizzException ERROR_INVALID_PASSWORD = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_password");
        private static readonly FizzException ERROR_INVALID_DISPATCHER = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_dispatcher");

        // TODO: make this exponential backoff
        private static int RETRY_DELAY_MS = 10*1000;

        private static int nextId = 0;

        private readonly object synclock = new object();
        private readonly int _id;
        private readonly string _clientId;
        private readonly IMqttClientOptions _options;
        private readonly bool _retry;
        private readonly IFizzActionDispatcher _dispatcher;
        private IMqttClient _client;
        private bool _manualDisconnect = false;

        // TODO: make this thread safe using Interlocked.Add/Interlocked.Remove
        public Action<int, object, MqttClientConnectedEventArgs> Connected { set; get; }
        public Action<int, object, MqttClientDisconnectedEventArgs> Disconnected { get; set; }
        public Action<int, object, MqttApplicationMessageReceivedEventArgs> MessageReceived { get; set; }

        public FizzMQTTConnection(string username,
                                  string password,
                                  string clientId, 
                                  bool retry, 
                                  bool cleanSession,
                                  IFizzActionDispatcher dispatcher)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw ERROR_INVALID_CLIENT_ID;
            }
            if (string.IsNullOrEmpty(username))
            {
                throw ERROR_INVALID_USERNAME;
            }
            if (string.IsNullOrEmpty(password))
            {
                throw ERROR_INVALID_PASSWORD;
            }
            if (dispatcher == null)
            {
                throw ERROR_INVALID_DISPATCHER; 
            }

            _clientId = clientId;
            _id = Interlocked.Increment(ref nextId);

            MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder()
                .WithClientId(_clientId)
                .WithCredentials(username, password)
                .WithTcpServer(FizzConfig.MQTT_HOST_ENDPOINT)
                .WithCleanSession(cleanSession);

            if (FizzConfig.MQTT_USE_TLS) {
                builder.WithTls();
            }

            _options = builder.Build();
            _retry = retry;
            _dispatcher = dispatcher;
        }

        public int Id
        {
            get
            {
                return _id;
            }
        }

        public Task ConnectAsync()
        {
            lock (synclock)
            {
                if (_client != null)
                {
                    return Task.CompletedTask;
                }

                _client = new MQTTnet.MqttFactory().CreateMqttClient();
                _client.Connected += OnConnected;
                _client.Disconnected += OnDisconnected;
                _client.ApplicationMessageReceived += OnMessageReceieved;

                var connected = new TaskCompletionSource<object>();
                _client.ConnectAsync(_options)
                       .ContinueWith(task => DispatchTask<object>(task, connected));
                return connected.Task;
            }
        }

        public Task DisconnectAsync()
        {
            lock (synclock)
            {
                if (_client == null)
                {
                    return Task.CompletedTask;
                }

                var client = _client;
                _client = null;
                _manualDisconnect = true;

                var disconnected = new TaskCompletionSource<object>();
                return client.DisconnectAsync()
                             .ContinueWith(task => DispatchTask<object>(task, disconnected));
            }
        }

        private void OnConnected(object sender, MqttClientConnectedEventArgs args)
        {
            if (Connected != null)
            {
                _dispatcher.Post(() => Connected.Invoke(_id, sender, args));
            }
        }

        private void OnMessageReceieved(object sender, MqttApplicationMessageReceivedEventArgs args)
        {
            if (MessageReceived != null)
            {
                _dispatcher.Post(() => MessageReceived.Invoke(_id, sender, args));
            }
        }

        private void OnDisconnected(object sender, MqttClientDisconnectedEventArgs args)
        {
            if (Disconnected != null)
            {
                _dispatcher.Post(() => Disconnected.Invoke(_id, sender, args));
            }

            if (_manualDisconnect)
            {
                _manualDisconnect = false;
                return;
            }
            if (!_retry)
            {
                return;
            }

            _dispatcher.Delay(RETRY_DELAY_MS, () =>
            {
                try
                {
					if (_client != null) {
                    	_client.ConnectAsync(_options);
					}
                }
                catch
                {
                    FizzLogger.E("Unable to reconnect to Fizz event service.");
                }
            });
        }

        private void DispatchTask<T>(Task source, TaskCompletionSource<T> dest) where T: class
        {
            if (source.Exception != null)
            {
                _dispatcher.Post(() => dest.SetException(source.Exception));
            }
            else
            {
                _dispatcher.Post(() => dest.SetResult(null));
            }
        }
    }
}
