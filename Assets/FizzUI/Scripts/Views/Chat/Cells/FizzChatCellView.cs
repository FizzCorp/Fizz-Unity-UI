using UnityEngine;
using UnityEngine.UI;
using System;
using Fizz.UI.Core;
using Fizz.UI.Components;
using Fizz.UI.Model;

namespace Fizz.UI
{
    /// <summary>
    /// Cell view for chat messages
    /// </summary>
    public class FizzChatCellView : FizzBaseComponent
    {
        /// <summary>
        /// Chat cell background image.
        /// </summary>
        [SerializeField] protected Image BackgroundImage;
        /// <summary>
        /// Chat message label.
        /// </summary>
        [SerializeField] protected TextWithEmoji MessageLabel;
        /// <summary>
        /// Custom view node.
        /// </summary>
        [SerializeField] protected RectTransform CustomNode;
        /// <summary>
        /// Chat message time label.
        /// </summary>
        [SerializeField] protected Text TimeLabel;

        public RectTransform messageRect
        {
            get { return MessageLabel.GetComponent<RectTransform>(); }
        }

        /// <summary>
        /// Gets or sets the row number.
        /// </summary>
        /// <value>The row number.</value>
        public int rowNumber { get; set; }

        protected FizzChatCellModel _model;
        protected bool _appTranslationEnabled;

        #region Public Methods

        public virtual void SetData(FizzChatCellModel model, bool appTranslationEnabled)
        {
            _model = model;
            _appTranslationEnabled = appTranslationEnabled;

            if (TimeLabel != null)
            {
                DateTime dt = Utils.GetDateTimeToUnixTime(_model.Created);
                string timeFormat = string.Format("{0:h:mm tt}", dt);
                TimeLabel.text = timeFormat;
            }

            if (CustomNode != null)
            {
                CustomNode.DestroyChildren();
                CustomNode.gameObject.SetActive(false);
            }
        }

        public virtual void SetCustomData(RectTransform customView)
        {
            if (CustomNode != null)
            {
                CustomNode.gameObject.SetActive(true);
                CustomNode.DestroyChildren();
                if (customView != null)
                {
                    customView.SetParent(CustomNode, false);
                }
            }
        }

        #endregion
    }
}
