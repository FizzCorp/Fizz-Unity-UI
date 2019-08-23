using System.Collections.Generic;

namespace Fizz.UI
{
    using Extentions;

    public interface IFizzPredefinedInputDataProvider
    {
        List<string> GetAllTags ();
        List<FizzPredefinedDataItem> GetAllPhrases (string tag);
        List<FizzPredefinedDataItem> GetAllStickers (string tag);

        FizzPredefinedPhraseDataItem GetPhrase (string id);
        FizzPredefinedStickerDataItem GetSticker (string id);
    }
}