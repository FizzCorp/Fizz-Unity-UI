using System;
using System.Collections.Generic;
using Fizz;
using Fizz.Chat;
using Fizz.Common;
using Fizz.UI;
using Fizz.UI.Model;
using UnityEngine;

namespace Fizz
{
    public class FizzService : MonoBehaviour
    {

        private static string APP_ID = "751326fc-305b-4aef-950a-074c9a21d461";
        private static string APP_SECRET = "5c963d03-64e6-439a-b2a9-31db60dd0b34";

        public IFizzClient Client { get; private set; }

        public bool IsConnected { get; private set; }

        public bool IsTranslationEnabled { get; private set; }

        public string UserId { get; private set; }

        public string UserName { get; private set; }

        public List<FizzChannel> Channels { get; private set; }

        public Action<bool> OnConnected;
        public Action<FizzException> OnDisconnected;

        public Action<string, FizzChannelMessage> OnChannelMessage;
        public Action<string, FizzChannelMessage> OnChannelMessageUpdate;
        public Action<string, FizzChannelMessage> OnChannelMessageDelete;

        public Action<string> OnChannelHistoryUpdated;

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

        void Awake()
        {
            UserId = "fizz_user";
            UserName = "Fizz User";
            IsConnected = false;
            IsTranslationEnabled = true;

            Client = new FizzClient(APP_ID, APP_SECRET);
            Channels = new List<FizzChannel>();

            channelLoopup = new Dictionary<string, FizzChannel>();
        }

        void Update()
        {
            if (Client != null)
            {
                Client.Update();
            }
        }

        public void Open(string userId, string userName, string locale, bool tranlation, List<FizzChannelMeta> channelList, Action<bool> onDone)
        {
            UserId = userId;
            UserName = userName;
            AddChannelsInternal(channelList);
            IsTranslationEnabled = tranlation;
            Client.Open(userId, locale, FizzServices.All, ex =>
            {
                if (ex == null)
                {

                    Client.Chat.Listener.OnConnected += Listener_OnConnected;
                    Client.Chat.Listener.OnDisconnected += Listener_OnDisconnected;
                    Client.Chat.Listener.OnMessageUpdated += Listener_OnMessageUpdated;
                    Client.Chat.Listener.OnMessageDeleted += Listener_OnMessageDeleted;
                    Client.Chat.Listener.OnMessagePublished += Listener_OnMessagePublished;
                }

                if (onDone != null)
                    onDone(ex == null);
            });
        }

        public void Close()
        {
            if (Client != null)
            {
                if (Client.Chat != null)
                {
                    Client.Chat.Listener.OnConnected -= Listener_OnConnected;
                    Client.Chat.Listener.OnDisconnected -= Listener_OnDisconnected;
                    Client.Chat.Listener.OnMessageUpdated -= Listener_OnMessageUpdated;
                    Client.Chat.Listener.OnMessageDeleted -= Listener_OnMessageDeleted;
                    Client.Chat.Listener.OnMessagePublished -= Listener_OnMessagePublished;
                }

                Client.Close(ex =>
                {
                    IsConnected = false;
                });

            }

            Channels.Clear();
            channelLoopup.Clear();
        }

        public void AddChannel(FizzChannelMeta channel)
        {
            if (channel == null)
                return;

            if (channelLoopup.ContainsKey(channel.Id))
                return;

            FizzChannel fizzChannel = AddChannelInternal(channel);

            if (IsConnected && fizzChannel != null)
            {
                fizzChannel.SubscribeAndQuery();
            }
        }

        public void RemoveChannel(FizzChannelMeta channel)
        {
            if (channel == null)
                return;

            if (string.IsNullOrEmpty(channel.Id))
                return;

            try
            {
                if (channelLoopup.ContainsKey(channel.Id))
                {
                    FizzChannel fizzChannel = channelLoopup[channel.Id];
                    channelLoopup.Remove(channel.Id);
                    Channels.Remove(fizzChannel);
                    fizzChannel.Unsubscribe(ex => { });
                }
                else
                {
                    FizzLogger.W("FizzService RemoveChannel channel does not exist");
                }
            }
            catch (Exception e)
            {
                FizzLogger.E(e);
            }
        }

        public void PublishMessage(string channel, string nick, string body, Dictionary<string, string> data, bool translate, bool persist, Action<FizzException> callback)
        {
            if (Client != null)
            {
                Client.Chat.PublishMessage(channel, nick, body, data, translate, persist, ex =>
                {
                    if (ex == null)
                    {
                        Client.Ingestion.TextMessageSent(channel, body, nick);
                    }
                    if (callback != null)
                    {
                        callback.Invoke(ex);
                    }
                });
            }
        }

        public void UpdateMessage(string channel, long messageId, string nick, string body, Dictionary<string, string> data, bool translate, bool persist, Action<FizzException> callback)
        {
            if (Client != null)
            {
                Client.Chat.UpdateMessage(channel, messageId, nick, body, data, translate, persist, callback);
            }
        }

        public void DeleteMessage(string channelId, long messageId, Action<FizzException> callback)
        {
            if (Client != null)
            {
                Client.Chat.DeleteMessage(channelId, messageId, callback);
            }
        }

        public FizzChannel GetChannelById(string id)
        {
            if (channelLoopup.ContainsKey(id))
                return channelLoopup[id];

            return null;
        }

        void Listener_OnMessagePublished(Fizz.Chat.FizzChannelMessage msg)
        {
            if (channelLoopup.ContainsKey(msg.To))
            {
                channelLoopup[msg.To].AddMessage(msg);
            }

            if (OnChannelMessage != null)
            {
                OnChannelMessage.Invoke(msg.To, msg);
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

        void AddChannelsInternal(List<FizzChannelMeta> channelList)
        {
            if (channelList == null)
                return;

            foreach (FizzChannelMeta meta in channelList)
            {
                AddChannelInternal(meta);
            }
        }

        FizzChannel AddChannelInternal(FizzChannelMeta channelMeta)
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
