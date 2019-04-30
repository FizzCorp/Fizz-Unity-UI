using Fizz.Chat;
using Fizz.Chat.Impl;
using Fizz.Common;
using Fizz.Ingestion;
using Fizz.Ingestion.Impl;
using Fizz.Threading;
using System;

namespace Fizz
{
    public enum FizzClientState
    {
        Closed,
        Opened
    }

    [Flags]
    public enum FizzServices
    {
        Chat = 1 << 0,
        Analytics = 1 << 1,
        All = Chat | Analytics
    }

    public interface IFizzClient
    {
        void Open(string userId, string locale, FizzServices services, Action<FizzException> callback);
        void Close(Action<FizzException> callback);
        void Update();

        IFizzChatClient Chat { get; }
        IFizzIngestionClient Ingestion { get; }
        FizzClientState State { get; }
        string Version { get; }
    }

    public class FizzClient : IFizzClient
    {
        readonly FizzChatClient _chat;
        readonly IFizzRestClient _restClient;
        readonly IFizzAuthRestClient _authClient;
        readonly IFizzSessionProvider _sessionClient;
        readonly FizzIngestionClient _ingestionClient;
        readonly FizzActionDispatcher _dispatcher = new FizzActionDispatcher();

        public FizzClient(string appId, string appSecret)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw FizzException.ERROR_INVALID_APP_ID;
            }

            _chat = new FizzChatClient(appId, _dispatcher);
            _restClient = new FizzRestClient(_dispatcher);
            _sessionClient = new FizzIdSecretSessionProvider(appId, appSecret, _restClient);
            _authClient = new FizzAuthRestClient(_restClient);
            _ingestionClient = new FizzIngestionClient(new FizzInMemoryEventLog(), _dispatcher);
        }

        public FizzClient(string appId, IFizzSessionProvider sessionClient)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw FizzException.ERROR_INVALID_APP_ID;
            }

            _sessionClient = sessionClient;
            _chat = new FizzChatClient(appId, _dispatcher);
            _restClient = new FizzRestClient(_dispatcher);
            _authClient = new FizzAuthRestClient(_restClient);
            _ingestionClient = new FizzIngestionClient(new FizzInMemoryEventLog(), _dispatcher);
        }

        public void Open(string userId, string locale, FizzServices services, Action<FizzException> callback)
        {
            try
            {
                if (State == FizzClientState.Opened)
                    return;

                FizzSessionRepository sessionRepo = new FizzSessionRepository(userId, locale, _sessionClient);
                _authClient.Open(sessionRepo, ex =>
                {
                    if (ex == null)
                    {
                        if (services.HasFlag(FizzServices.Chat))
                        {
                            _chat.Open(userId, _authClient, sessionRepo);
                        }
                        if (services.HasFlag(FizzServices.Analytics))
                        {
                            _ingestionClient.Open(userId, sessionRepo.Session._serverTS, _authClient);
                        }

                        State = FizzClientState.Opened;
                        FizzUtils.DoCallback(null, callback);
                    }
                    else
                    {
                        FizzUtils.DoCallback(ex, callback);
                    }
                });
            }
            catch (FizzException ex)
            {
                FizzUtils.DoCallback(ex, callback);
            }
        }

        public void Close(Action<FizzException> callback)
        {
            try
            {
                if (State == FizzClientState.Closed)
                    return;

                Close(() => { FizzUtils.DoCallback(null, callback); });
            }
            catch (FizzException ex)
            {
                FizzUtils.DoCallback(ex, callback);
            }
        }

        public void Update()
        {
            _dispatcher.Process();
        }

        public IFizzChatClient Chat
        {
            get
            {
                return _chat;
            }
        }

        public IFizzIngestionClient Ingestion
        {
            get
            {
                return _ingestionClient;
            }
        }

        public FizzClientState State { get; private set; }

        public string Version { get; } = "v1.4.5";

        private void Close(Action callback)
        {
            _ingestionClient.Close(() =>
            {
                _authClient.Close();
                _chat.Close(callback);
                State = FizzClientState.Closed;
            });
        }
    }
}
