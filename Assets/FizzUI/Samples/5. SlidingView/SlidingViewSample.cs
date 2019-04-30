using Fizz.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fizz.Demo
{
    /// <summary>
    /// SlidingViewSample demonstrates the use of FizzChatView in Sliding window for Lanscape games
    /// </summary>
    public class SlidingViewSample : MonoBehaviour
    {
        [SerializeField] FizzChatView chatView;
        [SerializeField] RectTransform blocker;

        private readonly string globalChannelId = "global-channel_temp";
        private readonly string localChannelId = "local-channel_temp";

        private void Awake()
        {
            SetupChatViews();

            chatView.AddChannel(globalChannelId);
            chatView.AddChannel(localChannelId);
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
            blocker.gameObject.SetActive(_isVisible);

            if (_isVisible)
            {
                chatView.SetCurrentChannel(globalChannelId);
            }
        }

        public void HandleClose()
        {
            SceneManager.LoadScene("SceneSelector");
        }
    }
}