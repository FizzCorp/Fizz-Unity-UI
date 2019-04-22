using System.Collections.Generic;
using Fizz;
using Fizz.Common;
using Fizz.UI;
using Fizz.UI.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SlidingViewSample : MonoBehaviour
{
    [SerializeField] FizzChatView chatView;

    private FizzChannelMeta globalChannel;
    private FizzChannelMeta localChannel;

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        globalChannel = new FizzChannelMeta("global-channel", "Global");
        localChannel = new FizzChannelMeta("local-channel", "Local");

        FizzService.Instance.AddChannel(globalChannel);
        FizzService.Instance.AddChannel(localChannel);

    }

    private void Start()
    {
        SetupChatViews();

        chatView.AddChannel(globalChannel);
        chatView.AddChannel(localChannel);
    }

    private void SetupChatViews()
    {
        chatView.ShowChannels = true;
        chatView.EnableFetchHistory = true;
        chatView.ShowMessageTranslation = true;
    }

    bool _isVisible = false;
    public void HandleViewToggle()
    {
        _isVisible = !_isVisible;

        gameObject.GetComponent<RectTransform>().anchoredPosition = _isVisible ? Vector2.zero : Vector2.left * 600;

        if (_isVisible)
        {
            chatView.SetChannel(globalChannel);
        }
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
