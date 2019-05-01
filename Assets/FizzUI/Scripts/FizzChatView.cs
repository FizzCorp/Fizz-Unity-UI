using Fizz.Common;
using Fizz.UI.Core;
using Fizz.UI.Model;
using UnityEngine;

namespace Fizz.UI
{
    public class FizzChatView : FizzBaseComponent
    {
        [SerializeField] FizzChannelListView ChannelListView;
        [SerializeField] FizzChatMessageView ChatMessageView;
        [SerializeField] FizzChatInputView ChatInputView;

        private bool _showChannels = true;
        private bool _showInputView = true;
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
        /// Show/Hide Chat Input
        /// </summary>
        public bool ShowInput
        {
            get { return _showInputView; }
            set
            {
                _showInputView = value;
                UpdateInputViewVisibility();
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
                ChatMessageView.EnableHistoryFetch = _enableFetchHistory;
            }
        }

        /// <summary>
        /// Show/Hide Message Translations
        /// </summary>
        public bool ShowMessageTranslation
        {
            get { return _showTranslation; }
            set
            {
                _showTranslation = value;
                ChatMessageView.ShowMessageTranslation = _showTranslation;
            }
        }

        /// <summary>
        /// Add Channel to UI. Note that Channel should be added to FizzService first.
        /// </summary>
        /// <param name="channelId">Id of channel to be added in UI</param>
        /// <param name="select">Select channel</param>
        public void AddChannel(string channelId, bool select = false)
        {
            ChannelListView.AddChannel(channelId, select);
        }

        /// <summary>
        /// Remove channel from UI
        /// </summary>
        /// <param name="channelId">Id of channel to be removed from UI</param>
        public void RemoveChannel(string channelId)
        {
            ChannelListView.RemoveChannel(channelId);
        }

        /// <summary>
        /// Set the current channel which is already added to UI.
        /// </summary>
        /// <param name="channelId">Id of the Channel to select</param>
        public bool SetCurrentChannel(string channelId)
        {
            return ChannelListView.SetChannel(channelId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public void SetCustomDataViewSource(IFizzChatViewCustomDataSource source)
        {
            ChatMessageView.SetCustomDataSource(source);
        }

        /// <summary>
        /// Reset all the content which includes channel list, messages and input. 
        /// </summary>
        public void Reset()
        {
            ChannelListView.Reset();
            ChatMessageView.Reset();
            ChatInputView.Reset();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            ChatInputView.OnSend.AddListener(HandleSend);
            ChannelListView.OnChannelSelected.AddListener(HandleChannelSelected);

            SyncViewState();
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

            ChatMessageView.AddNewMessage(text);
        }

        private void HandleChannelSelected(FizzChannel channel)
        {
            if (channel != null)
            {
                ChatMessageView.SetChannel(channel.Id);
            }
            else
            {
                ChatMessageView.Reset();
            }
        }

        private void UpdateChannelListVisibility()
        {
            ChannelListView.SetVisibility(_showChannels);
            ChatMessageView.RectTransform.offsetMax = _showChannels ? Vector2.down * 80 : Vector2.zero;
        }

        private void UpdateInputViewVisibility()
        {
            ChatInputView.gameObject.SetActive(_showInputView);
            ChatMessageView.RectTransform.offsetMin = _showInputView ? Vector2.up * 80 : Vector2.zero;
        }

        private void SyncViewState()
        {
            HandleChannelSelected(ChannelListView.CurrentSelectedChannel);

            ChatInputView.Reset();
        }
    }
}