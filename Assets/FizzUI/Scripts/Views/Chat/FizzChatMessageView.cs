using System;
using System.Collections.Generic;
using Fizz.Chat;
using Fizz.Common;
using Fizz.UI.Components;
using Fizz.UI.Components.Extentions;
using Fizz.UI.Core;
using Fizz.UI.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI
{
    /// <summary>
    /// User interface chat view.
    /// </summary>
    public class FizzChatMessageView : FizzBaseComponent, ICustomScrollRectDataSource
    {
        /// <summary>
        /// The background image.
        /// </summary>
        [SerializeField] Image BackgroundImage;
        /// <summary>
        /// The table view.
        /// </summary>
        [SerializeField] CustomScrollRect ScrollRect;
        /// <summary>
        /// The options menu.
        /// </summary>
        [SerializeField] FizzTooltipComponent OptionsMenu;
        /// <summary>
        /// Spinner to show when fetching history
        /// </summary>
        [SerializeField] FizzSpinnerComponent Spinner;
        /// <summary>
        /// Scroll Indicator button
        /// </summary>
        [SerializeField] Button ScrollIndicator;

        public bool EnableHistoryFetch { get; set; } = false;

        public bool ShowMessageTranslation { get; set; } = false;

        void Awake()
        {
            if (!_isInitialized)
                Initialize();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            try
            {
                FizzService.Instance.OnChannelMessagePublish += OnChannelMessage;
                FizzService.Instance.OnChannelMessageDelete += OnChannelMessageDeleted;
                FizzService.Instance.OnChannelMessageUpdate += OnChannelMessageUpdated;
                FizzService.Instance.OnChannelMessagesAvailable += OnChannelHistoryUpdated;
            }
            catch { }

            ScrollIndicator.onClick.AddListener(OnScrollIndicator);

            ScrollRect.onValueChanged.AddListener(OnScollValueChanged);
            ScrollRect.onPullToRefresh.AddListener(OnPullToRefresh);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            try
            {
                FizzService.Instance.OnChannelMessagePublish -= OnChannelMessage;
                FizzService.Instance.OnChannelMessageDelete -= OnChannelMessageDeleted;
                FizzService.Instance.OnChannelMessageUpdate -= OnChannelMessageUpdated;
                FizzService.Instance.OnChannelMessagesAvailable -= OnChannelHistoryUpdated;
            }
            catch { }

            ScrollIndicator.onClick.RemoveListener(OnScrollIndicator);

            ScrollRect.onValueChanged.RemoveListener(OnScollValueChanged);
            ScrollRect.onPullToRefresh.RemoveListener(OnPullToRefresh);
        }

        void LateUpdate()
        {
            if (_isDirty) ScrollRect.RefreshContent();
            if (!_userScroll && _resetScroll && _chatCellModelList.Count > 0) ScrollRect.GoToScrollItem(_chatCellModelList.Count - 1);
            _isDirty = false;
            _resetScroll = false;
        }

        #region Public Methods

        public void Setup(bool enableFetchHistory, bool showTranslation)
        {
            EnableHistoryFetch = enableFetchHistory;
            ShowMessageTranslation = showTranslation;
        }

        public void SetChannel(string channelId)
        {
            Reset();

            if (!_isInitialized)
                Initialize();

            _channel = GetChannelById(channelId);

            if (_channel != null)
            {
                LoadChatAsync(true);
            }

            try
            {
                _userId = FizzService.Instance.UserId;
            }
            catch { }
        }

        public void SetCustomDataSource(IFizzChatViewCustomDataSource source)
        {
            _chatDataSource = source;
        }

        public void AddNewMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            if (_channel == null)
                return;

            try
            {
                long now = FizzUtils.Now();
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { FizzChatCellModel.KEY_CLIENT_ID, now + "" }
                };

                FizzChatCellModel model = new FizzChatCellModel(
                    now,
                    FizzService.Instance.UserId,
                    FizzService.Instance.UserName,
                    _channel.Id,
                    message,
                    data,
                    null,
                    now)
                {
                    DeliveryState = FizzChatCellDeliveryState.Pending
                };

                AddAction(model);

                FizzService.Instance.Client.Chat.PublishMessage(
                    _channel.Id,
                    FizzService.Instance.UserName,
                    message,
                    data,
                    FizzService.Instance.IsTranslationEnabled,
                    _channel.Meta.FilterContent,
                    _channel.Meta.PersistMessages,
                    exception =>
                    {
                        if (exception == null)
                        {
                            model.DeliveryState = FizzChatCellDeliveryState.Sent;
                            AddAction(model);
                            
                            FizzService.Instance.Client.Ingestion.TextMessageSent(_channel.Id, message, FizzService.Instance.UserName);
                        }
                    });
            }
            catch { }
        }

        public void Reset()
        {
            if (!_isInitialized)
                Initialize();

            _channel = null;
            Spinner.HideSpinner();

            if (_chatCellModelList != null)
            {
                _chatCellModelList.Clear();
                ScrollRect.RefreshContent();
            }

            if (_actionsLookUpDict != null)
            {
                _actionsLookUpDict.Clear();
            }

            _isFetchHistoryInProgress = false;
        }

        #endregion

        #region Private Methods

        private void Initialize()
        {
            _chatCellModelList = new List<FizzChatCellModel>();
            _actionsLookUpDict = new Dictionary<string, FizzChatCellModel>();
            ScrollRect.pullDirection = CustomScrollRect.PullDirection.Up;
            ScrollRect.Initialize(this);
            ScrollRect.RebuildContent();
            LoadCellPrefabs();

            _isInitialized = true;
        }

        private void LoadCellPrefabs()
        {
            otherCellView = Utils.LoadPrefabs<FizzChatOtherCellView>("Cells/OtherCell");
            otherRepeatCellView = Utils.LoadPrefabs<FizzChatOtherRepeatCellView>("Cells/OtherRepeatCell");
            userCellView = Utils.LoadPrefabs<FizzChatUserCellView>("Cells/UserCell");
            dateHeaderCellView = Utils.LoadPrefabs<FizzChatDateHeaderCellView>("Cells/DateHeaderCell");
        }

        private void LoadChatAsync(bool scrollDown)
        {
            LoadChat(scrollDown);

            _isDirty = true;
            _resetScroll = true;
        }

        private void LoadChat(bool scrollDown)
        {
            if (scrollDown) ResetScrollIndicator();

            _lastAction = null;
            ResetOptionsMenu();
            LoadMessages();
        }

        private void LoadMessages()
        {
            _chatCellModelList.Clear();
            _actionsLookUpDict.Clear();

            IList<FizzChannelMessage> actionsList = _channel.Messages;

            for (int index = 0; index < actionsList.Count; index++)
            {
                FizzChannelMessage action = actionsList[index];

                CheckForDateHeader(action);

                FizzChatCellModel model = GetChatCellModelFromAction(action);
                model.DeliveryState = FizzChatCellDeliveryState.Published;

                _chatCellModelList.Add(model);
                AddActionInLookup(model);
            }
        }

        private void CheckForDateHeader(FizzChannelMessage action)
        {
            bool shouldAddHeaderModel = false;
            if (_lastAction == null)
            {
                shouldAddHeaderModel = true;
                _lastAction = action;
            }
            else
            {
                DateTime last = Utils.GetDateTimeToUnixTime(_lastAction.Created);
                last = last.Date;
                DateTime current = Utils.GetDateTimeToUnixTime(action.Created);
                TimeSpan ts = current.Subtract(last);
                if (ts.Days > 0)
                {
                    shouldAddHeaderModel = true;
                }
                _lastAction = action;
            }

            if (shouldAddHeaderModel)
            {
                FizzChatCellModel model = GetChatCellModelFromAction(action);
                model.Type = FizzChatCellType.DateCell;

                _chatCellModelList.Add(model);
            }
        }

        private void OnScollValueChanged(Vector2 val)
        {
            if (OptionsMenu.isActiveAndEnabled)
            {
                OptionsMenu.gameObject.SetActive(false);
            }

            float diff = ScrollRect.ContentSize - (ScrollRect.ViewportSize + ScrollRect.content.anchoredPosition.y);
            _userScroll = (ScrollRect.ContentSize > ScrollRect.ViewportSize && diff > 50.0f);
            if (!_userScroll)
                ScrollIndicator.gameObject.SetActive(_userScroll);
        }

        private void OnPullToRefresh()
        {
            FetchRoomHistory();
        }

        private void OnTranslateToggleClicked(int row)
        {
            ScrollRect.RefreshContent();
            int lastCellIndex = _chatCellModelList.Count - 1;
            if (row == lastCellIndex)
            {
                ScrollRect.GoToScrollItem(lastCellIndex);
            }
        }

        private void OnScrollIndicator()
        {
            if (_userScroll)
            {
                ScrollRect.StopMovement();
                _userScroll = false;
                _resetScroll = true;
                ScrollIndicator.gameObject.SetActive(false);
            }
        }

        private void FetchRoomHistory()
        {
            if (_channel == null) return;
            if (EnableHistoryFetch && !_isFetchHistoryInProgress)
            {
                bool isHistoryAvailable = false;
                if (_lastAction != null && _lastAction.Id > 1)
                {
                    isHistoryAvailable = true;
                }

                if (isHistoryAvailable)
                {
                    _isFetchHistoryInProgress = true;
                    Spinner.ShowSpinner();
                    _channel.FetchHistory(() =>
                    {
                        _isFetchHistoryInProgress = false;
                        Spinner.HideSpinner();
                    });
                }
            }
        }

        private void AddAction(FizzChatCellModel model, bool groupRefresh = false, bool hardRefresh = false, bool updateOnly = false)
        {
            if (model.Type == FizzChatCellType.ChatCell
                && _channel.Id.Equals(model.To))
            {
                FizzChatCellModel existingModel = GetActionFromLookup(model);
                if (existingModel != null)
                {
                    existingModel.Update(model);

                    if (!groupRefresh)
                        _isDirty = true;
                }
                else
                {
                    if (!updateOnly)
                    {
                        CheckForDateHeader(model);
                        _chatCellModelList.Add(model);
                        AddActionInLookup(model);
                        if (hardRefresh)
                        {
                            ScrollRect.RefreshContent();
                            ScrollRect.GoToScrollItem(_chatCellModelList.Count - 1);
                        }
                        else
                        {
                            // Should refresh content
                            _isDirty = true;
                            // Should reset scroll 
                            _resetScroll = true;
                            // show scroll indicator
                            ScrollIndicator.gameObject.SetActive(_userScroll);
                        }
                    }
                }
            }
        }

        private void RemoveAction(FizzChatCellModel model)
        {
            if (model.Type == FizzChatCellType.ChatCell
                && _channel.Id.Equals(model.To))
            {
                FizzChatCellModel existingModel = GetActionFromLookup(model);
                if (existingModel != null)
                {
                    if (!string.IsNullOrEmpty(existingModel.AlternateId.ToString())
                        && _actionsLookUpDict.ContainsKey(existingModel.AlternateId.ToString()))
                    {
                        _actionsLookUpDict.Remove(existingModel.AlternateId.ToString());
                    }
                    else if (!string.IsNullOrEmpty(existingModel.Id.ToString())
                      && _actionsLookUpDict.ContainsKey(existingModel.Id.ToString()))
                    {
                        _actionsLookUpDict.Remove(existingModel.AlternateId.ToString());
                    }
                    _chatCellModelList.Remove(existingModel);

                    _isDirty = true;
                }
            }
        }


        private void ResetOptionsMenu()
        {
            OptionsMenu.gameObject.SetActive(false);
        }

        private void ResetScrollIndicator()
        {
            _userScroll = false;
            ScrollIndicator.gameObject.SetActive(false);
        }

        private FizzChatCellModel GetChatCellModelFromAction(FizzChannelMessage action)
        {
            var model = new FizzChatCellModel(action.Id, action.From, action.Nick, action.To, action.Body, action.Data, action.Translations, action.Created)
            {
                Type = FizzChatCellType.ChatCell
            };
            return model;
        }

        private void AddActionInLookup(FizzChatCellModel model)
        {
            if (model.Type == FizzChatCellType.ChatCell)
            {
                if (!string.IsNullOrEmpty(model.Id.ToString()) && !_actionsLookUpDict.ContainsKey(model.Id.ToString()))
                {
                    _actionsLookUpDict.Add(model.Id.ToString(), model);
                }
                else if (!string.IsNullOrEmpty(model.AlternateId.ToString()) && !_actionsLookUpDict.ContainsKey(model.AlternateId.ToString()))
                {
                    _actionsLookUpDict.Add(model.AlternateId.ToString(), model);
                }
            }
        }

        private FizzChatCellModel GetActionFromLookup(FizzChatCellModel action)
        {
            FizzChatCellModel model = null;
            if (!string.IsNullOrEmpty(action.AlternateId.ToString()) && _actionsLookUpDict.ContainsKey(action.AlternateId.ToString()))
            {
                _actionsLookUpDict.TryGetValue(action.AlternateId.ToString(), out model);
            }
            else if (!string.IsNullOrEmpty(action.Id.ToString()) && _actionsLookUpDict.ContainsKey(action.Id.ToString()))
            {
                _actionsLookUpDict.TryGetValue(action.Id.ToString(), out model);
            }
            return model;
        }

        private FizzChannel GetChannelById(string id)
        {
            try
            {
                return FizzService.Instance.GetChannel(id);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Room Action Event Listeners

        void OnChannelHistoryUpdated(string channelId)
        {
            if (_channel != null && _channel.Id.Equals(channelId))
            {
                _channel = GetChannelById(channelId);
                LoadChatAsync(!_isFetchHistoryInProgress);
            }
        }

        void OnChannelMessage(string channelId, FizzChannelMessage action)
        {
            if (_channel != null && _channel.Id.Equals(channelId))
            {
                FizzChatCellModel model = GetChatCellModelFromAction(action);
                model.DeliveryState = FizzChatCellDeliveryState.Published;
                AddAction(model);
            }
        }

        void OnChannelMessageUpdated(string channelId, FizzChannelMessage action)
        {
            if (_channel != null && _channel.Id.Equals(channelId))
            {
                AddAction(GetChatCellModelFromAction(action));
            }
        }

        void OnChannelMessageDeleted(string channelId, FizzChannelMessage action)
        {
            if (_channel != null && _channel.Id.Equals(channelId))
            {
                RemoveAction(GetChatCellModelFromAction(action));
            }
        }


        #endregion

        public GameObject GetListItem(int index, int itemType, GameObject obj)
        {
            if (obj == null)
            {
                switch (itemType)
                {
                    case (int)ChatCellViewType.YoursMessageAction:
                        obj = Instantiate(userCellView.gameObject);
                        break;
                    case (int)ChatCellViewType.TheirsMessageAction:
                        obj = Instantiate(otherCellView.gameObject);
                        break;
                    case (int)ChatCellViewType.TheirsRepeatMessageAction:
                        obj = Instantiate(otherRepeatCellView.gameObject);
                        break;
                    case (int)ChatCellViewType.DateHeader:
                        obj = Instantiate(dateHeaderCellView.gameObject);
                        break;
                    default:
                        obj = Instantiate(userCellView.gameObject);
                        break;
                }
            }

            FizzChatCellModel model = _chatCellModelList[index];
            if (model.Type == FizzChatCellType.ChatCell)
            {
                FizzChatCellView chatCellView = obj.GetComponent<FizzChatCellView>();
                chatCellView.rowNumber = index;
                chatCellView.SetData(model, ShowMessageTranslation);

                if (itemType == (int)ChatCellViewType.TheirsMessageAction)
                {
                    var leftCell = chatCellView as FizzChatOtherCellView;
                    leftCell.OnTranslateToggle = OnTranslateToggleClicked;
                }
                else if (itemType == (int)ChatCellViewType.TheirsRepeatMessageAction)
                {
                    var leftRepeatCell = chatCellView as FizzChatOtherRepeatCellView;
                    leftRepeatCell.OnTranslateToggle = OnTranslateToggleClicked;
                }

                if (model.Data != null)
                {
                    RectTransform customView = null;
                    if (_chatDataSource != null)
                    {
                        customView = _chatDataSource.GetCustomMessageDrawable(model);
                        if (customView != null)
                            chatCellView.SetCustomData(customView);
                    }
                }
            }
            else if (model.Type == FizzChatCellType.DateCell)
            {
                FizzChatDateHeaderCellView dateHeader = obj.GetComponent<FizzChatDateHeaderCellView>();
                dateHeader.SetData(model, ShowMessageTranslation);
            }

            return obj;
        }

        public int GetItemCount()
        {
            return _chatCellModelList.Count;
        }

        public int GetItemType(int index)
        {
            ChatCellViewType actionType;
            FizzChatCellModel model = _chatCellModelList[index];
            if (model.Type == FizzChatCellType.ChatCell)
            {

                FizzChatCellModel lastAction = null;
                FizzChatCellModel chatAction = _chatCellModelList[index];

                int lastIndex = index - 1;
                while (lastIndex > -1)
                {
                    if (_chatCellModelList[lastIndex].Type == FizzChatCellType.ChatCell)
                    {
                        lastAction = _chatCellModelList[lastIndex];
                        break;
                    }
                    lastIndex--;
                }

                string senderId = chatAction.From;

                bool ownMessage = _userId.Equals(senderId);

                actionType = ownMessage ? ChatCellViewType.YoursMessageAction : ChatCellViewType.TheirsMessageAction;

                if (!ownMessage && lastAction != null && chatAction.From.Equals(lastAction.From))
                {
                    actionType = ChatCellViewType.TheirsRepeatMessageAction;
                }
            }
            else
            {
                actionType = ChatCellViewType.DateHeader;
            }

            return (int)actionType;
        }

        private enum ChatCellViewType
        {
            YoursMessageAction,
            TheirsMessageAction,
            TheirsRepeatMessageAction,
            DateHeader
        }

        /// <summary>
        /// The left cell view.
        /// </summary>
        private FizzChatOtherCellView otherCellView;
        /// <summary>
        /// The left repeat cell view.
        /// </summary>
        private FizzChatOtherRepeatCellView otherRepeatCellView;
        /// <summary>
        /// The right cell view.
        /// </summary>
        private FizzChatUserCellView userCellView;
        /// <summary>
        /// The chat date header cell view.
        /// </summary>
        private FizzChatDateHeaderCellView dateHeaderCellView;

        /// <summary>
        /// The data.
        /// </summary>
        private FizzChannel _channel;
        /// <summary>
        /// The last action.
        /// </summary>
        private FizzChannelMessage _lastAction;
        /// <summary>
        /// The chat actions.
        /// </summary>
        private List<FizzChatCellModel> _chatCellModelList;
        /// <summary>
        /// The lookup dictionary for actions
        /// </summary>
        Dictionary<string, FizzChatCellModel> _actionsLookUpDict;
        /// <summary>
        /// The chat data source
        /// </summary>
        IFizzChatViewCustomDataSource _chatDataSource = null;

        private string _userId = string.Empty;
        private bool _isDirty = false;
        private bool _resetScroll = false;
        private bool _userScroll = false;
        private bool _isInitialized = false;
        private bool _isFetchHistoryInProgress = false;
    }
}