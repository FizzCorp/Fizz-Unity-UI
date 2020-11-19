using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI.Extentions
{
    public class ThemeLabelColor : MonoBehaviour
    {
        [SerializeField] private ThemeColor Color = ThemeColor.Primary;

        private void Awake ()
        {
            if (FizzTheme.FizzThemeData == null) return;

            Text label = gameObject.GetComponent<Text> ();
            if (label != null)
            {
                Color toApply = GetThemeColor (Color);
                label.color = new Color (toApply.r, toApply.g, toApply.b, label.color.a);
            }
        }

        private Color GetThemeColor (ThemeColor color)
        {
            switch (color)
            {
                case ThemeColor.Primary:
                    return FizzTheme.FizzThemeData.Primary;
                case ThemeColor.Secondary:
                    return FizzTheme.FizzThemeData.Secondary;
                case ThemeColor.Base_1:
                    return FizzTheme.FizzThemeData.Base_1;
                case ThemeColor.Base_2:
                    return FizzTheme.FizzThemeData.Base_2;
                default:
                    return FizzTheme.FizzThemeData.Base_2;
            }
        }
    }
}