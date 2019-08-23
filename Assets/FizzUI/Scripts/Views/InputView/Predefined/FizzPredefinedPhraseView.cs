using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI
{
    using Extentions;
    using System;

    public class FizzPredefinedPhraseView : MonoBehaviour
    {
        [SerializeField] Text phraseLabel;

        public Action<FizzPredefinedPhraseView> OnPhraseClick;

        public FizzPredefinedPhraseDataItem PhraseData { get { return data; } }

        private FizzPredefinedPhraseDataItem data;
        private Button button;

        public void SetPhraseData (FizzPredefinedDataItem dataItem)
        {
            data = (FizzPredefinedPhraseDataItem)dataItem;

            phraseLabel.text = data.GetLocalizedContent (Application.systemLanguage);
        }

        private void Awake ()
        {
            button = gameObject.GetComponent<Button> ();
        }

        private void OnEnable ()
        {
            button.onClick.AddListener (OnClick);
        }

        private void OnDisable ()
        {
            button.onClick.RemoveListener (OnClick);
        }

        private void OnClick ()
        {
            if (OnPhraseClick != null)
                OnPhraseClick.Invoke (this);
        }
    }
}