using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution;
using Estream.Cart42.Web.Helpers;

namespace Estream.Cart42.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly DataContext db;
		protected readonly ICurrentUser currentUser;

        public BaseController()
        {
        }

        public BaseController(DataContext db) : this()
        {
            this.db = db;
        }

        public BaseController(DataContext db, ICurrentUser currentUser) : this(db)
		{
			this.currentUser = currentUser;
		}

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            var language = (string)RouteData.Values["language"];
            var culture = (string)RouteData.Values["culture"];

            if (language == null && WebConfigurationManager.AppSettings["DefaultCulture"] != "en-US")
            {
                language = WebConfigurationManager.AppSettings["DefaultCulture"].Split('-')[0];
                culture = WebConfigurationManager.AppSettings["DefaultCulture"].Split('-')[1];
            }
            else
            {
                if (language == null)
                {
                    var cookie = HttpContext.Request.Cookies["lang"];
                    if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
                    {
                        var values = cookie.Value.Split('-');
                        language = values[0];
                        if (values.Length == 2)
                        {
                            culture = values[1];
                        }
                    }
                }

                if (language == null)
                {
                    var prefLangs = HttpContext.Request.UserLanguages;
                    if (prefLangs != null && prefLangs.Any())
                    {
                        var prefLang = prefLangs.First();
                        if (prefLang.IndexOf(';') >= 0)
                            prefLang = prefLang.Remove(prefLang.IndexOf(';'));
                        if (prefLang.IndexOf('-') >= 0)
                        {
                            var values = prefLang.Split('-');
                            language = values[0];
                            culture = values[1];
                        }
                        else
                        {
                            language = prefLang;
                        }
                    }
                }

                if (language == null)
                {
                    language = WebConfigurationManager.AppSettings["DefaultCulture"].Split('-')[0];
                    culture = WebConfigurationManager.AppSettings["DefaultCulture"].Split('-')[1];
                }
            }

            string cultureCode = culture != null ? string.Format("{0}-{1}", language, culture) : language;

            HttpContext.Response.SetCookie(new HttpCookie("lang", cultureCode));

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureCode);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureCode);

            return base.BeginExecuteCore(callback, state);
        }

        [Obsolete(
            "Do not use the standard Json helpers to return JSON data to the client.  Use either JsonSuccess or JsonError instead."
            )]
        protected JsonResult Json<T>(T data)
        {
            throw new InvalidOperationException(
                "Do not use the standard Json helpers to return JSON data to the client.  Use either JsonSuccess or JsonError instead.");
        }

        protected StandardJsonResult JsonValidationError()
        {
            var result = new StandardJsonResult();

            foreach (ModelError validationError in ModelState.Values.SelectMany(v => v.Errors))
            {
                result.AddError(validationError.ErrorMessage);
            }
            return result;
        }

        protected StandardJsonResult JsonError(string errorMessage)
        {
            var result = new StandardJsonResult();

            result.AddError(errorMessage);

            return result;
        }

        protected StandardJsonResult<T> JsonSuccess<T>(T data)
        {
            return new StandardJsonResult<T> {Data = data};
        }
    }
}