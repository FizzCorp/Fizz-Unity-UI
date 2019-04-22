using Fizz;
using Fizz.Common;
using Fizz.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSelectorScript : MonoBehaviour
{

    [SerializeField] InputField userIdInputField;
    [SerializeField] InputField userNameInputField;

    private readonly string USER_ID_KEY = "FIZZ_USER_ID";
    private readonly string USER_NAME_KEY = "FIZZ_USER_NAME";

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        string userId = PlayerPrefs.GetString(USER_ID_KEY, System.Guid.NewGuid().ToString());
        string userName = PlayerPrefs.GetString(USER_NAME_KEY, "User");

        userIdInputField.text = userId;
        userNameInputField.text = userName;
    }

    private void OnEnable()
    {
        userNameInputField.onEndEdit.AddListener(HandleUserCradChange);
        userNameInputField.onEndEdit.AddListener(HandleUserCradChange);
    }

    private void OnDisable()
    {
        userNameInputField.onEndEdit.RemoveListener(HandleUserCradChange);
        userNameInputField.onEndEdit.RemoveListener(HandleUserCradChange);
    }

    public void HandleConnect()
    {
        if (FizzService.Instance.IsConnected)
            return;

        FizzService.Instance.Open(userIdInputField.text, userNameInputField.text, Utils.GetSystemLanguage(), true, null, (success) =>
        {
            if (success)
            {
                FizzLogger.D("FizzClient Opened Successfully!!");
            }
        });
    }

    public void HandleDisconnect()
    {
        FizzService.Instance.Close();
    }

    public void HandleFullView()
    {
        SceneManager.LoadScene("FullViewScene");
    }

    public void HandleTabView()
    {
        SceneManager.LoadScene("TabViewScene");
    }

    public void HandleGuildView()
    {
        SceneManager.LoadScene("GuildScene");
    }

    public void HandleCustomCellView()
    {
        SceneManager.LoadScene("CustomCellScene");
    }

    public void HandleSlidingView()
    {
        SceneManager.LoadScene("SlidingViewScene");
    }

    public void HandleClearPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    private void HandleUserCradChange(string str)
    {
        PlayerPrefs.SetString(USER_ID_KEY, userIdInputField.text);
        PlayerPrefs.SetString(USER_NAME_KEY, userNameInputField.text);
        PlayerPrefs.Save();
    }
}
