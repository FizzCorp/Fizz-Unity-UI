using Fizz.UI.Model;

namespace Fizz.UI.Components
{
    public class FizzChatDateHeaderCellView : FizzChatCellView
    {
        #region Public Methods

        public override void SetData(FizzChatCellModel model, bool appTranslationEnabled)
        {
            base.SetData(model, appTranslationEnabled);

            MessageLabel.text = Utils.GetFormattedTimeForUnixTimeStamp(_model.Created);
        }

        #endregion
    }
}