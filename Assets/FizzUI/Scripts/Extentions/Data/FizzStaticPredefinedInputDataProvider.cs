using System.Collections.Generic;
using UnityEngine;

namespace Fizz.UI.Extentions
{
    public class FizzStaticPredefinedInputDataProvider : IFizzPredefinedInputDataProvider
    {
        public List<string> GetAllPhrases (string tag)
        {
            List<string> phrases = new List<string> ();

            if (FizzStaticPredefinedInputData.Instance == null) return phrases;

            foreach (FizzPredefinedDataItem dataItem in FizzStaticPredefinedInputData.Instance.Phrases)
            {
                if (dataItem.Tag.Equals (tag))
                    phrases.Add (dataItem.Id);
            }
            return phrases;
        }

        public List<string> GetAllStickers (string tag)
        {
            List<string> stickers = new List<string> ();

            if (FizzStaticPredefinedInputData.Instance == null) return stickers;

            foreach (FizzPredefinedDataItem dataItem in FizzStaticPredefinedInputData.Instance.Stickers)
            {
                if (dataItem.Tag.Equals (tag))
                    stickers.Add (dataItem.Id);
            }
            return stickers;
        }

        public List<string> GetAllTags ()
        {
            List<string> tags = new List<string> ();

            if (FizzStaticPredefinedInputData.Instance == null) return tags;

            foreach (FizzPredefinedDataItem dataItem in FizzStaticPredefinedInputData.Instance.Phrases)
            {
                if (tags.Contains (dataItem.Tag)) continue;
                tags.Add (dataItem.Tag);
            }
            foreach (FizzPredefinedDataItem dataItem in FizzStaticPredefinedInputData.Instance.Stickers)
            {
                if (tags.Contains (dataItem.Tag)) continue;
                tags.Add (dataItem.Tag);
            }
            return tags;
        }

        public FizzPredefinedPhraseDataItem GetPhrase (string id)
        {
            if (FizzStaticPredefinedInputData.Instance == null) return null;
            
            foreach (FizzPredefinedPhraseDataItem dataItem in FizzStaticPredefinedInputData.Instance.Phrases)
            {
                if (dataItem.Id.Equals (id))
                    return dataItem;
            }
            return null;
        }

        public FizzPredefinedStickerDataItem GetSticker (string id)
        {
            if (FizzStaticPredefinedInputData.Instance == null) return null;

            foreach (FizzPredefinedStickerDataItem dataItem in FizzStaticPredefinedInputData.Instance.Stickers)
            {
                if (dataItem.Id.Equals (id))
                    return dataItem;
            }
            return null;
        }

        public List<string> GetRecentPhrases ()
        {
#if UNITY_EDITOR
            if (recentPhrases == null) recentPhrases = new List<string> ();
            return recentPhrases;
#else
            string ids = PlayerPrefs.GetString ("fizz_cached_recent_phrases", string.Empty);
            return new List<string> (ids.Split (';'));
#endif
        }

        public List<string> GetRecentStickers ()
        {
#if UNITY_EDITOR
            if (recentStickers == null) recentStickers = new List<string> ();
            return recentStickers;
#else
            string ids = PlayerPrefs.GetString ("fizz_cached_recent_stickers", string.Empty);
            return new List<string> (ids.Split (';'));
#endif
        }

        public void AddPhraseToRecent (string id)
        {
#if UNITY_EDITOR
            if (recentPhrases == null) recentPhrases = new List<string> (9);
            if (recentPhrases.Contains (id)) return;
            if (recentPhrases.Count >= 9) recentPhrases.RemoveAt (recentPhrases.Count - 1);
            recentPhrases.Insert (0, id);
#else
            string ids = PlayerPrefs.GetString ("fizz_cached_recent_phrases", string.Empty);
            if (ids.Contains (id)) return;
            if (ids.Split (';').Length >= 9) ids = ids.Substring (0, ids.LastIndexOf (';') + 1);

            ids = id + ";" + ids;
            
            PlayerPrefs.SetString ("fizz_cached_recent_phrases", ids);
            PlayerPrefs.Save ();
#endif
        }

        public void AddStickerToRecent (string id)
        {
#if UNITY_EDITOR
            if (recentStickers == null) recentStickers = new List<string> (5);
            if (recentStickers.Contains (id)) return;
            if (recentStickers.Count >= 5) recentStickers.RemoveAt (recentStickers.Count - 1);
            recentStickers.Insert (0, id);
#else
            string ids = PlayerPrefs.GetString ("fizz_cached_recent_stickers", string.Empty);
            if (ids.Contains (id)) return;
            if (ids.Split (';').Length >= 5) ids = ids.Substring (0, ids.LastIndexOf (';') + 1);

            ids = id + ";" + ids;
            
            PlayerPrefs.SetString ("fizz_cached_recent_stickers", ids);
            PlayerPrefs.Save ();
#endif
        }

#if UNITY_EDITOR
        private List<string> recentPhrases;
        private List<string> recentStickers;
#endif
    }
}