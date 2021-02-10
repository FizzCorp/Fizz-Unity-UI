using Fizz.Chat;
using Fizz.Common;
using Fizz.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fizz.Demo
{
    /// <summary>
    /// CustomCellSample demonstrate the use of IFizzCustomMessageCellViewDataSource to show custom cells
    /// StatusNode is used to set status from (Happy, Sad, Bored, Angry) and delete it.
    /// </summary>
    public class CustomCellSample : MonoBehaviour, IFizzCustomMessageCellViewDataSource
    {
        [SerializeField] private FizzChatView chatView = null;
        [SerializeField] private Button sendStatusButton = null;

        // Custom data keys
        public static string KEY_DATA_TYPE = "custom-type";
        public static string KEY_DATA_STATUS = "custom-type-data";

        private void Awake()
        {
            SetupChatView();
        }

        void OnEnable()
        {
            AddStatusChannel();
            sendStatusButton.onClick.AddListener(OnSendStatusButtonPressed);
        }

        void OnDisable()
        {
            RemoveStatusChannel();
            sendStatusButton.onClick.RemoveListener(OnSendStatusButtonPressed);
        }

        private void SetupChatView()
        {
            //Enable history fetch
            chatView.EnableFetchHistory = true;
            //Hide Header view
            chatView.ShowHeaderView = false;
            //Hide input
            chatView.ShowInput = false;
            //Hide message translation and toggle
            chatView.ShowMessageTranslation = false;
            //Set view source for custom data 
            chatView.SetCustomDataViewSource(this);
        }

        private void AddStatusChannel()
        {
            //Add and Set channel
            chatView.AddChannel("status-channel", true);
        }

        void RemoveStatusChannel()
        {
            chatView.RemoveChannel ("status-channel");
        }

        private void OnSendStatusButtonPressed()
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { KEY_DATA_TYPE, "status" },
                { KEY_DATA_STATUS, "HAPPY" }
            };

            // Publish a new message with custom data
            FizzService.Instance.Client.Chat.PublishMessage(
                "status-channel",
                FizzService.Instance.UserName,
                string.Empty,
                string.Empty,
                data,
                false,
                false,
                true,
                null);
        }

        RectTransform IFizzCustomMessageCellViewDataSource.GetCustomMessageCellViewNode(FizzChannelMessage message)
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
            catch
            {
                FizzLogger.E("Unable to add custom node in message cell.");
            }

            return null;
        }

        public void HandleClose()
        {
            SceneManager.LoadScene("SceneSelector");
        }
    }
}