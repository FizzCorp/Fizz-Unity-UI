using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI
{
    using Components;
    using Extentions;
    using Fizz.UI.Core;

    public class FizzPredefinedInputView : FizzInputView
    {
        [SerializeField] RectTransform PhrasesContainer;
        [SerializeField] RectTransform StickersContainer;
        [SerializeField] RectTransform TabsContainer;

        [SerializeField] Button RecentButton;

        [SerializeField] FizzPredefinedPhraseView PhraseViewPrefab;
        [SerializeField] FizzPredefinedStickerView StickerViewPrefab;
        [SerializeField] FizzPredefinedTagTabView TagTabViewPrefab;

        private IFizzPredefinedInputDataProvider dataProvider;
        private FizzPredefinedTagTabView selectedTab;

        private void Awake ()
        {
            dataProvider = Registry.PredefinedInputDataProvider;
            AdjustGridSize ();
        }

        protected override void OnEnable ()
        {
            base.OnEnable ();

            RecentButton.onClick.AddListener (OnRecentButtonClicked);
            LoadView ();
        }

        protected override void OnDisable ()
        {
            base.OnDisable ();

            RecentButton.onClick.RemoveListener (OnRecentButtonClicked);
        }

        public override void Reset ()
        {
            base.Reset ();
        }

        private void LoadView ()
        {
            LoadTagTabs ();
            LoadPhrases ();
            LoadStickers ();
        }

        private void LoadTagTabs ()
        {
            TabsContainer.DestroyChildren ();
            List<string> tags = dataProvider.GetAllTags ();

            if (tags.Count == 0) return;

            bool selected = false;
            foreach (string tag in tags)
            {
                FizzPredefinedTagTabView tagView = Instantiate (TagTabViewPrefab);
                tagView.gameObject.SetActive (true);
                tagView.transform.SetParent (TabsContainer, false);
                tagView.transform.localScale = Vector3.one;
                tagView.SetTag (tag);
                tagView.OnTabClick = OnTagTabSelected;

                if (!selected)
                {
                    OnTagTabSelected (tagView);
                    selected = true;
                }
            }
        }

        private void OnTagTabSelected (FizzPredefinedTagTabView tab)
        {
            if (selectedTab != null && !selectedTab.Tag.Equals (tab.Tag))
            {
                selectedTab.SetSelected (false);
            }

            SetRecentSelected (false);

            selectedTab = tab;
            selectedTab.SetSelected (true);

            LoadPhrases ();
            LoadStickers ();
        }

        private void LoadPhrases (bool loadRecent = false)
        {
            PhrasesContainer.DestroyChildren ();
            if (!loadRecent && selectedTab == null) return;
            List<string> phrases = loadRecent? dataProvider.GetRecentPhrases () : dataProvider.GetAllPhrases (selectedTab.Tag);
            if (phrases.Count == 0) return;
            
            foreach (string id in phrases)
            {
                FizzPredefinedDataItem phraseItem = dataProvider.GetPhrase (id);
                if (phraseItem == null) continue; 

                FizzPredefinedPhraseView phraseView = Instantiate (PhraseViewPrefab);
                phraseView.gameObject.SetActive (true);
                phraseView.transform.SetParent (PhrasesContainer, false);
                phraseView.transform.localScale = Vector3.one;
                phraseView.SetPhraseData (phraseItem);
                phraseView.OnPhraseClick = OnPhraseClicked;
            }
        }

        private void OnPhraseClicked (FizzPredefinedPhraseView phraseView)
        {
            if (phraseView == null) return;

            dataProvider.AddPhraseToRecent (phraseView.PhraseData.Id);

            if (OnSendData != null)
            {
                Dictionary<string, string> phraseData = new Dictionary<string, string> ();
                phraseData.Add ("type", "fizz_predefine_phrase");
                phraseData.Add ("phrase_id", phraseView.PhraseData.Id);
                OnSendData.Invoke (phraseData);
            }
        }

        private void LoadStickers (bool loadRecent = false)
        {
            StickersContainer.DestroyChildren ();
            if (!loadRecent && selectedTab == null) return;
            List<string> stickers = loadRecent? dataProvider.GetRecentStickers () : dataProvider.GetAllStickers (selectedTab.Tag);
            if (stickers.Count == 0) return;


            foreach (string id in stickers)
            {
                FizzPredefinedDataItem stickerItem = dataProvider.GetSticker (id);
                if (stickerItem == null) continue;

                FizzPredefinedStickerView stickerView = Instantiate (StickerViewPrefab);
                stickerView.gameObject.SetActive (true);
                stickerView.transform.SetParent (StickersContainer, false);
                stickerView.transform.localScale = Vector3.one;
                stickerView.SetStickerData (stickerItem);
                stickerView.OnStickerClick = OnStickerClicked;
            }
        }

        private void OnStickerClicked (FizzPredefinedStickerView stickerView)
        {
            if (stickerView == null) return;

            dataProvider.AddStickerToRecent (stickerView.StickerData.Id);

            if (OnSendData != null)
            {
                Dictionary<string, string> stickerData = new Dictionary<string, string> ();
                stickerData.Add ("type", "fizz_predefine_sticker");
                stickerData.Add ("sticker_id", stickerView.StickerData.Id);
                OnSendData.Invoke (stickerData);
            }
        }

        private void OnRecentButtonClicked ()
        {
            if (selectedTab != null)
            {
                selectedTab.SetSelected (false);
                selectedTab = null;
            }

            SetRecentSelected (true);

            LoadPhrases (true);
            LoadStickers (true);
        }

        private void SetRecentSelected (bool selected)
        {
            RecentButton.targetGraphic.enabled = selected;
        }

        private void AdjustGridSize ()
        {
            //Tags
            GridLayoutGroup TabGrid = TabsContainer.GetComponent<GridLayoutGroup> ();
            TabGrid.cellSize = new Vector2(
                (TabsContainer.rect.width - TabGrid.padding.left - TabGrid.padding.right - (TabGrid.spacing.x * 3))/4,
                TabGrid.cellSize.y);
            //Phrases
            GridLayoutGroup PhrasesGrid = PhrasesContainer.GetComponent<GridLayoutGroup> ();
            PhrasesGrid.cellSize = new Vector2 (
                (PhrasesContainer.rect.width - PhrasesGrid.padding.left - PhrasesGrid.padding.right - (PhrasesGrid.spacing.x * 2)) / 3,
                (PhrasesContainer.rect.height - PhrasesGrid.padding.top - PhrasesGrid.padding.bottom - (PhrasesGrid.spacing.y * 2)) / 3);
            //Stickers
            GridLayoutGroup stickerGrid = StickersContainer.GetComponent<GridLayoutGroup> ();
            stickerGrid.cellSize = new Vector2 (
                (StickersContainer.rect.width - stickerGrid.padding.left - stickerGrid.padding.right - (stickerGrid.spacing.x * 4)) / 5,
                (StickersContainer.rect.width - stickerGrid.padding.left - stickerGrid.padding.right - (stickerGrid.spacing.x * 4)) / 5);
        }
    }
}
