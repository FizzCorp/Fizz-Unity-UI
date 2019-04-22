using System.Collections.Generic;
using Fizz;
using Fizz.Chat;
using Fizz.Common;
using Fizz.UI;
using Fizz.UI.Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomCellSample : MonoBehaviour, IFizzChatViewCustomDataSource
{

    [SerializeField] FizzChatView chatView;
    [SerializeField] Button sendStatusButton;

    public static string KEY_DATA_TYPE = "custom-type";
    public static string KEY_DATA_STATUS = "custom-type-data";

    private FizzChannelMeta statusChannel;

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        statusChannel = new FizzChannelMeta("status-channel", "Global");

        FizzService.Instance.AddChannel(statusChannel);
    }

    private void Start()
    {
        //Enable history fetch
        chatView.EnableFetchHistory = true;
        //Hide Channels list
        chatView.ShowChannels = false;
        //Hide message translation and toggle
        chatView.ShowMessageTranslation = false;

        //Set view source for custom data 
        chatView.SetCustomDataViewSource(this);
        //Add and Set channel
        chatView.AddChannel(statusChannel, true);
    }

    void OnEnable()
    {
        sendStatusButton.onClick.AddListener(OnSendStatusButtonPressed);
    }

    void OnDisable()
    {
        sendStatusButton.onClick.RemoveListener(OnSendStatusButtonPressed);
    }

    private void OnSendStatusButtonPressed()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add(KEY_DATA_TYPE, "status");
        data.Add(KEY_DATA_STATUS, "HAPPY");
        FizzService.Instance.PublishMessage(
            statusChannel.Id,
            FizzService.Instance.UserName,
            string.Empty,
            data,
            false,
            true,
            ex =>
            {
                if (ex == null)
                {
                    FizzLogger.D("Status Pushed");
                }
            });
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
