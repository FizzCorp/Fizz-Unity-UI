using Fizz;
using Fizz.UI;
using Fizz.UI.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FullScreenSample : MonoBehaviour
{
    [SerializeField] FizzChatView chatView;

    private FizzChannelMeta globalChannel;
    private FizzChannelMeta localChannel;

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        globalChannel = new FizzChannelMeta("global-channel", "Global");
        localChannel = new FizzChannelMeta("local-channel", "Local");

        FizzService.Instance.AddChannel(globalChannel);
        FizzService.Instance.AddChannel(localChannel);
    }

    private void Start()
    {
        //Show Channel List (Top tab bar)
        chatView.ShowChannels = true;

        //Allow fetching hisgtory
        chatView.EnableFetchHistory = true;

        //Show messaage translations
        chatView.ShowMessageTranslation = true;

        //Add and Set a channel at same time
        chatView.AddChannel(globalChannel, true);
        //Add channel
        chatView.AddChannel(localChannel);
    }

    private void OnDestroy()
    {
        FizzService.Instance.RemoveChannel(globalChannel);
        FizzService.Instance.RemoveChannel(localChannel);
    }

    public void HandleClose ()
    {
        SceneManager.LoadScene ("SceneSelector");
    }
}
