using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Models;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Controllers
{
    public class ContentPageController : BaseController
    {
        private readonly ICacheService cacheService;

        public ContentPageController(DataContext db, ICacheService cacheService) : base(db)
        {
            this.cacheService = cacheService;
        }

        [ChildActionOnly]
        //[OutputCache(Duration = 5*60, VaryByParam = "*")]
        public PartialViewResult HeaderLinks()
        {
            var model = cacheService.Get("contentpage_headerlinks",
                () =>
                {
                    IOrderedQueryable<ContentPage> pages = db.ContentPages.Where(
                        c => c.HeaderPosition != null)
                        .OrderBy(c => c.HeaderPosition);

                    var m = new List<ContentPageLinksViewModel>();

                    foreach (ContentPage page in pages)
                    {
                        var viewItem = new ContentPageLinksViewModel();
                        viewItem.Title = page.LinkText ?? page.Title;
                        viewItem.Url = page.RedirectToUrl ??
                                       Url.Action("Details", "ContentPage",
                                           new {id = page.Id, slug = page.Title.ToSlug()});
                        m.Add(viewItem);
                    }
                    return m;
                }, invalidateKeys: "contentpages");

            return PartialView("_HeaderLinks", model);
        }

        [ChildActionOnly]
        //[OutputCache(Duration = 5*60, VaryByParam = "*")]
        public PartialViewResult FooterLinks()
        {
            var model = cacheService.Get("contentpage_footerlinks",
                () =>
                {
                    IOrderedQueryable<ContentPage> pages =
                        db.ContentPages.Where(c => c.FooterPosition != null).OrderBy(c => c.FooterPosition);

                    var m = new List<ContentPageLinksViewModel>();

                    foreach (ContentPage page in pages)
                    {
                        var viewItem = new ContentPageLinksViewModel();
                        viewItem.Title = page.LinkText ?? page.Title;
                        viewItem.Url = page.RedirectToUrl ??
                                       Url.Action("Details", "ContentPage",
                                           new {id = page.Id, slug = page.Title.ToSlug()});
                        m.Add(viewItem);
                    }
                    return m;
                }, invalidateKeys: "contentpages");

            return PartialView("_FooterLinks", model);
        }

        public ActionResult Details(int id)
        {
            ContentPage model = db.ContentPages.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }
    }
}