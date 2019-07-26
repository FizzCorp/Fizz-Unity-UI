using UnityEngine;

namespace Fizz.UI.Core
{
    public class LabelColorSchemeService : MonoBehaviour, IServiceLabelColorScheme
    {
        [SerializeField] Color Palette1;
        [SerializeField] Color Palette2;
        [SerializeField] Color Palette3;
        [SerializeField] Color Palette4;

        public Color Palette_1
        {
            get { return Palette1; }
        }

        public Color Palette_2
        {
            get { return Palette2; }
        }

        public Color Palette_3
        {
            get { return Palette3; }
        }

        public Color Palette_4
        {
            get { return Palette4; }
        }

        void Awake ()
        {
            Registry.LabelColorScheme = this;
        }
    }
}
