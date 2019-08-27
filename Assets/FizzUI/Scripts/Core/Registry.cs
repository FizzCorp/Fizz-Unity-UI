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

        public static IFizzPredefinedInputDataProvider PredefinedInputDataProvider
        {
            get { return _predefinedInputDataProvider; }
            set { _predefinedInputDataProvider = value; }
        }

        private static IServiceLocalization _localizationService = new LocalizationService ();
        private static IFizzPredefinedInputDataProvider _predefinedInputDataProvider = new FizzStaticPredefinedInputDataProvider ();
    }
}