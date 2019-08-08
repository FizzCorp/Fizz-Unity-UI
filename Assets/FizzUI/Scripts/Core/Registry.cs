
using Fizz.UI.Extentions;

namespace Fizz.UI.Core
{
    public static class Registry
    {
        public static IServiceLocalization Localization
        {
            get { return _localizationService; }
            set { _localizationService = value; }
        }

        private static IServiceLocalization _localizationService = new LocalizationService ();
    }
}