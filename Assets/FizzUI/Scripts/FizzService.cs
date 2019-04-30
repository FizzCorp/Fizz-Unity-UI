using System;
using System.Collections.Generic;
using Fizz.Chat;
using Fizz.Common;
using Fizz.UI.Model;
using UnityEngine;

namespace Fizz
{
    public class FizzService : MonoBehaviour
    {

        private static readonly string APP_ID = "751326fc-305b-4aef-950a-074c9a21d461";
        private static readonly string APP_SECRET = "5c963d03-64e6-439a-b2a9-31db60dd0b34";

        public IFizzClient Client { get; private set; }

        public bool IsConnected { get; private set; }

        public bool IsTranslationEnabled { get; private set; }

        public string UserId { get; private set; }

        public string UserName { get; private set; }

        public IFizzLanguageCode LanguageCode { get; private set; }

        public List<FizzChannel> Channels { get; private set; }

        public Action<bool> OnConnected { get; set; }
        public Action<FizzException> OnDisconnected { get; set; }

        public Action<string, FizzChannelMessage> OnChannelMessagePublish { get; set; }
        public Action<string, FizzChannelMessage> OnChannelMessageUpdate { get; set; }
        public Action<string, FizzChannelMessage> OnChannelMessageDelete { get; set; }

        public Action<string> OnChannelSubscribed { get; set; }
        public Action<string> OnChannelUnsubscribed { get; set; }
        public Action<string> OnChannelMessagesAvailable { get; set; }

        private Dictionary<string, FizzChannel> channelLoopup;

        private static readonly object m_Lock = new object();
        private static FizzService m_Instance = null;

        public static FizzService Instance
        {
            get
            {
                lock (m_Lock)
                {
                    if (m_Instance == null)
                    {
                        // Search for existing instance.
                        m_Instance = (FizzService)FindObjectOfType(typeof(FizzService));

                        // Create new instance if one doesn't already exist.
                        if (m_Instance == null)
                        {
                            // Need to create a new GameObject to attach the singleton to.
                            var singletonObject = new GameObject();
                            m_Instance = singletonObject.AddComponent<FizzService>();
                            singletonObject.name = typeof(FizzService).ToString() + " (Singleton)";

                            // Make instance persistent.
                            DontDestroyOnLoad(singletonObject);
                        }
                    }

                    return m_Instance;
                }
            }
        }

        public void Open(string userId, string userName, IFizzLanguageCode langCode, bool tranlation, Action<bool> onDone)
        {
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

            if (langCode == null)
            {
                FizzLogger.E("FizzService can not open client with null language code.");
                return;
            }

            UserId = userId;
            UserName = userName;
            LanguageCode = langCode;
            IsTranslationEnabled = tranlation;
            Client.Open(userId, langCode.Code, FizzServices.All, ex =>
            {
                if (onDone != null)
                    onDone(ex == null);
            });
        }

        public void Close()
        {
            if (Client != null)
            {
                Client.Close(ex =>
                {
                    IsConnected = false;
                });
            }

            Channels.Clear();
            channelLoopup.Clear();
        }

        public void SubscribeChannel(FizzChannelMeta meta)
        {
            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opnened before subscrirbing channel.");
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
            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opnened before unsubscrirbing channel.");
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
                    FizzLogger.W("FizzService unable to unsibscribe, channel [" + channelId + "] does not exist. ");
                }
            }
            catch (Exception e)
            {
                FizzLogger.E(e);
            }
        }

        public FizzChannel GetChannel(string id)
        {
            if (channelLoopup.ContainsKey(id))
                return channelLoopup[id];

            return null;
        }

        void Awake()
        {
            UserId = "fizz_user";
            UserName = "Fizz User";
            IsConnected = false;
            IsTranslationEnabled = true;
            LanguageCode = FizzLanguageCodes.English;

            Client = new FizzClient(APP_ID, APP_SECRET);
            Channels = new List<FizzChannel>();

            channelLoopup = new Dictionary<string, FizzChannel>();

            AddInternalListeners();
        }

        void Update()
        {
            if (Client != null)
            {
                Client.Update();
            }
        }

        void AddInternalListeners()
        {
            Client.Chat.Listener.OnConnected += Listener_OnConnected;
            Client.Chat.Listener.OnDisconnected += Listener_OnDisconnected;
            Client.Chat.Listener.OnMessageUpdated += Listener_OnMessageUpdated;
            Client.Chat.Listener.OnMessageDeleted += Listener_OnMessageDeleted;
            Client.Chat.Listener.OnMessagePublished += Listener_OnMessagePublished;
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
            IsConnected = false;

            if (OnDisconnected != null)
            {
                OnDisconnected.Invoke(obj);
            }
        }

        void Listener_OnConnected(bool syncRequired)
        {
            IsConnected = true;

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

        FizzChannel AddChannelToList(FizzChannelMeta channelMeta)
        {
            if (channelLoopup.ContainsKey(channelMeta.Id))
                return null;

            FizzChannel channel = new FizzChannel(channelMeta);
            Channels.Add(channel);
            channelLoopup.Add(channel.Id, channel);
            return channel;
        }

        void OnApplicationQuit()
        {
            Close();
        }
    }
}