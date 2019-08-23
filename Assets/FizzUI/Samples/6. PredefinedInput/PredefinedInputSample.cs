using Fizz.Chat;
using Fizz.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.Demo
{
    using UI.Extentions;
    using UnityEngine.SceneManagement;

    public class PredefinedInputSample : MonoBehaviour, IFizzCustomMessageCellViewDataSource
    {
        [SerializeField] FizzChatView ChatView;

        [SerializeField] Text PhrasePrefab;
        [SerializeField] Image StickerPrefab;

        [SerializeField] FizzStaticPredefinedInputDataProvider dataProvider;

        // Global Channel Id
        private readonly string globalChannelId = "global-channel-temp";
        // Local Channel Id
        private readonly string localChannelId = "local-channel-temp";

        private void Awake ()
        {
            SetupChatView ();



            print ("@@@@@@@@@@@ Count " + FizzStaticPredefinedInputData.Instance.Phrases.Count );
        }

        private void OnEnable ()
        {
            AddChannels ();
        }

        private void OnDisable ()
        {
            RemoveChannels ();
        }

        private void SetupChatView ()
        {
            ChatView.EnableFetchHistory = true;
            ChatView.ShowMessageTranslation = true;
            ChatView.SetCustomDataViewSource (this);

            ChatView.onClose.AddListener (() => gameObject.SetActive (false));
        }

        private void AddChannels ()
        {
            // Add and Set a channel at same time
            ChatView.AddChannel (globalChannelId, true);
            // Add channel
            ChatView.AddChannel (localChannelId);
        }

        private void RemoveChannels ()
        {
            // Remove channel from chatview
            ChatView.RemoveChannel (globalChannelId);
            ChatView.RemoveChannel (localChannelId);
        }

        public RectTransform GetCustomMessageCellViewNode (FizzChannelMessage message)
        {
            if (message.Data != null)
            {
                if (message.Data.ContainsKey ("type") && message.Data["type"].Equals ("fizz_predefine_phrase"))
                {
                    string id = message.Data["phrase_id"];
                    FizzPredefinedPhraseDataItem phraseData = dataProvider.GetPhrase (id);
                    if (phraseData != null)
                    {
                        Text phraseView = Instantiate (PhrasePrefab);
                        phraseView.gameObject.SetActive (true);
                        phraseView.text = phraseData.GetLocalizedContent (Application.systemLanguage);
                        return phraseView.rectTransform;
                    }

                }
                else if (message.Data.ContainsKey ("type") && message.Data["type"].Equals ("fizz_predefine_sticker"))
                {
                    string id = message.Data["sticker_id"];
                    FizzPredefinedStickerDataItem stickerData = dataProvider.GetSticker (id);
                    if (stickerData != null)
                    {
                        Image sticker = Instantiate (StickerPrefab);
                        sticker.gameObject.SetActive (true);
                        sticker.sprite = stickerData.Content;
                        return sticker.rectTransform;
                    }

                }
            }
            return null;
        }

        public void HandleClose ()
        {
            SceneManager.LoadScene ("SceneSelector");
        }
    }
}