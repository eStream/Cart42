using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Views
{
    public class BaseViewPage<TModel> : WebViewPage<TModel>
    {
        private readonly ISettingService settingService;
        private readonly ITemplateSettingService templateSettingService;

        public ISettingService Settings
        {
            get { return settingService; }
        }

        public string ThemeUrl
        {
            get
            {
                return Url.Content("~/Content/Themes/" + ViewBag.Theme);
            }
        }

        public BaseViewPage()
        {
            settingService = DependencyResolver.Current.GetService<ISettingService>();
            templateSettingService = DependencyResolver.Current.GetService<ITemplateSettingService>();
        }

        public BaseViewPage(ISettingService settingService, ITemplateSettingService templateSettingService)
        {
            this.settingService = settingService;
            this.templateSettingService = templateSettingService;
        }

        public override void Execute()
        {
            // Do nothing
        }

        public string Setting(string key)
        {
            return templateSettingService.GetSetting(ViewBag.Theme, key);
        }

        public bool SettingTrue(string key)
        {
            return (Setting(key) ?? string.Empty).ToLower() == "true";
        }
    }
}