using Fizz.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI.Extentions
{
    public class LabelColorScheme : MonoBehaviour
    {
        [SerializeField] LabelColorSchemePalette Palette;

        private Text label;

        private void Awake ()
        {
            label = gameObject.GetComponent<Text> ();
        }

        private void Start ()
        {
            Color colorToApply = GetColorForPalette ();
            label.color = new Color (colorToApply.r, colorToApply.g, colorToApply.b, label.color.a);
        }
        

        private Color GetColorForPalette ()
        {
            Color color = Color.black;
            switch (Palette)
            {
                case LabelColorSchemePalette.Palette_1:
                    color = Registry.LabelColorScheme.Palette_1;
                    break;
                case LabelColorSchemePalette.Palette_2:
                    color = Registry.LabelColorScheme.Palette_2;
                    break;
                case LabelColorSchemePalette.Palette_3:
                    color = Registry.LabelColorScheme.Palette_3;
                    break;
                case LabelColorSchemePalette.Palette_4:
                    color = Registry.LabelColorScheme.Palette_4;
                    break;
                default:
                    color = Color.black;
                    break;
            }

            return color;
        }

    }

    public enum LabelColorSchemePalette
    {
        Palette_1,
        Palette_2,
        Palette_3,
        Palette_4
    }
}
