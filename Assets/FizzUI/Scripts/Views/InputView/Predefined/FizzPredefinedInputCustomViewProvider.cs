using Fizz.Chat;
using Fizz.UI.Core;
using Fizz.UI.Extentions;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI
{
    public class FizzPredefinedInputCustomViewProvider : MonoBehaviour, IFizzCustomMessageCellViewDataSource
    {
        [SerializeField] FizzChatView ChatView;
        [SerializeField] Text PhrasePrefab;
        [SerializeField] Image StickerPrefab;

        IFizzPredefinedInputDataProvider dataProvider;

        private void Awake ()
        {
            dataProvider = Registry.PredefinedInputDataProvider;

            ChatView.SetCustomDataViewSource (this);
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
    }
}