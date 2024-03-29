﻿using Fizz.Common;
using Fizz.UI.Core;
using Fizz.UI.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Fizz.UI
{
    /// <summary>
    /// FizzChatView is the core UI component which contains channel list, messages and input. 
    /// Channels are added and removed from it in order to show or hide them from view. But note
    /// that a channel should be Subscribed first by using FizzService. It can be configured to 
    /// show/hide channel list and input. It can be added to any container like, Tabs and Po-pup etc.
    /// </summary>
    public class FizzChatView : FizzBaseComponent
    {
        [SerializeField] private FizzHeaderView HeaderView = null;
        [SerializeField] private FizzChannelsView ChannelsView = null;
        [SerializeField] private FizzMessagesView MessagesView = null;
        [SerializeField] private FizzInputView InputView = null;

        private bool _showChannels = true;
        private bool _showGroups = false;
        private bool _showInputView = true;
        private bool _showHeaderView = true;
        private bool _showTranslation = true;
        private bool _showCloseButton = true;
        private bool _enableFetchHistory = true;

        private bool _isChannelListVisible = false;

        public UnityEvent onClose
        {
            get { return HeaderView.OnClose; }
            set { HeaderView.OnClose = value; }
        }

        /// <summary>
        /// Show/Hide Channel list
        /// </summary>
        public bool ShowChannelsButton
        {
            get { return _showChannels; }
            set
            {
                _showChannels = value;
                HeaderView.SetChannelButtonVisibility(_showChannels);
            }
        }

        /// <summary>
        /// Show/Hide Group list
        /// </summary>
        public bool ShowGroups
        {
            get { return _showGroups; }
            set
            {
                _showGroups = value;
                UpdateGroupsVisibility();
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
                HandleHeaderViewVisibility();
            }
        }

        public bool ShowCloseButton
        {
            get { return _showCloseButton; }
            set
            {
                _showCloseButton = value;
                HeaderView.SetCloseButtonVisibility(_showCloseButton);
            }
        }

        /// <summary>
        /// Add Channel to UI. Note that Channel should be added to FizzService first.
        /// </summary>
        /// <param name="channelId">Id of channel to be added in UI</param>
        /// <param name="select">Select channel</param>
        public void AddChannel(string channelId, bool select = false)
        {
            ChannelsView.AddChannel(channelId, select);
        }

        /// <summary>
        /// Remove channel from UI
        /// </summary>
        /// <param name="channelId">Id of channel to be removed from UI</param>
        public void RemoveChannel(string channelId)
        {
            ChannelsView.RemoveChannel(channelId);
        }

        /// <summary>
        /// Set the current channel which is already added to UI.
        /// </summary>
        /// <param name="channelId">Id of the Channel to select</param>
        public bool SetCurrentChannel(string channelId)
        {
            return ChannelsView.SetChannel(channelId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public void SetCustomDataViewSource(IFizzCustomMessageCellViewDataSource source)
        {
            MessagesView.SetCustomDataSource(source);
        }

        /// <summary>
        /// Reset all the content which includes channel list, messages and input. 
        /// </summary>
        public void Reset()
        {
            HeaderView.Reset();
            ChannelsView.Reset();
            MessagesView.Reset();
            InputView.Reset();

            ResetChannelsVisibility();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            InputView.OnSendMessage.AddListener(HandleSendMessage);
            InputView.OnSendData.AddListener(HandleSendData);
            ChannelsView.OnChannelSelected.AddListener(HandleChannelSelected);
            HeaderView.OnChannel.AddListener(HandleChannelsButton);
            HeaderView.OnClose.AddListener(HandleCloseButton);

            FizzService.Instance.GroupRepository.OnGroupAdded += OnGroupAdded;
            FizzService.Instance.GroupRepository.OnGroupUpdated += OnGroupUpdated;
            FizzService.Instance.GroupRepository.OnGroupRemoved += OnGroupRemoved;
            FizzService.Instance.GroupRepository.OnGroupMembersUpdated += OnGroupMembersUpdated;

            SyncViewState();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            InputView.OnSendMessage.RemoveListener(HandleSendMessage);
            InputView.OnSendData.AddListener(HandleSendData);
            ChannelsView.OnChannelSelected.RemoveListener(HandleChannelSelected);
            HeaderView.OnChannel.RemoveListener(HandleChannelsButton);
            HeaderView.OnClose.RemoveListener(HandleCloseButton);

            FizzService.Instance.GroupRepository.OnGroupAdded -= OnGroupAdded;
            FizzService.Instance.GroupRepository.OnGroupUpdated -= OnGroupUpdated;
            FizzService.Instance.GroupRepository.OnGroupRemoved -= OnGroupRemoved;
            FizzService.Instance.GroupRepository.OnGroupMembersUpdated -= OnGroupMembersUpdated;
        }

        private void HandleSendMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            FizzChannelModel channel = ChannelsView.CurrentSelectedChannel;

            if (channel == null)
                return;

            try
            {
                long now = FizzUtils.Now();
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { FizzMessageCellModel.KEY_CLIENT_ID, now + "" }
                };

                FizzMessageCellModel model = new FizzMessageCellModel(
                    now,
                    FizzService.Instance.UserId,
                    FizzService.Instance.UserName,
                    channel.Id,
                    message,
                    data,
                    null,
                    now)
                {
                    DeliveryState = FizzChatCellDeliveryState.Pending
                };

                MessagesView.AddMessage(model);

                channel.PublishMessage(
                    FizzService.Instance.UserName,
                    message,
                    GetLanguageCode(),
                    data,
                    FizzService.Instance.IsTranslationEnabled,
                    exception =>
                    {
                        if (exception == null)
                        {
                            if (model.DeliveryState != FizzChatCellDeliveryState.Published)
                            {
                                model.DeliveryState = FizzChatCellDeliveryState.Sent;
                            }
                            MessagesView.AddMessage(model);

                            FizzService.Instance.Client.Ingestion.TextMessageSent(channel.Id, message, FizzService.Instance.UserName);
                        }
                    });
            }
            catch
            {
                FizzLogger.E("Something went wrong while calling PublishMessage of FizzService.");
            }
        }

        private void HandleSendData(Dictionary<string, string> data)
        {
            if (data == null) return;

            if (ChannelsView.CurrentSelectedChannel == null)
                return;

            FizzChannelModel channel = ChannelsView.CurrentSelectedChannel;

            try
            {
                long now = FizzUtils.Now();
                data.Add(FizzMessageCellModel.KEY_CLIENT_ID, now + "");

                FizzMessageCellModel model = new FizzMessageCellModel(
                    now,
                    FizzService.Instance.UserId,
                    FizzService.Instance.UserName,
                    channel.Id,
                    string.Empty,
                    data,
                    null,
                    now)
                {
                    DeliveryState = FizzChatCellDeliveryState.Pending
                };

                MessagesView.AddMessage(model);

                channel.PublishMessage(
                    FizzService.Instance.UserName,
                    string.Empty,
                    string.Empty,
                    data,
                    FizzService.Instance.IsTranslationEnabled,
                    exception =>
                    {
                        if (exception == null)
                        {
                            if (model.DeliveryState != FizzChatCellDeliveryState.Published)
                            {
                                model.DeliveryState = FizzChatCellDeliveryState.Sent;
                            }
                            MessagesView.AddMessage(model);


                            string dataStr = Utils.GetDictionaryToString(data, FizzMessageCellModel.KEY_CLIENT_ID);
                            FizzService.Instance.Client.Ingestion.TextMessageSent(channel.Id, dataStr, FizzService.Instance.UserName);
                        }
                    });
            }
            catch
            {
                FizzLogger.E("Something went wrong while calling PublishMessage of FizzService.");
            }
        }

        private void HandleChannelSelected(FizzChannelModel channel)
        {
            if (channel != null)
            {
                MessagesView.SetChannel(channel.Id);
                HeaderView.SetTitleText(channel.Name);

                ShowInput = true;
                if (channel.GetType() == typeof(FizzGroupChannelModel))
                {
                    FizzGroupChannelModel groupChannel = (FizzGroupChannelModel)channel;
                    if (FizzService.Instance.GroupRepository.GroupInvites.ContainsKey(groupChannel.GroupId))
                    {
                        ShowInput = false;
                    }
                }

                if (_isChannelListVisible) HandleChannelsButton();
            }
            else
            {
                MessagesView.Reset();
            }
        }

        private void HandleChannelsButton()
        {
            _isChannelListVisible = !_isChannelListVisible;
            Tween.TweenRect.Begin(transform.GetChild(0).GetComponent<RectTransform>(),
                _isChannelListVisible ? Vector2.zero : Vector2.right * ChannelsView.RectTransform.rect.width,
                _isChannelListVisible ? Vector2.right * ChannelsView.RectTransform.rect.width : Vector2.zero,
                0.25f, 0);
        }

        private void HandleCloseButton()
        {
            ResetChannelsVisibility();
        }

        private void HandleHeaderViewVisibility()
        {
            HeaderView.SetVisibility(_showHeaderView);
            MessagesView.RectTransform.offsetMax = _showHeaderView ? Vector2.down * HeaderView.RectTransform.rect.height : Vector2.zero;
        }

        private void UpdateChannelListVisibility()
        {
            ChannelsView.SetVisibility(_showChannels);
            MessagesView.RectTransform.offsetMax = _showChannels ? Vector2.down * HeaderView.RectTransform.rect.height : Vector2.zero;
        }

        private void UpdateInputViewVisibility()
        {
            InputView.gameObject.SetActive(_showInputView);
            MessagesView.RectTransform.offsetMin = _showInputView ? Vector2.up * InputView.RectTransform.rect.height : Vector2.zero;
        }

        private void SyncViewState()
        {
            HandleChannelSelected(ChannelsView.CurrentSelectedChannel);

            InputView.Reset();
        }

        private void ResetChannelsVisibility()
        {
            _isChannelListVisible = false;
            transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        void UpdateGroupsVisibility()
        {
            foreach (FizzGroupModel group in FizzService.Instance.GroupRepository.Groups)
            {
                if (_showGroups)
                {
                    ChannelsView.AddGroup(group);
                }
                else
                {
                    ChannelsView.RemoveGroup(group);
                }
            }
        }

        void OnGroupAdded(FizzGroupModel group)
        {
            if (!_showGroups)
                return;

            ChannelsView.AddGroup(group);
        }

        void OnGroupUpdated(FizzGroupModel group)
        {
            if (!_showGroups)
                return;

            ChannelsView.UpdateGroup(group);
            if (ChannelsView.CurrentSelectedChannel == group.Channel)
            {
                HeaderView.SetTitleText(group.Channel.Name);
            }
        }

        void OnGroupRemoved(FizzGroupModel group)
        {
            if (!_showGroups)
                return;

            ChannelsView.RemoveGroup(group);
        }

        void OnGroupMembersUpdated(FizzGroupModel group)
        {
            if (!_showGroups)
                return;

            if (group.Channel.Id == ChannelsView.CurrentSelectedChannel.Id)
            {
                if (!FizzService.Instance.GroupRepository.GroupInvites.ContainsKey(group.Id))
                {
                    ShowInput = true;
                }
            }
        }

        private string GetLanguageCode()
        {
            string langCode = FizzLanguageCodes.English.Code;
            try
            {
                langCode = FizzService.Instance.Language.Code;
            }
            catch (Exception)
            {
                FizzLogger.E("Unable to get LanguageCode");
            }

            return langCode;
        }
    }
}