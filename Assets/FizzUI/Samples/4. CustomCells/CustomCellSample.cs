using System.Collections.Generic;
using Fizz.Chat;
using Fizz.Common;
using Fizz.UI;
using Fizz.UI.Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fizz.Demo
{
    /// <summary>
    /// CustomCellSample demonstrate the use of IFizzChatViewCustomDataSource to show custom cells
    /// StatusNode is used to set status from (Happy, Sad, Bored, Angry) and delete it.
    /// </summary>
    public class CustomCellSample : MonoBehaviour, IFizzChatViewCustomDataSource
    {
        [SerializeField] FizzChatView chatView;
        [SerializeField] Button sendStatusButton;

        private FizzChannelMeta statusChannel;

        // Custom data keys
        public static string KEY_DATA_TYPE { get; set; } = "custom-type";
        public static string KEY_DATA_STATUS { get; set; } = "custom-type-data";

        private void Awake()
        {
            SetupChatView();

            AddStatusChannel();
        }

        void OnEnable()
        {
            sendStatusButton.onClick.AddListener(OnSendStatusButtonPressed);
        }

        void OnDisable()
        {
            sendStatusButton.onClick.RemoveListener(OnSendStatusButtonPressed);
        }

        void OnDestroy()
        {
            FizzService.Instance.UnsubscribeChannel(statusChannel.Id);
        }

        private void SetupChatView()
        {
            //Enable history fetch
            chatView.EnableFetchHistory = true;
            //Hide Channels list
            chatView.ShowChannels = false;

            chatView.ShowInput = false;
            //Hide message translation and toggle
            chatView.ShowMessageTranslation = false;
            //Set view source for custom data 
            chatView.SetCustomDataViewSource(this);
        }

        private void AddStatusChannel()
        {
            statusChannel = new FizzChannelMeta("status-channel", "Status");

            FizzService.Instance.SubscribeChannel(statusChannel);

            //Add and Set channel
            chatView.AddChannel(statusChannel.Id, true);
        }

        private void OnSendStatusButtonPressed()
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { KEY_DATA_TYPE, "status" },
                { KEY_DATA_STATUS, "HAPPY" }
            };

            // Publish a new message with custom data
            FizzService.Instance.PublishMessage(
                statusChannel.Id,
                FizzService.Instance.UserName,
                string.Empty,
                data,
                false,
                false,
                true,
                null);
        }

        RectTransform IFizzChatViewCustomDataSource.GetCustomMessageDrawable(FizzChannelMessage message)
        {
            try
            {
                if (message != null && message.Data != null
                    && (message.Data.ContainsKey(KEY_DATA_TYPE) && message.Data[KEY_DATA_TYPE] == "status"))
                {
                    StatusNode node = Instantiate(Resources.Load<StatusNode>("StatusNode"));
                    node.SetData(FizzService.Instance.UserId, message);
                    return node.GetComponent<RectTransform>();

                }
            }
            catch { }

            return null;
        }

        public void HandleClose()
        {
            SceneManager.LoadScene("SceneSelector");
        }
    }
}