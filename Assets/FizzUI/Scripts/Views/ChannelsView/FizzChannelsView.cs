using System;
using System.Collections.Generic;
using System.Linq;
using Fizz.Common;
using Fizz.UI.Core;
using Fizz.UI.Model;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Fizz.UI
{
    //Fizz Channel List
    public class FizzChannelsView : FizzBaseComponent
    {
        /// <summary>
		/// The background.
		/// </summary>
		[SerializeField] Image Background;
        /// <summary>
        /// The button prefab.
        /// </summary>
        [SerializeField] FizzChannelView ChannelItemPrefab;
        /// <summary>
        /// The buttons contianer.
        /// </summary>
        [SerializeField] HorizontalLayoutGroup ChannelsContainer;
        /// <summary>
        /// The on bar item pressed.
        /// </summary>
        public FizzChannelItemSelectedEvent OnChannelSelected;

        [Serializable]
        public class FizzChannelItemSelectedEvent : UnityEvent<FizzChannel>
        { }

        private List<string> _channelWatchList;
        private Dictionary<string, FizzChannelView> _channels;
        private int _reloadContainerLayout = -1;
        private VerticalLayoutGroup _backgroundVLG;

        private bool _isInitialized = false;

        public FizzChannel CurrentSelectedChannel { get; private set; }

        void Awake()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }

        void LateUpdate()
        {
            if (_reloadContainerLayout != -1)
            {
                if (_reloadContainerLayout < 1)
                {
                    UpdateLayout();
                }

                _reloadContainerLayout--;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            try
            {
                FizzService.Instance.OnChannelSubscribed += HandleOnChannelSubscribe;
                FizzService.Instance.OnChannelUnsubscribed += HandleOnChannelUnsubscribe;
            }
            catch
            {
                FizzLogger.E("Unable to call FizzService.");
            }

            SyncViewState();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            try
            {
                FizzService.Instance.OnChannelSubscribed -= HandleOnChannelSubscribe;
                FizzService.Instance.OnChannelUnsubscribed -= HandleOnChannelUnsubscribe;
            }
            catch
            {
                FizzLogger.E("Unable to call FizzService.");
            }
        }

        #region Public Methods

        public void SetVisibility(bool visible)
        {
            CanvasGroup cg = gameObject.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = visible ? 1 : 0;
            }
        }

        public void AddChannel(string channelId, bool select = false)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            FizzChannel channel = GetChannelById(channelId);

            if (channel == null)
            {
                if (!_channelWatchList.Contains(channelId)) _channelWatchList.Add(channelId);
                FizzLogger.W("Channel not found, please add channel [" + channelId + "] to FizzService first.");
                return;
            }

            if (!_channels.ContainsKey(channel.Id))
            {
                _AddButton(channel);
            }

            if (_channels.ContainsKey(channel.Id) && select)
            {
                TabButtonPressed(channel);
            }
        }

        public void RemoveChannel(string channelId)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (!_isInitialized)
            {
                Initialize();
            }

            _channelWatchList.Remove(channelId);

            _RemoveButton(channelId);
        }

        public bool SetChannel(string channelId)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            FizzChannel channel = GetChannelById(channelId);

            if (channel == null)
                return false;

            if (_channels.ContainsKey(channel.Id))
            {
                TabButtonPressed(channel);
                return true;
            }
            else
            {
                FizzLogger.W("FizzChatView: Unable to set channel, add channel first");
                return false;
            }
        }

        public void Reset()
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            CurrentSelectedChannel = null;

            foreach (KeyValuePair<string, FizzChannelView> pair in _channels)
            {
                FizzChannelView button = pair.Value;
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            _channels.Clear();
            _channelWatchList.Clear();
        }

        #endregion

        #region Private Method(s)

        private void Initialize()
        {
            if (_isInitialized)
                return;

            CurrentSelectedChannel = null;
            _channels = new Dictionary<string, FizzChannelView>();
            _channelWatchList = new List<string>();
            _backgroundVLG = ChannelItemPrefab.GetComponentInChildren<VerticalLayoutGroup>();
            _isInitialized = true;
        }

        private void UpdateLayout()
        {
            Rect barSize = transform.GetComponent<RectTransform>().rect;
            Rect containerSize = ChannelsContainer.GetComponent<RectTransform>().rect;
            if (containerSize.width > barSize.width)
            {
                ResetLayout();
            }
            else
            {
                float elementSize = barSize.width / _channels.Count;
                foreach (KeyValuePair<string, FizzChannelView> pair in _channels)
                {
                    FizzChannelView barButton = pair.Value;
                    LayoutElement layoutElement = barButton.GetComponent<LayoutElement>();
                    layoutElement.preferredWidth = elementSize;
                }

                RectTransform cRect = ChannelsContainer.GetComponent<RectTransform>();
                cRect.anchorMin = Vector2.zero;
                cRect.anchorMax = Vector2.one;
                cRect.sizeDelta = Vector2.zero;
                cRect.offsetMin = Vector2.zero;
                cRect.offsetMax = Vector2.zero;

                ChannelsContainer.childForceExpandHeight = true;
                ChannelsContainer.childForceExpandWidth = true;
                ChannelsContainer.spacing = 0;
                ChannelsContainer.padding = new RectOffset(0, 0, 0, 0);

                _backgroundVLG.padding = new RectOffset(0, 0, 0, 0);

                ChannelsContainer.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                ChannelsContainer.SetLayoutHorizontal();
            }

            if (CurrentSelectedChannel == null && _channels.Count > 0)
            {
                TabButtonPressed(_channels.Values.First().GetChannel());
            }
        }

        private void TabButtonPressed(FizzChannel data)
        {
            if (CurrentSelectedChannel != null)
            {
                if (CurrentSelectedChannel.Id.Equals(data.Id))
                {
                    if (OnChannelSelected != null)
                        OnChannelSelected.Invoke(data);
                    return;
                }

                FizzChannelView currentButton = _channels[CurrentSelectedChannel.Id];
                currentButton.SetSelected(false);
            }

            CurrentSelectedChannel = data;
            if (OnChannelSelected != null)
                OnChannelSelected.Invoke(data);

            FizzChannelView barButton = _channels[data.Id];
            barButton.SetSelected(true);
        }

        private void ResetLayout()
        {
            foreach (KeyValuePair<string, FizzChannelView> pair in _channels)
            {
                FizzChannelView barButton = pair.Value;
                LayoutElement layoutElement = barButton.GetComponent<LayoutElement>();
                layoutElement.preferredWidth = -1;
            }

            RectTransform cRect = ChannelsContainer.GetComponent<RectTransform>();
            cRect.anchorMin = Vector2.zero;
            cRect.anchorMax = new Vector2(0, 1);
            cRect.sizeDelta = Vector2.zero;
            cRect.offsetMin = Vector2.zero;
            cRect.offsetMax = Vector2.zero;

            ChannelsContainer.childForceExpandHeight = true;
            ChannelsContainer.childForceExpandWidth = false;
            ChannelsContainer.spacing = 40;
            ChannelsContainer.padding = new RectOffset(40, 40, 0, 0);

            _backgroundVLG.padding = new RectOffset(0, 0, 0, 0);

            ChannelsContainer.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            ChannelsContainer.SetLayoutHorizontal();
        }

        private bool _AddButton(FizzChannel _item)
        {
            if (_item == null) return false;

            bool _added = false;
            if (!_channels.ContainsKey(_item.Id))
            {
                FizzChannelView _button = Instantiate(ChannelItemPrefab);
                _button.gameObject.SetActive(true);
                _button.transform.SetParent(ChannelsContainer.transform, false);
                _button.transform.localScale = Vector3.one;
                _button.SetChannel(_item, TabButtonPressed);
                _channels.Add(_item.Id, _button);
                _added = true;

                ResetLayout();
                _reloadContainerLayout = 1;
            }
            return _added;
        }

        private bool _RemoveButton(string channelId)
        {
            bool _removed = false;
            if (!string.IsNullOrEmpty(channelId) && _channels.ContainsKey(channelId))
            {
                if (CurrentSelectedChannel != null && CurrentSelectedChannel.Id.Equals(channelId))
                {
                    CurrentSelectedChannel = null;
                }

                Destroy(_channels[channelId].gameObject);
                _channels.Remove(channelId);
                _removed = true;
                ResetLayout();
                _reloadContainerLayout = 1;

            }
            return _removed;
        }

        private void SyncViewState()
        {
            try
            {
                foreach (FizzChannel channel in FizzService.Instance.Channels)
                {
                    if (_channelWatchList.Contains(channel.Id) && !_channels.ContainsKey(channel.Id))
                    {
                        _AddButton(channel);
                        _channelWatchList.Remove(channel.Id);
                    }
                }

                foreach (string channelId in _channels.Keys)
                {
                    if (FizzService.Instance.GetChannel(channelId) == null)
                    {
                        _RemoveButton(channelId);
                    }
                }
            }
            catch
            {
                FizzLogger.E("Something went wrong while calling Channels of FizzService.");
            }
        }

        private void HandleOnChannelSubscribe(string channelId)
        {
            if (string.IsNullOrEmpty(channelId))
                return;

            if (_channels.ContainsKey(channelId))
                return;

            if (_channelWatchList.Contains(channelId))
            {
                _AddButton(GetChannelById(channelId));
                _channelWatchList.Remove(channelId);
            }
        }

        private void HandleOnChannelUnsubscribe(string channelId)
        {
            if (string.IsNullOrEmpty(channelId))
                return;

            if (!_channels.ContainsKey(channelId))
                return;

            _RemoveButton(channelId);
        }

        FizzChannel GetChannelById(string channelId)
        {
            try
            {
                return FizzService.Instance.GetChannel(channelId);
            }
            catch (Exception)
            {
                FizzLogger.W("ChannelList unable to get channel with id " + channelId);
            }
            return null;
        }

        #endregion
    }
}