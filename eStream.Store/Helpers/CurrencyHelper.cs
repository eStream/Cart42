using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Helpers
{
    public static class CurrencyHelper
    {
        private static readonly ISettingService settings;

        static CurrencyHelper()
        {
            settings = new SettingService(DataContext.Current);
        }

        public static string ToCurrencyString(this decimal value)
        {
            return settings.Get<string>(SettingField.CurrencyPrefix) +
                   value.ToString("N") +
                   settings.Get<string>(SettingField.CurrencySuffix);
        }
    }
}