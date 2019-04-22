using Fizz.UI.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI.Components
{
	/// <summary>
	/// User interface own repeat chat cell view.
	/// </summary>
	public class FizzChatUserRepeatCellView : FizzChatCellView
	{
		/// <summary>
		/// Chat message delivery status image.
		/// </summary>
		[SerializeField] Image DeliveryStatusImage;
		/// <summary>
		/// Chat message sent status image.
		/// </summary>
		[SerializeField] Image SentStatusImage;

		#region Public Methods

		/// <summary>
		/// Set the data to populate ChatCellView.
		/// </summary>
		/// <param name="model">FIZZChatMessageAction. Please see <see cref="FIZZ.Actions.IFIZZChatMessageAction"/></param>
		public override void SetData (FizzChatCellModel model, bool appTranslationEnabled)
		{
			base.SetData (model, appTranslationEnabled);

			LoadChatMessageAction ();
		}

		#endregion

		#region Private Methods

		private void LoadChatMessageAction () {
			MessageLabel.gameObject.SetActive (true);
            MessageLabel.text = _model.Body;
			
			if (_model.DeliveryState == FizzChatCellDeliveryState.Pending) {
				SentStatusImage.gameObject.SetActive (false);
				DeliveryStatusImage.gameObject.SetActive (false);
			} else if (_model.DeliveryState == FizzChatCellDeliveryState.Sent) {
				SentStatusImage.gameObject.SetActive (true);
				DeliveryStatusImage.gameObject.SetActive (false);
			} else if (_model.DeliveryState == FizzChatCellDeliveryState.Published) {
				SentStatusImage.gameObject.SetActive (false);
				DeliveryStatusImage.gameObject.SetActive (true);
			}
		}

		#endregion
	}
		
}
