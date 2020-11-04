using Fizz.UI.Core;
using Fizz.UI.Model;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI
{
    public class FizzChannelView : FizzBaseComponent
    {
        [SerializeField] Image BackgroundImage;
        [SerializeField] Button ActionButton;
        [SerializeField] Text NameLabel;

        private FizzChannelModel _channel;
        private Action<FizzChannelModel> _onClickAction;

        public void SetData (FizzChannelModel channel, Action<FizzChannelModel> onClick)
        {
            _channel = channel;
            _onClickAction = onClick;

            NameLabel.text = channel.Meta.Name;
        }

        public void SetSelected (bool selected)
        {
            //Color color;
            //ColorUtility.TryParseHtmlString ("#EEEEEEFF", out color);

            //BackgroundImage.color = selected ? color : Color.white;
            ActionButton.interactable = !selected;
        }

        public FizzChannelModel GetChannel ()
        {
            return _channel;
        }

        protected override void OnEnable ()
        {
            base.OnEnable ();

            ActionButton.onClick.AddListener (HandleActionButton);
        }

        protected override void OnDisable ()
        {
            base.OnDisable ();

            ActionButton.onClick.RemoveListener (HandleActionButton);
        }

        private void HandleActionButton ()
        {
            if (_onClickAction != null)
            {
                _onClickAction.Invoke (_channel);
            }
        }
    }
}