using System;
using Fizz.UI.Core;
using Fizz.UI.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI
{
    public class FizzChannelListItem : FizzBaseComponent
    {
        /// <summary>
		/// The m text.
		/// </summary>
		[SerializeField] Text NameLabel;
        /// <summary>
        /// The background.
        /// </summary>
        [SerializeField] Image Background;
        /// <summary>
        /// The label background.
        /// </summary>
        [SerializeField] Image LabelBackground;

        private FizzChannelMeta _meta;
        private Action<FizzChannelMeta> _onClick;

        protected override void OnEnable()
        {
            base.OnEnable();

            gameObject.GetComponent<Button>().onClick.AddListener(HandleButtonClick);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            gameObject.GetComponent<Button>().onClick.RemoveListener(HandleButtonClick);
        }

        public void SetChannel(FizzChannelMeta channel, Action<FizzChannelMeta> onClick)
        {
            _meta = channel;
            _onClick = onClick;

            NameLabel.text = _meta.Name.ToUpper();

            LabelBackground.color = Color.white;
        }

        public FizzChannelMeta GetChannel()
        {
            return _meta;
        }

        public void SetSelected(bool selected)
        {
            Color color;
            ColorUtility.TryParseHtmlString("#808080FF", out color);

            LabelBackground.color = selected ? color : Color.white;
        }

        private void HandleButtonClick()
        {
            if (_onClick != null)
            {
                _onClick.Invoke(_meta);
            }
        }
    }
}