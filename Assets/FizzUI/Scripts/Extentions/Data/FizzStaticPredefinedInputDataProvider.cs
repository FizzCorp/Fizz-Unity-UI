using System.Collections.Generic;
using UnityEngine;

namespace Fizz.UI.Extentions
{
    public class FizzStaticPredefinedInputDataProvider : MonoBehaviour, IFizzPredefinedInputDataProvider
    {
        [SerializeField] FizzStaticPredefinedInputData InputData;

        public List<FizzPredefinedDataItem> GetAllPhrases (string tag)
        {
            List<FizzPredefinedDataItem> phrases = new List<FizzPredefinedDataItem> ();
            foreach (FizzPredefinedDataItem dataItem in InputData.Phrases)
            {
                if (dataItem.Tag.Equals (tag))
                    phrases.Add (dataItem);
            }
            return phrases;
        }

        public List<FizzPredefinedDataItem> GetAllStickers (string tag)
        {
            List<FizzPredefinedDataItem> stickers = new List<FizzPredefinedDataItem> ();
            foreach (FizzPredefinedDataItem dataItem in InputData.Stickers)
            {
                if (dataItem.Tag.Equals (tag))
                    stickers.Add (dataItem);
            }
            return stickers;
        }

        public List<string> GetAllTags ()
        {
            List<string> tags = new List<string> ();
            foreach (FizzPredefinedDataItem dataItem in InputData.Phrases)
            {
                if (tags.Contains (dataItem.Tag)) continue;
                tags.Add (dataItem.Tag);
            }
            foreach (FizzPredefinedDataItem dataItem in InputData.Stickers)
            {
                if (tags.Contains (dataItem.Tag)) continue;
                tags.Add (dataItem.Tag);
            }
            return tags;
        }

        public FizzPredefinedPhraseDataItem GetPhrase (string id)
        {
            foreach (FizzPredefinedPhraseDataItem dataItem in InputData.Phrases)
            {
                if (dataItem.Id.Equals (id))
                    return dataItem;
            }
            return null;
        }

        public FizzPredefinedStickerDataItem GetSticker (string id)
        {
            foreach (FizzPredefinedStickerDataItem dataItem in InputData.Stickers)
            {
                if (dataItem.Id.Equals (id))
                    return dataItem;
            }
            return null;
        }
    }
}