using Fizz.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fizz.Demo
{
    /// <summary>
    /// TabViewSample is deigned to demonstrate the use of FizzChatView in a typical TabView imlementation.
    /// When one tab is enabled it add a channel to it to display messages and remove it when disabled.
    /// This sample hides the channel list becuase sample is using its own basic Tab implentation.
    /// </summary>
    public class TabViewSample : MonoBehaviour
    {
        [SerializeField] RectTransform[] tabContainers;

        // Global channel Id
        private readonly string globalChannelId = "global-channel";
        // Local channel Id
        private readonly string localChannelId = "local-channel";

        private FizzChatView globalChatView;
        private FizzChatView localChatView;

        private void Awake()
        {
            SetupChatViews();

            TabButtonPressed(0);
        }

        int _lastIndex = -1;
        public void TabButtonPressed(int index)
        {
            if (_lastIndex == index)
                return;

            if (_lastIndex > -1)
            {
                tabContainers[_lastIndex].gameObject.SetActive(false);

                // Remove channel when tab gets disabled
                if (_lastIndex == 0)
                {
                    globalChatView.RemoveChannel(globalChannelId);
                }
                else if (_lastIndex == 1)
                {
                    localChatView.RemoveChannel(localChannelId);
                }
            }

            _lastIndex = index;
            tabContainers[_lastIndex].gameObject.SetActive(true);

            // Add channel when tab gets enabled
            if (_lastIndex == 0)
            {
                globalChatView.AddChannel(globalChannelId, true);
            }
            else if (_lastIndex == 1)
            {
                localChatView.AddChannel(localChannelId, true);
            }
        }

        private void SetupChatViews()
        {
            globalChatView = tabContainers[0].GetComponent<FizzChatView>();
            localChatView = tabContainers[1].GetComponent<FizzChatView>();

            // Hide channel list for both chat views as this sample use its own Tabs

            globalChatView.ShowChannels = false;
            globalChatView.EnableFetchHistory = true;
            globalChatView.ShowMessageTranslation = true;

            localChatView.ShowChannels = false;
            localChatView.EnableFetchHistory = true;

            // Hide message translations from local chat view, for demonstraions 
            localChatView.ShowMessageTranslation = false;
        }

        public void HandleClose()
        {
            SceneManager.LoadScene("SceneSelector");
        }
    }
}