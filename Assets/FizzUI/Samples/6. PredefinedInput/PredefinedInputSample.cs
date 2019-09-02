using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fizz.Demo
{
    using UI;

    // Predefined Input sample is designed to demonstrate the usage of FizzChatView with Predefined Input View. 
    // FizzPredefinedInputView is used as a static keyboard which will show predefined phrases and sticker. 
    
    public class PredefinedInputSample : MonoBehaviour
    {
        [SerializeField] FizzChatView ChatView;

        private void Awake ()
        {
            SetupChatView ();
        }

        private void OnEnable ()
        {
            AddPredefineInputChannel ();
        }

        private void OnDisable ()
        {
            RemovePredefineInputChannel ();
        }

        private void SetupChatView ()
        {
            ChatView.EnableFetchHistory = true;
            ChatView.ShowMessageTranslation = true;

            ChatView.onClose.AddListener (() => gameObject.SetActive (false));
        }

        private void AddPredefineInputChannel ()
        {
            ChatView.AddChannel ("predefine-channel", true);
        }

        void RemovePredefineInputChannel ()
        {
            ChatView.RemoveChannel ("predefine-channel");
        }

        public void HandleClose ()
        {
            SceneManager.LoadScene ("SceneSelector");
        }
    }
}