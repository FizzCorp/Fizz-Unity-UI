
namespace Fizz.UI.Core
{
    public static class Registry
    {
        public static IServiceLocalization Localization
        {
            get { return _localizationService; }
            set { _localizationService = value; }
        }

        public static IServiceLabelColorScheme LabelColorScheme
        {
            get { return _labelColorSceneService; }
            set { _labelColorSceneService = value; }
        }

        private static IServiceLocalization _localizationService = new LocalizationService ();
        private static IServiceLabelColorScheme _labelColorSceneService = null;
    }

    public interface IServiceLabelColorScheme
    {
        UnityEngine.Color Palette_1 { get; }
        UnityEngine.Color Palette_2 { get; }
        UnityEngine.Color Palette_3 { get; }
        UnityEngine.Color Palette_4 { get; }
    }
}