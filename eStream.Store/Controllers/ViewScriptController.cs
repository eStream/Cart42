using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.UI;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;
using Estream.Cart42.Web.Views;
using RazorEngine;
using RazorEngine.Templating;

namespace Estream.Cart42.Web.Controllers
{
    public class ViewScriptController : BaseController
    {
        private readonly ICacheService cacheService;

        public ViewScriptController(ICacheService cacheService)
        {
            this.cacheService = cacheService;
        }

        // GET: ViewScript
        public ContentResult Index(string c, string n)
        {
            var viewPath = Server.MapPath("~/Views");
            var jsFile = string.Format(@"{0}\Shared\{1}\{2}.js", viewPath, c, n);
            var content = cacheService.Get(jsFile, () => System.IO.File.ReadAllText(jsFile),
                absoluteExpiration: DateTime.Now.AddMinutes(30));

            var jsResult = Razor.Parse(content, jsFile + CultureInfo.CurrentUICulture);
            return new ContentResult { Content = jsResult, ContentType = "application/javascript" };
        }
    }
}