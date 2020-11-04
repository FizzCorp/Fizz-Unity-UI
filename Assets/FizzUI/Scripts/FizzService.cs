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

        #region UI
        private static readonly string UI_GROUP_TAG = "Groups";
        #endregion

        #region Properties
        public IFizzClient Client { get; private set; }

        public bool IsConnected { get { return (Client == null) ? false : ((Client.Chat == null) ? false : Client.Chat.IsConnected); } }

        public bool IsTranslationEnabled { get; private set; }

        public string UserId { get; private set; }

        public string UserName { get; private set; }

        public IFizzLanguageCode Language { get; private set; }

        public FizzGroupRepository GroupRepository { get { if (!_isIntialized) Initialize(); return _groupRepository; }  }

        public FizzUserRepository UserRepository { get { if (!_isIntialized) Initialize(); return _userRepository; } }

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

        public Action OnUserNotificationsSubscribed { get; set; }
        public Action OnUserNotificationsUnsubscribed { get; set; }
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
            _groupRepository.Open(userId);
            _userRepository.Open(userId);
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

            if (Channels != null) Channels.Clear();
            if (channelLookup != null) channelLookup.Clear();

            if (_groupRepository != null) _groupRepository.Close();

            if (_userRepository != null) _userRepository.Close();

            if (Client != null)
            {
                Client.Close(null);
            }
        }

        public void SubscribeChannel(FizzChannelMeta channelMeta)
        {
            if (!_isIntialized) Initialize();

            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before subscribing channel.");
                return;
            }

            if (channelMeta == null)
            {
                FizzLogger.E("FizzClient unable to subscribe, channel meta is null.");
                return;
            }

            if (channelLookup.ContainsKey(channelMeta.Id))
            {
                FizzLogger.W("FizzClient channel is already subscribed.");
                return;
            }

            FizzChannel channel = new FizzChannel(channelMeta);

            Channels.Add(channel);
            channelLookup.Add(channel.Id, channel);
            channel.SubscribeAndQueryLatest();
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

            if (!channelLookup.ContainsKey(channelId))
            {
                FizzLogger.W("FizzService unable to remove, channel [" + channelId + "] does not exist. ");
                return;
            }

            FizzChannel channel = channelLookup[channelId];
            channelLookup.Remove(channelId);
            Channels.Remove(channel);
            channel.Unsubscribe(null);
        }

        public void SubscribeUserNotifications(Action<FizzException> cb)
        {
            if (!_isIntialized) Initialize();

            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before subscribing user.");
                return;
            }

            if (IsConnected)
            {
                Client.Chat.UserNotifications.Subscribe(ex =>
                {
                    if (ex == null)
                    {
                        if (OnUserNotificationsSubscribed != null)
                        {
                            OnUserNotificationsSubscribed.Invoke();
                        }
                    }

                    FizzUtils.DoCallback(ex, cb);
                });
            }

        }

        public void UnsubscribeUserNotifications(Action<FizzException> cb)
        {
            if (!_isIntialized) Initialize();

            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before unsubscribing user.");
                return;
            }

            try
            {
                Client.Chat.UserNotifications.Unsubscribe(ex =>
                {
                    if (ex == null)
                    {
                        if (OnUserNotificationsUnsubscribed != null)
                        {
                            OnUserNotificationsUnsubscribed.Invoke();
                        }
                    }

                    FizzUtils.DoCallback(ex, cb);
                });
            }
            catch (Exception e)
            {
                FizzLogger.E(e);
            }
        }

        

        public FizzChannel GetChannel(string id)
        {
            if (!_isIntialized) Initialize();

            if (channelLookup.ContainsKey(id))
                return channelLookup[id];

            foreach (FizzGroup group in GroupRepository.Groups)
            {
                if (group.Channel.Id.Equals(id))
                    return group.Channel;
            }

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
            channelLookup = new Dictionary<string, FizzChannel>();

            _groupRepository = new FizzGroupRepository(Client, UI_GROUP_TAG);
            _userRepository = new FizzUserRepository(Client);

            AddInternalListeners();

            _isIntialized = true;
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
            }
            catch (FizzException ex)
            {
                FizzLogger.E("Unable to unbind chat listeners. " + ex.Message);
            }
        }

        void Listener_OnMessagePublished(FizzChannelMessage msg)
        {
            FizzChannel channel = GetChannel(msg.To);
            if (channel != null)
            {
                channel.AddMessage(msg);

                if (OnChannelMessagePublish != null)
                {
                    OnChannelMessagePublish.Invoke(msg.To, msg);
                }
            }
        }

        void Listener_OnMessageDeleted(FizzChannelMessage msg)
        {
            FizzChannel channel = GetChannel(msg.To);
            if (channel != null)
            {
                channel.RemoveMessage(msg);

                if (OnChannelMessageDelete != null)
                {
                    OnChannelMessageDelete.Invoke(msg.To, msg);
                }
            }
        }

        void Listener_OnMessageUpdated(FizzChannelMessage msg)
        {
            FizzChannel channel = GetChannel(msg.To);
            if (channel != null)
            {
                channel.UpdateMessage(msg);

                if (OnChannelMessageUpdate != null)
                {
                    OnChannelMessageUpdate.Invoke(msg.To, msg);
                }
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
            FizzLogger.D("FizzService Listener_OnConnected = " + syncRequired);
            if (syncRequired)
            {
                foreach (FizzChannel channel in Channels)
                {
                    channel.SubscribeAndQueryLatest();
                }
            }

            if (OnConnected != null)
            {
                OnConnected.Invoke(syncRequired);
            }
        }

        private bool _isIntialized = false;
        private Dictionary<string, FizzChannel> channelLookup;
        private FizzGroupRepository _groupRepository;
        private FizzUserRepository _userRepository;
    }
}