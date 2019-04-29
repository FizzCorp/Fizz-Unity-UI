using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fizz.Demo
{
    public class SceneSelectorScript : MonoBehaviour
    {
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
    }
}