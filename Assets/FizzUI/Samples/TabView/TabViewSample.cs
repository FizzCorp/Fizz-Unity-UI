using System.Collections.Generic;
using Fizz;
using Fizz.Common;
using Fizz.UI;
using Fizz.UI.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TabViewSample : MonoBehaviour
{
    [SerializeField] RectTransform[] tabContainers;

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
        }

        _lastIndex = index;
        tabContainers[_lastIndex].gameObject.SetActive(true);

        if (_lastIndex == 0)
        {
            tabContainers[_lastIndex].GetComponent<FizzChatView>().AddChannel(globalChannel, true);
        }
        else if (_lastIndex == 1)
        {
            tabContainers[_lastIndex].GetComponent<FizzChatView>().AddChannel(localChannel, true);
        }
    }

    private void SetupChatViews()
    {
        FizzChatView globalChatView = tabContainers[0].GetComponent<FizzChatView>();
        FizzChatView localChatView = tabContainers[1].GetComponent<FizzChatView>();

        globalChatView.ShowChannels = false;
        globalChatView.EnableFetchHistory = true;
        globalChatView.ShowMessageTranslation = true;

        localChatView.ShowChannels = false;
        localChatView.EnableFetchHistory = true;
        localChatView.ShowMessageTranslation = false;
    }

    private void OnDestroy()
    {
        FizzService.Instance.RemoveChannel(globalChannel);
        FizzService.Instance.RemoveChannel(localChannel);
    }

    public void HandleClose()
    {
        SceneManager.LoadScene("SceneSelector");
    }
}
