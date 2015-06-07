using System.Web;
using System.Web.Mvc;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Engines
{
    public class ThemeableViewEngine : RazorViewEngine
    {
        private readonly ISettingService settingService;

        public ThemeableViewEngine(ISettingService settingService)
        {
            this.settingService = settingService;
            AreaViewLocationFormats = new[]
                                      {
                                          "~/Areas/{2}/Views/{1}/{0}.cshtml",
                                          "~/Areas/{2}/Views/Shared/{0}.cshtml",
                                      };

            AreaMasterLocationFormats = new[]
                                        {
                                            "~/Areas/{2}/Views/{1}/{0}.cshtml",
                                            "~/Areas/{2}/Views/Shared/{0}.cshtml",
                                        };

            AreaPartialViewLocationFormats = new[]
                                             {
                                                 "~/Areas/{2}/Views/{1}/{0}.cshtml",
                                                 "~/Areas/{2}/Views/Shared/{0}.cshtml",
                                             };

            ViewLocationFormats = new[]
                                  {
                                      "~/Views/%1/{1}/{0}.cshtml",
                                      "~/Views/%1/Shared/{0}.cshtml",
                                  };

            MasterLocationFormats = new[]
                                    {
                                        "~/Views/%1/{1}/{0}.cshtml",
                                        "~/Views/%1/Shared/{0}.cshtml",
                                    };

            PartialViewLocationFormats = new[]
                                         {
                                             "~/Views/%1/{1}/{0}.cshtml",
                                             "~/Views/%1/Shared/{0}.cshtml",
                                         };
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            var theme = getTheme(controllerContext);
            return base.CreatePartialView(controllerContext, partialPath.Replace("%1", theme));
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            var theme = getTheme(controllerContext);
            return base.CreateView(controllerContext, viewPath.Replace("%1", theme), masterPath.Replace("%1", theme));
        }

        protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            var theme = getTheme(controllerContext);
            return base.FileExists(controllerContext, virtualPath.Replace("%1", theme));
        }

        private string getTheme(ControllerContext controllerContext)
        {
            var theme = settingService.Get<string>(SettingField.Theme);

            if (controllerContext.HttpContext.Request.QueryString["theme"] != null)
            {
                //var cookie = new HttpCookie("theme", controllerContext.HttpContext.Request.QueryString["theme"]);
                //controllerContext.HttpContext.Response.SetCookie(cookie);
                theme = controllerContext.HttpContext.Request.QueryString["theme"];
            }
            /*
            else if (controllerContext.HttpContext.Request.Cookies["theme"] != null)
            {
                theme = controllerContext.HttpContext.Request.Cookies["theme"].Value;
            }
             */
            else if (controllerContext.HttpContext.Items["theme"] != null)
            {
                theme = controllerContext.HttpContext.Items["theme"].ToString();
            }

            controllerContext.Controller.ViewBag.Theme = theme;
            return theme;
        }
    }
}