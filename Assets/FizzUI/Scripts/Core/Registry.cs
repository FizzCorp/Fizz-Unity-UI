
namespace Fizz.UI.Core
{
    public static class Registry
    {
        public static IServiceLocalization Localization { get; set; } = new LocalizationService();
    }
}