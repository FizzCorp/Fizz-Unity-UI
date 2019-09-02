using Fizz.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fizz.Demo
{
    /// <summary>
    /// FullScreensample is designed to demonstrate how FizzChatView can be used to display a full screen chat 
    /// including channel list and an input bar.
    /// </summary>
    public class FullScreenSample : MonoBehaviour
    {
        [SerializeField] FizzChatView chatView;

        // Global Channel Id
        private readonly string globalChannelId = "global-channel";
        // Local Channel Id
        private readonly string localChannelId = "local-channel";

        private void Awake()
        {
            SetupChatView();
        }

        private void OnEnable()
        {
            AddChannels();
        }

        private void OnDisable()
        {
            RemoveChannels();
        }

        private void SetupChatView()
        {
            // Show channel list
            chatView.ShowChannelsButton = true;

            // Show header view
            chatView.ShowHeaderView = true;

            // Show close button
            chatView.ShowCloseButton = true;

            // Show chat input 
            chatView.ShowInput = true;

            // Allow fetching hisgtory
            chatView.EnableFetchHistory = true;

            // Show messaage translations
            chatView.ShowMessageTranslation = true;

            chatView.onClose.AddListener (() => gameObject.SetActive (false));
        }

        private void AddChannels()
        {
            // Add and Set a channel at same time
            chatView.AddChannel(globalChannelId, true);
            // Add channel
            chatView.AddChannel(localChannelId);
        }

        private void RemoveChannels()
        {
            // Remove channel from chatview
            chatView.RemoveChannel(globalChannelId);
            chatView.RemoveChannel(localChannelId);
        }

        public void HandleClose()
        {
            SceneManager.LoadScene("SceneSelector");
        }
    }
}