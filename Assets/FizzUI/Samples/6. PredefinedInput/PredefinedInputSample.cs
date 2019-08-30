using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fizz.Demo
{
    using UI;

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
            //Add and Set channel
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