using Fizz.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI
{
    public class FizzChannelGroupView : FizzBaseComponent
    {
        [SerializeField] Text TitleLabel;

        public int ChannelCount { get; set; }

        public void SetData (string title)
        {
            if (string.IsNullOrEmpty (title))
                TitleLabel.gameObject.SetActive (false);

            TitleLabel.text = title;

            ChannelCount = 0;
        }
    }
}
