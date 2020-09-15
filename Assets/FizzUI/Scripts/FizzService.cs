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

        public List<FizzChannel> Channels {
            get
            {
                List<FizzChannel> fizzChannels = new List<FizzChannel>();
                fizzChannels.AddRange(channels);
                foreach (FizzGroup group in Groups)
                {
                    fizzChannels.Add(group.Channel);
                }
                return channels;
            }
        }
        public List<FizzGroup> Groups { get; private set; }
        public List<IFizzUserGroup> GroupInvites { get; private set; }
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

            if (channels != null) channels.Clear();
            if (channelLookup != null) channelLookup.Clear();

            if (Groups != null) Groups.Clear();
            if (groupLookup != null) groupLookup.Clear();

            if (GroupInvites != null) GroupInvites.Clear();
        }

        public void AddGroup(FizzGroup group)
        {
            if (!_isIntialized) Initialize();

            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before adding group.");
                return;
            }

            if (group == null)
            {
                FizzLogger.E("FizzClient unable to add group, group is null.");
                return;
            }

            if (!groupLookup.ContainsKey(group.Id))
            {
                Groups.Add(group);
                groupLookup.Add(group.Id, group);
            }

            group = groupLookup[group.Id];
            group.Channel.SubscribeAndQueryLatest();
        }

        public void SubscribeChannel(FizzChannel channel)
        {
            if (!_isIntialized) Initialize();

            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before subscribing channel.");
                return;
            }

            if (channel == null)
            {
                FizzLogger.E("FizzClient unable to subscribe, channel is null.");
                return;
            }

            if (channelLookup.ContainsKey(channel.Id))
            {
                FizzLogger.W("FizzClient channel is already subscribed.");
                return;
            }

            channels.Add(channel);
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
            channels.Remove(channel);
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

            foreach (FizzGroup group in Groups)
            {
                if (group.Channel.Id.Equals(id))
                    return group.Channel;
            }

            return null;
        }

        public FizzGroup GetGroup(string id)
        {
            if (!_isIntialized) Initialize();

            if (groupLookup.ContainsKey(id))
                return groupLookup[id];

            return null;
        }

        private void SubscribeNotificationsAndFetchGroups()
        {
            SubscribeUserNotifications(ex =>
            {
                if (ex == null)
                {
                    IFizzFetchUserGroupsQuery groupFetchQuery = FizzService.Instance.Client.Chat.Users.BuildFetchUserGroupsQuery(FizzService.Instance.UserId);
                    FetchUserGroups(groupFetchQuery, new List<IFizzUserGroup>());
                }
            });
        }

        private void FetchUserGroups(IFizzFetchUserGroupsQuery groupFetchQuery, List<IFizzUserGroup> userGroups)
        {
            if (groupFetchQuery.HasNext)
            {
                groupFetchQuery.Next((groupList, ex) =>
                {
                    if (ex == null)
                    {
                        foreach (IFizzUserGroup group in groupList)
                        {
                            if (group.State == FizzGroupMemberState.Joined)
                            {
                                userGroups.Add(group);
                            }
                            else
                            {
                                GroupInvites.Add(group);
                            }
                        }
                        FetchUserGroups(groupFetchQuery, userGroups);
                    }
                });
            }
            else
            {
                FetchGroups(userGroups);
            }
        }

        private void FetchGroups(List<IFizzUserGroup> userGroups)
        {
            foreach (IFizzUserGroup userGroup in userGroups)
            {
                Client.Chat.Groups.FetchGroup(userGroup.GroupId, (group, ex) =>
                {
                    if (ex == null)
                    {
                        AddGroup(new FizzGroup(group));
                    }
                });
            }
        }

        void Update()
        {
            if (Client != null)
            {
                Client.Update();
            }
        }

        void Initialize()
        {
            if (_isIntialized) return;

            UserId = "fizz_user";
            UserName = "Fizz User";
            IsTranslationEnabled = true;
            Language = FizzLanguageCodes.English;

            Client = new FizzClient(APP_ID, APP_SECRET);
            channels = new List<FizzChannel>();
            Groups = new List<FizzGroup>();
            GroupInvites = new List<IFizzUserGroup>();

            channelLookup = new Dictionary<string, FizzChannel>();
            groupLookup = new Dictionary<string, FizzGroup>();

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
                foreach (FizzChannel channel in channels)
                {
                    channel.SubscribeAndQueryLatest();
                }

                SubscribeNotificationsAndFetchGroups();
            }

            if (OnConnected != null)
            {
                OnConnected.Invoke(syncRequired);
            }
        }

        private bool _isIntialized = false;
        private List<FizzChannel> channels;
        private Dictionary<string, FizzChannel> channelLookup;
        private Dictionary<string, FizzGroup> groupLookup;
    }
}