using System.Collections.Generic;
using Fizz.Common;
using Fizz.UI.Model;
using UnityEngine;
using UnityEngine.UI;
using Fizz.Chat;

namespace Fizz.Demo
{
    public class ConnectionScript : MonoBehaviour
    {
        [SerializeField] private InputField userIdInputField = null;
        [SerializeField] private InputField userNameInputField = null;
        [SerializeField] private Dropdown langCodeDropDown = null;
        [SerializeField] private Toggle translationToggle = null;

        [SerializeField] private Button connectButton = null;
        [SerializeField] private Button disconnectButton = null;

        [SerializeField] private Button launchButton = null;

        private readonly List<FizzChannelMeta> channelMetas = new List<FizzChannelMeta>
                                                                {
                                                                    new FizzChannelMeta("global-channel", "Global", "DEMO"),
                                                                    new FizzChannelMeta ("local-channel", "Local", "DEMO"),
                                                                    new FizzChannelMeta ("status-channel", "Status", "DEMO"),
                                                                    new FizzChannelMeta ("predefine-channel", "Global", "DEMO")
                                                                };

        private void Awake ()
        {
            SetupView ();
        }

        void OnEnable ()
        {
            AddListeners ();
        }

        void OnDisable ()
        {
            RemoveListeners ();
        }

        public void HandleConnect ()
        {
            try
            {
                if (FizzService.Instance.IsConnected)
                    return;

                FizzService.Instance.Open (
                    userIdInputField.text,                                  //UserId
                    userNameInputField.text,                                //UserName
                    FizzLanguageCodes.AllLanguages[langCodeDropDown.value], //LanguageCode
                    FizzServices.All,
                    translationToggle.isOn,                                 //Translation
                    (success) =>
                {
                    if (success)
                    {
                        FizzLogger.D ("FizzClient Opened Successfully!!");

                        SubscribeChannels();
                    }
                });
            }
            catch { FizzLogger.E ("Unable to connect to Fizz!"); }
        }

        public void HandleDisconnect ()
        {
            try
            {
                FizzService.Instance.Close ();
            }
            catch { FizzLogger.E ("Unable to disconnect to Fizz!"); }
        }

        private void SetupView ()
        {
            connectButton.gameObject.SetActive (!FizzService.Instance.IsConnected);
            disconnectButton.gameObject.SetActive (FizzService.Instance.IsConnected);

            launchButton.interactable = FizzService.Instance.IsConnected;

            SetupIdAndNameInputField ();
            SetupLanguageDropDown ();
            SetupTranslationToggle ();
        }

        private void SetupIdAndNameInputField ()
        {
            string userId = PlayerPrefs.GetString (USER_ID_KEY, System.Guid.NewGuid ().ToString ());
            string userName = PlayerPrefs.GetString (USER_NAME_KEY, "User");

            userIdInputField.text = userId;
            userNameInputField.text = userName;

            HandleUserCradChange (string.Empty);
        }

        private void SetupLanguageDropDown ()
        {
            langCodeDropDown.ClearOptions ();
            List<Dropdown.OptionData> optionsData = new List<Dropdown.OptionData> ();
            foreach (IFizzLanguageCode langCode in FizzLanguageCodes.AllLanguages)
            {
                optionsData.Add (new Dropdown.OptionData (langCode.Language));
            }
            langCodeDropDown.AddOptions (optionsData);

            langCodeDropDown.value = PlayerPrefs.GetInt (USER_LANG_CODE_KEY, 0);
        }

        private void SetupTranslationToggle ()
        {
            translationToggle.isOn = PlayerPrefs.GetInt (USER_TRANSLATION_KEY, 1) == 1;
        }

        private void AddListeners ()
        {
            try
            {
                FizzService.Instance.OnConnected += OnConnected;
                FizzService.Instance.OnDisconnected += OnDisconnected;
            }
            catch
            {
                FizzLogger.E ("Something went wrong with binding events with FizzService.");
            }

            userNameInputField.onEndEdit.AddListener (HandleUserCradChange);
            userNameInputField.onEndEdit.AddListener (HandleUserCradChange);

            langCodeDropDown.onValueChanged.AddListener (HandleLangCodeChange);
            translationToggle.onValueChanged.AddListener (HandleTranslationToggleChange);
        }

        private void RemoveListeners ()
        {
            try
            {
                FizzService.Instance.OnConnected -= OnConnected;
                FizzService.Instance.OnDisconnected -= OnDisconnected;
            }
            catch
            {
                FizzLogger.E ("Something went wrong with binding events with FizzService.");
            }

            userNameInputField.onEndEdit.RemoveListener (HandleUserCradChange);
            userNameInputField.onEndEdit.RemoveListener (HandleUserCradChange);

            langCodeDropDown.onValueChanged.RemoveListener (HandleLangCodeChange);
            translationToggle.onValueChanged.RemoveListener (HandleTranslationToggleChange);
        }

        private void OnConnected (bool sync)
        {
            connectButton.gameObject.SetActive (!FizzService.Instance.IsConnected);
            disconnectButton.gameObject.SetActive (FizzService.Instance.IsConnected);

            launchButton.interactable = true;
        }

        private void OnDisconnected (FizzException ex)
        {
            connectButton.gameObject.SetActive (!FizzService.Instance.IsConnected);
            disconnectButton.gameObject.SetActive (FizzService.Instance.IsConnected);

            launchButton.interactable = false;
        }

        private void HandleUserCradChange (string str)
        {
            PlayerPrefs.SetString (USER_ID_KEY, userIdInputField.text);
            PlayerPrefs.SetString (USER_NAME_KEY, userNameInputField.text);
            PlayerPrefs.Save ();
        }

        private void HandleLangCodeChange (int index)
        {
            PlayerPrefs.SetInt (USER_LANG_CODE_KEY, index);
            PlayerPrefs.Save ();
        }

        private void HandleTranslationToggleChange (bool isOn)
        {
            PlayerPrefs.SetInt (USER_TRANSLATION_KEY, isOn ? 1 : 0);
            PlayerPrefs.Save ();
        }

        private void SubscribeChannels()
        {
            foreach (FizzChannelMeta channelMeta in channelMetas)
            {
                FizzService.Instance.SubscribeChannel(channelMeta);
            }
        }

        private readonly string USER_ID_KEY = "FIZZ_USER_ID";
        private readonly string USER_NAME_KEY = "FIZZ_USER_NAME";
        private readonly string USER_LANG_CODE_KEY = "FIZZ_USER_LANG_CODE";
        private readonly string USER_TRANSLATION_KEY = "FIZZ_USER_TRANSLATION";
    }
}