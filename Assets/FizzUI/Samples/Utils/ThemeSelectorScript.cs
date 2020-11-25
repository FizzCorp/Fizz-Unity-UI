using Fizz.UI.Extentions;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.Demo
{
    public class ThemeSelectorScript : MonoBehaviour
    {
        [SerializeField] private FizzThemeData DefaultTheme = null;
        [SerializeField] private FizzThemeData BlueTheme = null;
        [SerializeField] private FizzThemeData GreenTheme = null;
        [SerializeField] private FizzThemeData PurpleTheme = null;

        [SerializeField] private Toggle defaultToggle = null;
        [SerializeField] private Toggle blueToggle = null;
        [SerializeField] private Toggle greenToggle = null;
        [SerializeField] private Toggle purpleToggle = null;

        private void Awake ()
        {
            SetActiveToggle ();
        }

        public void HandleDefaultToggleValue (bool val)
        {
            if (val)
            {
                FizzTheme.FizzThemeData = DefaultTheme;
            }
        }

        public void HandleBueToggleValue (bool val)
        {
            if (val)
            {
                FizzTheme.FizzThemeData = BlueTheme;
            }
        }

        public void HandleGreenToggleValue (bool val)
        {
            if (val)
            {
                FizzTheme.FizzThemeData = GreenTheme;
            }
        }

        public void HandlePurpleToggleValue (bool val)
        {
            if (val)
            {
                FizzTheme.FizzThemeData = PurpleTheme;
            }
        }

        private void SetActiveToggle ()
        {
            if (FizzTheme.FizzThemeData.Equals (DefaultTheme)) defaultToggle.isOn = true;
            if (FizzTheme.FizzThemeData.Equals (BlueTheme)) blueToggle.isOn = true;
            if (FizzTheme.FizzThemeData.Equals (GreenTheme)) greenToggle.isOn = true;
            if (FizzTheme.FizzThemeData.Equals (PurpleTheme)) purpleToggle.isOn = true;
        }

    }
}