using System;
using System.Collections.Generic;
using Fizz.Chat;
using Fizz.Common;
using Fizz.UI.Model;
using UnityEngine;

namespace Fizz
{
    /// <summary>
    /// FizzService is an intermediate class(MonoBehaviour, DontDestroyOnLoad) designed to work like bridge 
    /// between FizzClient and FizzUI. It contains an instance of FizzClient which can be opened and closed 
    /// according to game client need. It's also used to Subscribe and Unsubscribe channels when client is opened.
    /// </summary>
    public class FizzService : Singleton<FizzService>
    {
        #region Id and Secret
        private static readonly string APP_ID = "751326fc-305b-4aef-950a-074c9a21d461";
        private static readonly string APP_SECRET = "5c963d03-64e6-439a-b2a9-31db60dd0b34";
        #endregion

        #region Properties
        public IFizzClient Client { get; private set; }

        public bool IsConnected { get { return (Client == null) ? false : ((Client.Chat == null) ? false : Client.Chat.IsConnected); } }

        public bool IsTranslationEnabled { get; private set; }

        public string UserId { get; private set; }

        public string UserName { get; private set; }

        public IFizzLanguageCode Language { get; private set; }

        public List<FizzChannel> Channels { get; private set; }
        #endregion

        #region Events
        public Action<bool> OnConnected { get; set; }
        public Action<FizzException> OnDisconnected { get; set; }

        public Action<string, FizzChannelMessage> OnChannelMessagePublish { get; set; }
        public Action<string, FizzChannelMessage> OnChannelMessageUpdate { get; set; }
        public Action<string, FizzChannelMessage> OnChannelMessageDelete { get; set; }

        public Action<string> OnChannelSubscribed { get; set; }
        public Action<string> OnChannelUnsubscribed { get; set; }
        public Action<string> OnChannelMessagesAvailable { get; set; }

        public Action<FizzUserUpdateEventData> OnUserUpdated { get; set; }
        public Action<string> OnUserSubscribed { get; set; }
        public Action<string> OnUserUnsubscribed { get; set; }
        #endregion

        public void Open(string userId, string userName, IFizzLanguageCode lang, FizzServices services, bool tranlation, Action<bool> onDone)
        {
            if (!_isIntialized) Initialize();

            if (string.IsNullOrEmpty(userId))
            {
                FizzLogger.E("FizzService can not open client with null of empty userId.");
                return;
            }

            if (string.IsNullOrEmpty(userName))
            {
                FizzLogger.E("FizzService can not open client with null of empty userName.");
                return;
            }

            if (lang == null)
            {
                FizzLogger.E("FizzService can not open client with null language code.");
                return;
            }

            UserId = userId;
            UserName = userName;
            Language = lang;
            IsTranslationEnabled = tranlation;
            Client.Open(userId, lang, services, ex =>
            {
                if (onDone != null)
                    onDone(ex == null);

                if (ex != null)
                {
                    FizzLogger.E("Something went wrong while connecting to FizzClient. " + ex);
                }
            });
        }

        public void Close()
        {
            if (!_isIntialized) Initialize();

            if (Client != null)
            {
                Client.Close(null);
            }

            if (Channels != null) Channels.Clear();
            if (channelLoopup != null) channelLoopup.Clear();
            if (userSubcriptionLookup != null) userSubcriptionLookup.Clear();
        }

        public void SubscribeChannel(FizzChannelMeta meta)
        {
            if (!_isIntialized) Initialize();

            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before subscribing channel.");
                return;
            }

            if (meta == null)
            {
                FizzLogger.E("FizzClient unable to subscribe, channel meta is null.");
                return;
            }

            if (channelLoopup.ContainsKey(meta.Id))
            {
                FizzLogger.W("FizzClient channel is already subscribed.");
                return;
            }

            FizzChannel fizzChannel = AddChannelToList(meta);

            if (IsConnected && fizzChannel != null)
            {
                fizzChannel.SubscribeAndQuery();
            }
        }

        public void UnsubscribeChannel(string channelId)
        {
            if (!_isIntialized) Initialize();

            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before unsubscribing channel.");
                return;
            }

            if (string.IsNullOrEmpty(channelId))
            {
                FizzLogger.E("FizzClient unable to unsubscribe, channelId is null or empty.");
                return;
            }

            try
            {
                if (channelLoopup.ContainsKey(channelId))
                {
                    FizzChannel fizzChannel = channelLoopup[channelId];
                    channelLoopup.Remove(channelId);
                    Channels.Remove(fizzChannel);
                    fizzChannel.Unsubscribe(ex => { });
                }
                else
                {
                    FizzLogger.W("FizzService unable to unsubscribe, channel [" + channelId + "] does not exist. ");
                }
            }
            catch (Exception e)
            {
                FizzLogger.E(e);
            }
        }

        public void GetUser(string userId, Action<IFizzUser, FizzException> cb)
        {
            if(!_isIntialized) Initialize();

            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before fetching user.");
                return;
            }

            if (userId == null)
            {
                FizzLogger.E("FizzClient unable to fetch, userId is null.");
                return;
            }

            FizzService.Instance.Client.Chat.Users.GetUser(userId, (user, ex) =>
            {
                FizzUtils.DoCallback(user, ex, cb);
                FizzLogger.W(userId + " Get User Online = " + user.Online);
            });
        }

        public void SubscribeUser(string userId, Action<FizzException> cb)
        {
            if (!_isIntialized) Initialize();

            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before subscribing user.");
                return;
            }

            if (userId == null)
            {
                FizzLogger.E("FizzClient unable to subscribe, userId is null.");
                return;
            }

            if (userSubcriptionLookup.ContainsKey(userId))
            {
                FizzLogger.W("FizzClient userId is already subscribed.");
                return;
            }

            AddUserToList(userId);

            if (IsConnected)
            {
                FizzService.Instance.Client.Chat.Users.Subscribe(userId, ex =>
                {
                    if (ex == null)
                    {
                        if (FizzService.Instance.OnUserSubscribed != null)
                        {
                            FizzService.Instance.OnUserSubscribed.Invoke(userId);
                        }
                    }

                    FizzUtils.DoCallback(ex, cb);
                });
            }

        }

        public void UnsubscribeUser(string userId, Action<FizzException> cb)
        {
            if (!_isIntialized) Initialize();

            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before unsubscribing user.");
                return;
            }

            if (string.IsNullOrEmpty(userId))
            {
                FizzLogger.E("FizzClient unable to unsubscribe, userId is null or empty.");
                return;
            }

            try
            {
                if (userSubcriptionLookup.ContainsKey(userId))
                {
                    userSubcriptionLookup.Remove(userId);
                    FizzService.Instance.Client.Chat.Users.Unsubscribe(userId, ex =>
                    {
                        if (ex == null)
                        {
                            if (FizzService.Instance.OnUserUnsubscribed != null)
                            {
                                FizzService.Instance.OnUserUnsubscribed.Invoke(userId);
                            }
                        }

                        FizzUtils.DoCallback(ex, cb);
                    });
                }
                else
                {
                    FizzLogger.W("FizzService unable to unsubscribe, user [" + userId + "] does not exist. ");
                }
            }
            catch (Exception e)
            {
                FizzLogger.E(e);
            }
        }

        public FizzChannel GetChannel(string id)
        {
            if (!_isIntialized) Initialize();

            if (channelLoopup.ContainsKey(id))
                return channelLoopup[id];

            return null;
        }

        void Update()
        {
            if (Client != null)
            {
                Client.Update();
            }
        }

        public FizzService ()
        {
            if (_isIntialized) Initialize();
        }

        void Initialize()
        {
            if (_isIntialized) return;

            UserId = "fizz_user";
            UserName = "Fizz User";
            IsTranslationEnabled = true;
            Language = FizzLanguageCodes.English;

            Client = new FizzClient(APP_ID, APP_SECRET);
            Channels = new List<FizzChannel>();

            channelLoopup = new Dictionary<string, FizzChannel>();
            userSubcriptionLookup = new Dictionary<string, string>();

            AddInternalListeners();

            _isIntialized = true;
        }

        FizzChannel AddChannelToList(FizzChannelMeta channelMeta)
        {
            if (channelLoopup.ContainsKey(channelMeta.Id))
                return null;

            FizzChannel channel = new FizzChannel(channelMeta);
            Channels.Add(channel);
            channelLoopup.Add(channel.Id, channel);
            return channel;
        }

        void AddUserToList(string userId)
        {
            userSubcriptionLookup.Add(userId, userId);
        }

        void OnDestroy()
        {
            Close();
            RemoveInternalListeners();
        }

        void AddInternalListeners()
        {
            try
            {
                Client.Chat.Listener.OnConnected += Listener_OnConnected;
                Client.Chat.Listener.OnDisconnected += Listener_OnDisconnected;
                Client.Chat.Listener.OnMessageUpdated += Listener_OnMessageUpdated;
                Client.Chat.Listener.OnMessageDeleted += Listener_OnMessageDeleted;
                Client.Chat.Listener.OnMessagePublished += Listener_OnMessagePublished;
                Client.Chat.UserListener.OnUserUpdated += Listener_OnUserUpdated;
            }
            catch (FizzException ex)
            {
                FizzLogger.E("Unable to bind chat listeners. " + ex.Message);
            }
        }

        void RemoveInternalListeners()
        {
            try
            {
                Client.Chat.Listener.OnConnected -= Listener_OnConnected;
                Client.Chat.Listener.OnDisconnected -= Listener_OnDisconnected;
                Client.Chat.Listener.OnMessageUpdated -= Listener_OnMessageUpdated;
                Client.Chat.Listener.OnMessageDeleted -= Listener_OnMessageDeleted;
                Client.Chat.Listener.OnMessagePublished -= Listener_OnMessagePublished;
                Client.Chat.UserListener.OnUserUpdated -= Listener_OnUserUpdated;
            }
            catch (FizzException ex)
            {
                FizzLogger.E("Unable to unbind chat listeners. " + ex.Message);
            }
        }

        void Listener_OnMessagePublished(FizzChannelMessage msg)
        {
            if (channelLoopup.ContainsKey(msg.To))
            {
                channelLoopup[msg.To].AddMessage(msg);
            }

            if (OnChannelMessagePublish != null)
            {
                OnChannelMessagePublish.Invoke(msg.To, msg);
            }
        }

        void Listener_OnMessageDeleted(FizzChannelMessage msg)
        {
            if (channelLoopup.ContainsKey(msg.To))
            {
                channelLoopup[msg.To].RemoveMessage(msg);
            }

            if (OnChannelMessageDelete != null)
            {
                OnChannelMessageDelete.Invoke(msg.To, msg);
            }
        }

        void Listener_OnMessageUpdated(FizzChannelMessage msg)
        {
            if (channelLoopup.ContainsKey(msg.To))
            {
                channelLoopup[msg.To].UpdateMessage(msg);
            }

            if (OnChannelMessageUpdate != null)
            {
                OnChannelMessageUpdate.Invoke(msg.To, msg);
            }
        }

        void Listener_OnDisconnected(FizzException obj)
        {
            if (OnDisconnected != null)
            {
                OnDisconnected.Invoke(obj);
            }
        }

        void Listener_OnConnected(bool syncRequired)
        {
            if (OnConnected != null)
            {
                OnConnected.Invoke(syncRequired);
            }

            if (!syncRequired)
                return;

            foreach (FizzChannel channel in Channels)
            {
                channel.SubscribeAndQuery();
            }
        }

        void Listener_OnUserUpdated(FizzUserUpdateEventData eventData)
        {
            if (OnUserUpdated != null)
            {
                OnUserUpdated.Invoke(eventData);
            }
        }

        private bool _isIntialized = false;
        private Dictionary<string, FizzChannel> channelLoopup;
        private Dictionary<string, string> userSubcriptionLookup;
    }
}