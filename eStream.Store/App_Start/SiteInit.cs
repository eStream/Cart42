using System.Web.Configuration;
using Estream.Cart42.Web.Areas.Admin.Controllers;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Tasks;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web
{
    public class SiteInit : IRunAtStartup
    {
        private readonly ISettingService settingService;

        public SiteInit(DataContext db, ISettingService settingService)
        {
            this.settingService = settingService;
        }

        public void Execute()
        {
            if (settingService.Get<bool>(SettingField.IsInitialized)) return;

            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["DefaultStoreName"]))
                settingService.Set(SettingField.StoreName, WebConfigurationManager.AppSettings["DefaultStoreName"]);

            if (WebConfigurationManager.AppSettings["DefaultCulture"].StartsWith("bg"))
            {
                settingService.Set(SettingField.CurrencyCode, "BGN");
                settingService.Set(SettingField.CurrencySuffix, " лв.");
            }

            settingService.Set(SettingField.IsInitialized, true);
        }
    }
}