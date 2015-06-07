using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;

namespace Estream.Cart42.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            var defLang = WebConfigurationManager.AppSettings["DefaultCulture"].Split('-')[0];
            var defCult = WebConfigurationManager.AppSettings["DefaultCulture"].Split('-')[1];

            routes.MapRoute("ContentLocalizedSlug",
                "{language}-{culture}/Content/{id}/{slug}",
                new { language = defLang, culture = defCult, controller = "ContentPage", action = "Details", slug = UrlParameter.Optional },
                new { id = @"\d+" },
                new[] { "Estream.Cart42.Web.Controllers" }
                );

            routes.MapRoute("ContentSlug",
                "Content/{id}/{slug}",
                new { controller = "ContentPage", action = "Details", slug = UrlParameter.Optional },
                new { id = @"\d+" },
                new[] { "Estream.Cart42.Web.Controllers" }
                );

            routes.MapRoute("CategoryLocalizedSlug",
                "{language}-{culture}/Category/{id}/{slug}",
                new { language = defLang, culture = defCult, controller = "Category", action = "Details", slug = UrlParameter.Optional },
                new { id = @"\d+" },
                new[] { "Estream.Cart42.Web.Controllers" }
                );

            routes.MapRoute("CategorySlug",
                "Category/{id}/{slug}",
                new { controller = "Category", action = "Details", slug = UrlParameter.Optional },
                new { id = @"\d+" },
                new[] { "Estream.Cart42.Web.Controllers" }
                );

            routes.MapRoute("ProductLocalizedSlug",
                "{language}-{culture}/Product/{id}/{slug}",
                new { language = defLang, culture = defCult, controller = "Product", action = "Details", slug = UrlParameter.Optional },
                new { id = @"\d+" },
                new[] {"Estream.Cart42.Web.Controllers"}
                );

            routes.MapRoute("ProductSlug",
                "Product/{id}/{slug}",
                new { controller = "Product", action = "Details", slug = UrlParameter.Optional },
                new { id = @"\d+" },
                new[] { "Estream.Cart42.Web.Controllers" }
                );

            routes.MapRoute("DefaultLocalized",
                "{language}-{culture}/{controller}/{action}/{id}",
                new { language = defLang, culture = defCult, controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "Estream.Cart42.Web.Controllers" }
                );

            routes.MapRoute("Default", "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "Estream.Cart42.Web.Controllers" }
                );
        }
    }
}