using System.Linq;
using System.Web.Mvc;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class HomeController : BaseController
    {
        private readonly ISettingService settingService;

        public HomeController(DataContext db, ISettingService settingService)
            : base(db)
        {
            this.settingService = settingService;
        }

        public ActionResult Index()
        {
            if (!User.HasAccess(OperatorRoles.REPORTS))
            {
                return RedirectToAction("OperatorWelcome");
            }

            if (db.Orders.None())
            {
                if (settingService.Get<bool>(SettingField.ShowWelcomePage))
                {
                    if (!settingService.Get<bool>(SettingField.ShowCategoryTutorial)
                        && !settingService.Get<bool>(SettingField.ShowProductTutorial)
                        && !settingService.Get<bool>(SettingField.ShowOptionTutorial)
                        && !settingService.Get<bool>(SettingField.ShowTaxRateTutorial)
                        && !settingService.Get<bool>(SettingField.ShowShippingRateTutorial))
                    {
                        settingService.Set(SettingField.ShowWelcomePage, false);
                    }
                    else
                    {
                        return RedirectToAction("Welcome");
                    }
                }

                return View().WithInfo(
                        "NOTE: The dashboard currently displays sample data. It will display the actual data after the first order placed on your store".TA());
            }

            return View();
        }

        public ActionResult Welcome()
        {
            return View();
        }

        public ActionResult Remove()
        {
            settingService.Set(SettingField.ShowWelcomePage, false);
            settingService.Set(SettingField.ShowCategoryTutorial, false);
            settingService.Set(SettingField.ShowProductTutorial, false);
            settingService.Set(SettingField.ShowOptionTutorial, false);
            settingService.Set(SettingField.ShowTaxRateTutorial, false);
            settingService.Set(SettingField.ShowShippingRateTutorial, false);

            return RedirectToAction("Index");
        }

        public ActionResult OperatorWelcome()
        {
            return View();
        }
    }
}