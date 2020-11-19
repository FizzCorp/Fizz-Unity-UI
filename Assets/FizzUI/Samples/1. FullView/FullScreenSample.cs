using Fizz.Chat;
using Fizz.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fizz.Common;
using Fizz.UI.Model;

namespace Fizz.Demo
{
    /// <summary>
    /// FullScreensample is designed to demonstrate how FizzChatView can be used to display a full screen chat 
    /// including channel list and an input bar.
    /// </summary>
    public class FullScreenSample : MonoBehaviour, IFizzCustomMessageCellViewDataSource
    {
        [SerializeField] private FizzChatView chatView = null;

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
            chatView.ShowGroups = true;
        }

        private void OnDisable()
        {
            RemoveChannels();
            chatView.ShowGroups = false;
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

            //Set view source for custom data 
            chatView.SetCustomDataViewSource(this);

            chatView.onClose.AddListener (() => gameObject.SetActive (false));
        }

        RectTransform IFizzCustomMessageCellViewDataSource.GetCustomMessageCellViewNode(FizzChannelMessage message)
        {
            try
            {
                // Custom data key
                string KEY_DATA_TYPE = "custom-type";

                if (message != null && message.Data != null
                    && (message.Data.ContainsKey(KEY_DATA_TYPE) && message.Data[KEY_DATA_TYPE] == "invite"))
                {
                    GroupInviteNode node = Instantiate(Resources.Load<GroupInviteNode>("GroupInviteNode"));
                    node.SetData(message);
                    return node.GetComponent<RectTransform>();
                }
            }
            catch
            {
                FizzLogger.E("Unable to add custom node in message cell.");
            }

            return null;
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