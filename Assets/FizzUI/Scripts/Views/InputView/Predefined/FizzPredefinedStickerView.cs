using Fizz.UI.Extentions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI
{
    public class FizzPredefinedStickerView : MonoBehaviour
    {
        [SerializeField] Image StickerImage;

        public Action<FizzPredefinedStickerView> OnStickerClick;

        public FizzPredefinedStickerDataItem StickerData { get { return data; } }

        private FizzPredefinedStickerDataItem data;
        private Button button;

        public void SetStickerData (FizzPredefinedDataItem dataItem)
        {
            data = (FizzPredefinedStickerDataItem)dataItem;

            StickerImage.sprite = data.Content;
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
            if (OnStickerClick != null)
                OnStickerClick.Invoke (this);
        }
    }
}