using System.Collections.Generic;

namespace Fizz.UI
{
    using Extentions;

    public interface IFizzPredefinedInputDataProvider
    {
        List<string> GetAllTags ();
        List<string> GetAllPhrases (string tag);
        List<string> GetAllStickers (string tag);

        FizzPredefinedPhraseDataItem GetPhrase (string id);
        FizzPredefinedStickerDataItem GetSticker (string id);

        List<string> GetRecentPhrases ();
        List<string> GetRecentStickers ();

        void AddPhraseToRecent (string id);
        void AddStickerToRecent (string id);
    }
}