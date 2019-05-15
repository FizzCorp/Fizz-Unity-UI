﻿using Fizz.Common;
using Fizz.UI.Core;
using Fizz.UI.Model;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Fizz.UI
{
    /// <summary>
    /// FizzChatView is the core UI compoment which contains channel list, messages and input. 
    /// Channels are added and removed from it in order to show or hide them from view. But note
    /// that a channel should be Subsribed first by using FizzService. It can be configured to 
    /// show/hide channel list and input. It can be added to any container like, Tabs and Popup etc.
    /// </summary>
    public class FizzChatView : FizzBaseComponent
    {
        [SerializeField] FizzHeaderView HeaderView;
        [SerializeField] FizzChannelsView ChannelsView;
        [SerializeField] FizzMessagesView MessagesView;
        [SerializeField] FizzInputView InputView;
        [SerializeField] RectTransform BlockingLayer;

        private bool _showChannels = true;
        private bool _showInputView = true;
        private bool _showHeaderView = true;
        private bool _showTranslation = true;
        private bool _showCloseButton = true;
        private bool _enableFetchHistory = true;
        
        public UnityEvent onClose
        {
            get { return HeaderView.OnClose; }
            set { HeaderView.OnClose = value; }
        }

        /// <summary>
        /// Show/Hide Channel list
        /// </summary>
        public bool ShowChannels
        {
            get { return _showChannels; }
            set
            {
                _showChannels = value;
                HeaderView.SetChannelButtonVisibility (_showChannels);
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
                UpdateInputViewVisibility ();
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
                MessagesView.EnableHistoryFetch = _enableFetchHistory;
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
                MessagesView.ShowMessageTranslation = _showTranslation;
            }
        }

        public bool ShowHeaderView
        {
            get { return _showHeaderView; }
            set
            {
                _showHeaderView = value;
                HandleHeaderViewVisibility ();
            }
        }

        public bool ShowCloseButton
        {
            get { return _showCloseButton; }
            set
            {
                _showCloseButton = value;
                HeaderView.SetCloseButtonVisibility (_showCloseButton);
            }
        }

        /// <summary>
        /// Add Channel to UI. Note that Channel should be added to FizzService first.
        /// </summary>
        /// <param name="channelId">Id of channel to be added in UI</param>
        /// <param name="select">Select channel</param>
        public void AddChannel (string channelId, bool select = false)
        {
            ChannelsView.AddChannel (channelId, select);
        }

        /// <summary>
        /// Remove channel from UI
        /// </summary>
        /// <param name="channelId">Id of channel to be removed from UI</param>
        public void RemoveChannel (string channelId)
        {
            ChannelsView.RemoveChannel (channelId);
        }

        /// <summary>
        /// Set the current channel which is already added to UI.
        /// </summary>
        /// <param name="channelId">Id of the Channel to select</param>
        public bool SetCurrentChannel (string channelId)
        {
            return ChannelsView.SetChannel (channelId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public void SetCustomDataViewSource (IFizzCustomMessageCellViewDataSource source)
        {
            MessagesView.SetCustomDataSource (source);
        }

        /// <summary>
        /// Reset all the content which includes channel list, messages and input. 
        /// </summary>
        public void Reset ()
        {
            HeaderView.Reset ();
            ChannelsView.Reset ();
            MessagesView.Reset ();
            InputView.Reset ();
        }

        protected override void OnEnable ()
        {
            base.OnEnable ();

            InputView.OnSend.AddListener (HandleSend);
            ChannelsView.OnChannelSelected.AddListener (HandleChannelSelected);
            HeaderView.OnChannel.AddListener (HandleChannelsButton);
            BlockingLayer.GetComponent<Button> ().onClick.AddListener (HandleBlockingLayerTap);

            SyncViewState ();
        }

        protected override void OnDisable ()
        {
            base.OnDisable ();

            InputView.OnSend.RemoveListener (HandleSend);
            ChannelsView.OnChannelSelected.RemoveListener (HandleChannelSelected);
            HeaderView.OnChannel.RemoveListener (HandleChannelsButton);
            BlockingLayer.GetComponent<Button> ().onClick.RemoveListener (HandleBlockingLayerTap);
        }

        protected override void OnConnectionStateChange (bool isConnected)
        {
            base.OnConnectionStateChange (isConnected);

            FizzLogger.D ("OnConnectionStateChange isConnected " + isConnected);
        }

        private void HandleSend (string text)
        {
            if (string.IsNullOrEmpty (text)) return;

            MessagesView.AddNewMessage (text);
        }

        private void HandleChannelSelected (FizzChannel channel)
        {
            if (channel != null)
            {
                MessagesView.SetChannel (channel.Id);
                HeaderView.SetTitleText (channel.Name);
            }
            else
            {
                MessagesView.Reset ();
            }
        }

        private void HandleBlockingLayerTap ()
        { 
            transform.GetChild(0).GetComponent<RectTransform> ().anchoredPosition = Vector2.zero;
            SetBlockingLayerActive (false);
        }

        private void HandleChannelsButton ()
        {
            transform.GetChild (0).GetComponent<RectTransform> ().anchoredPosition = Vector2.right * 400;
            SetBlockingLayerActive (true);
        }

        private void HandleHeaderViewVisibility ()
        {
            HeaderView.SetVisibility (_showHeaderView);
            HeaderView.RectTransform.offsetMax = _showHeaderView ? Vector2.down * 80 : Vector2.zero;
        }

        private void UpdateChannelListVisibility ()
        {
            ChannelsView.SetVisibility (_showChannels);
            MessagesView.RectTransform.offsetMax = _showChannels ? Vector2.down * 80 : Vector2.zero;
        }

        private void UpdateInputViewVisibility ()
        {
            InputView.gameObject.SetActive (_showInputView);
            MessagesView.RectTransform.offsetMin = _showInputView ? Vector2.up * 80 : Vector2.zero;
        }

        private void SyncViewState ()
        {
            HandleChannelSelected (ChannelsView.CurrentSelectedChannel);

            InputView.Reset ();
        }

        private void SetBlockingLayerActive (bool active)
        {
            BlockingLayer.gameObject.SetActive (active);
        }
    }
}