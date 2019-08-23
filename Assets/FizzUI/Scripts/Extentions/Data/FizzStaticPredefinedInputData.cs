using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fizz.UI.Extentions
{
    [CreateAssetMenu (menuName = "Fizz/PredefinedInputData")]
    public class FizzStaticPredefinedInputData : ScriptableObject
    {
        public List<FizzPredefinedPhraseDataItem> Phrases;
        public List<FizzPredefinedStickerDataItem> Stickers;

        static FizzStaticPredefinedInputData _instance = null;
        public static FizzStaticPredefinedInputData Instance
        {
            get
            {
                if (!_instance)
                    _instance = Resources.FindObjectsOfTypeAll<FizzStaticPredefinedInputData> ().FirstOrDefault ();
                return _instance;
            }
        }
    }

    [Serializable]
    public class FizzPredefinedDataItem
    {
        public string Id;
        public string Tag;
    }

    [Serializable]
    public class FizzPredefinedPhraseDataItem : FizzPredefinedDataItem
    {
        public List<FizzPredefinedLocalizePhrase> LocalizedPhrases;

        public string GetLocalizedContent (SystemLanguage lang)
        {
            foreach (FizzPredefinedLocalizePhrase locPh in LocalizedPhrases)
            {
                if (locPh.Language.Equals (lang)) return locPh.Content;
            }
            return (LocalizedPhrases.Count > 0) ? LocalizedPhrases[0].Content : string.Empty;
        }
    }

    [Serializable]
    public class FizzPredefinedLocalizePhrase
    {
        public string Content;
        public SystemLanguage Language;
    }

    [Serializable]
    public class FizzPredefinedStickerDataItem : FizzPredefinedDataItem
    {
        public Sprite Content;
    }
}