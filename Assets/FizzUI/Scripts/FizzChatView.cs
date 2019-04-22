using Fizz.Common;
using Fizz.UI.Core;
using Fizz.UI.Model;
using UnityEngine;

namespace Fizz.UI
{
    public class FizzChatView : FizzBaseComponent
    {
        [SerializeField] FizzChannelListView ChannelListView;
        [SerializeField] FizzMessageListView ChatMessageListView;
        [SerializeField] FizzChatInputView ChatInputView;

        private bool _showChannels = true;
        private bool _showTranslation = true;
        private bool _enableFetchHistory = true;

        /// <summary>
        /// Show/Hide Channel list
        /// </summary>
        public bool ShowChannels
        {
            get { return _showChannels; }
            set
            {
                _showChannels = value;
                UpdateChannelListVisibility();
            }
        }

        /// <summary>
        /// Enable/Disable History Fetch
        /// </summary>
        public bool EnableFetchHistory
        {
            get { return _enableFetchHistory; }
            set
            {
                _enableFetchHistory = value;
                ChatMessageListView.EnableHistoryFetch = _enableFetchHistory;
            }
        }

        public bool ShowMessageTranslation
        {
            get { return _showTranslation; }
            set
            {
                _showTranslation = value;
                ChatMessageListView.ShowMessageTranslation = _showTranslation;
            }
        }

        public void AddChannel(FizzChannelMeta channelMeta, bool select = false)
        {
            ChannelListView.AddChannel(channelMeta, select);
        }

        public void RemoveChannel(FizzChannelMeta channelMeta)
        {
            ChannelListView.RemoveChannel(channelMeta);
        }

        public void SetChannel(FizzChannelMeta channelMeta)
        {
            ChannelListView.SetChannel(channelMeta);
        }

        public void SetCustomDataViewSource(IFizzChatViewCustomDataSource source)
        {
            ChatMessageListView.SetCustomDataSource(source);
        }

        public void Reset()
        {
            ChannelListView.Reset();
            ChatMessageListView.Reset();
            ChatInputView.Reset();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            ChatInputView.OnSend.AddListener(HandleSend);
            ChannelListView.OnChannelSelected.AddListener(HandleChannelSelected);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            ChatInputView.OnSend.RemoveListener(HandleSend);
            ChannelListView.OnChannelSelected.RemoveListener(HandleChannelSelected);
        }

        protected override void OnConnectionStateChange(bool isConnected)
        {
            base.OnConnectionStateChange(isConnected);

            FizzLogger.D("OnConnectionStateChange isConnected " + isConnected);
        }

        private void HandleSend(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            ChatMessageListView.AddNewMessage(text);
        }

        private void HandleChannelSelected(FizzChannelMeta channelMeta)
        {
            if (channelMeta != null)
            {
                ChatMessageListView.Reset();
                ChatMessageListView.SetChannel(channelMeta);
            }
        }

        private void UpdateChannelListVisibility()
        {
            ChannelListView.SetVisibility(_showChannels);
            ChatMessageListView.RectTransform.offsetMax = _showChannels ? Vector2.down * 80 : Vector2.zero;
        }
    }
}