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
    public class FizzChannelListView : FizzBaseComponent
    {
        /// <summary>
		/// The background.
		/// </summary>
		[SerializeField] Image Background;
        /// <summary>
        /// The button prefab.
        /// </summary>
        [SerializeField] FizzChannelListItem ChannelItemPrefab;
        /// <summary>
        /// The buttons contianer.
        /// </summary>
        [SerializeField] HorizontalLayoutGroup ChannelsContainer;
        /// <summary>
        /// The on bar item pressed.
        /// </summary>
        public FizzChannelItemSelectedEvent OnChannelSelected;

        [System.Serializable]
        public class FizzChannelItemSelectedEvent : UnityEvent<FizzChannelMeta>
        { }

        private Dictionary<string, FizzChannelListItem> _channels;
        private int _reloadContainerLayout = -1;
        private FizzChannelMeta _currentChannelData;
        private VerticalLayoutGroup _backgroundVLG;

        private bool _isInitialized = false;

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

        #region Public Methods

        public void SetVisibility(bool visible)
        {
            CanvasGroup cg = gameObject.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = visible ? 1 : 0;
            }
        }

        public void Setup(bool visible)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            _reloadContainerLayout = 0;

            gameObject.GetComponent<CanvasGroup>().alpha = visible ? 1 : 0;
        }

        public void AddChannel(FizzChannelMeta channelMeta, bool select = false)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (!_channels.ContainsKey(channelMeta.Id))
            {
                ResetLayout();
                _AddButton(channelMeta);
                _reloadContainerLayout = 1;
            }

            if (_channels.ContainsKey(channelMeta.Id) && select)
            {
                TabButtonPressed(channelMeta);
            }
        }

        public void RemoveChannel(FizzChannelMeta channelMeta)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            ResetLayout();
            _RemoveButton(channelMeta);
            _reloadContainerLayout = 1;
        }

        public void SetChannel(FizzChannelMeta channelMeta)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (_channels.ContainsKey(channelMeta.Id))
            {
                TabButtonPressed(channelMeta);
            }
            else
            {
                FizzLogger.W("FizzChatView: Unable to set channel, add channel first");
            }
        }

        public FizzChannelMeta GetChannel()
        {
            return _currentChannelData;
        }

        public void Reset()
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            _currentChannelData = null;
            if (_channels == null)
                return;
            foreach (KeyValuePair<string, FizzChannelListItem> pair in _channels)
            {
                FizzChannelListItem button = pair.Value;
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            _channels.Clear();
        }

        #endregion

        #region Private Method(s)

        private void Initialize()
        {
            if (_isInitialized)
                return;

            _currentChannelData = null;
            _channels = new Dictionary<string, FizzChannelListItem>();
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
                foreach (KeyValuePair<string, FizzChannelListItem> pair in _channels)
                {
                    FizzChannelListItem barButton = pair.Value;
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

            if (_currentChannelData == null && _channels.Count > 0)
            {
                TabButtonPressed(_channels.Values.First().GetChannel());
            }
        }

        private void TabButtonPressed(FizzChannelMeta data)
        {
            if (_currentChannelData != null)
            {
                if (_currentChannelData.Id.Equals(data.Id))
                    return;

                FizzChannelListItem currentButton = _channels[_currentChannelData.Id];
                currentButton.SetSelected(false);
            }

            _currentChannelData = data;
            if (OnChannelSelected != null)
                OnChannelSelected.Invoke(data);

            FizzChannelListItem barButton = _channels[data.Id];
            barButton.SetSelected(true);
        }

        private void ResetLayout()
        {
            foreach (KeyValuePair<string, FizzChannelListItem> pair in _channels)
            {
                FizzChannelListItem barButton = pair.Value;
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

        private bool _AddButton(FizzChannelMeta _item)
        {
            bool _added = false;
            if (!_channels.ContainsKey(_item.Id))
            {
                FizzChannelListItem _button = Instantiate(ChannelItemPrefab);
                _button.gameObject.SetActive(true);
                _button.transform.SetParent(ChannelsContainer.transform, false);
                _button.transform.localScale = Vector3.one;
                _button.SetChannel(_item, TabButtonPressed);
                _channels.Add(_item.Id, _button);
                _added = true;
            }
            return _added;
        }

        private bool _RemoveButton(FizzChannelMeta _item)
        {
            bool _removed = false;
            if (!string.IsNullOrEmpty(_item.Id) && _channels.ContainsKey(_item.Id))
            {
                if (_currentChannelData != null && _currentChannelData.Id.Equals(_item.Id))
                {
                    _currentChannelData = null;
                }

                Destroy(_channels[_item.Id].gameObject);
                _channels.Remove(_item.Id);
                _removed = true;
                _reloadContainerLayout = 1;

            }
            return _removed;
        }
        #endregion
    }
}