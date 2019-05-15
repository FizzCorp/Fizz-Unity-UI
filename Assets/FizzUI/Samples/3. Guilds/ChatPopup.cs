using System.Collections.Generic;
using Fizz.UI;
using UnityEngine;

namespace Fizz.Demo
{
    /// <summary>
    /// Popup with FizzChatView
    /// </summary>
    public class ChatPopup : MonoBehaviour
    {
        [SerializeField] FizzChatView chatView;

        private void Awake ()
        {
            chatView.ShowCloseButton = false;
        }

        public void Show(List<string> channels)
        {
            gameObject.SetActive(true);

            // Show channels only if list contains more than one channel
            chatView.ShowChannels = channels.Count > 1;

            for (int index = 0; index < channels.Count; index++)
            {
                string channelId = channels[index];
                if (string.IsNullOrEmpty(channelId))
                    continue;

                chatView.AddChannel(channelId);

                if (index == 0)
                    chatView.SetCurrentChannel(channelId);
            }
        }

        public void Hide()
        {
            // Reset every thing
            chatView.Reset();
            gameObject.SetActive(false);
        }
    }
}