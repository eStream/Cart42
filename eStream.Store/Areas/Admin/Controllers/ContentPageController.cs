using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class ContentPageController : BaseController
    {
        private readonly ICacheService cacheService;

        public ContentPageController(DataContext db, ICacheService cacheService)
            : base(db)
        {
            this.cacheService = cacheService;
        }

        // GET: Admin/ContentPage
        [AccessAuthorize(OperatorRoles.CONTENT)]
        public ActionResult Index()
        {
            return View(db.ContentPages.ToList());
        }

        // GET: Admin/ContentPage/Create
        [AccessAuthorize(OperatorRoles.CONTENT + OperatorRoles.WRITE)]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/ContentPage/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.CONTENT + OperatorRoles.WRITE)]
        public ActionResult Create(
            [Bind(
                Include =
                    "Id,Title,Content,LinkText,HeaderPosition,FooterPosition,MetaDescription,MetaKeywords,RedirectToUrl"
                )] ContentPage contentPage)
        {
            if (ModelState.IsValid)
            {
                db.ContentPages.Add(contentPage);
                db.SaveChanges();
                cacheService.Invalidate("contentpages");
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("Content page \"{0}\" has been added".TA(), contentPage.Title));
            }

            return View(contentPage);
        }

        // GET: Admin/ContentPage/Edit/5
        [AccessAuthorize(OperatorRoles.CONTENT + OperatorRoles.WRITE)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContentPage contentPage = db.ContentPages.Find(id);
            if (contentPage == null)
            {
                return HttpNotFound();
            }
            return View(contentPage);
        }

        // POST: Admin/ContentPage/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.CONTENT + OperatorRoles.WRITE)]
        public ActionResult Edit(
            [Bind(
                Include =
                    "Id,Title,Content,LinkText,HeaderPosition,FooterPosition,MetaDescription,MetaKeywords,RedirectToUrl"
                )] ContentPage contentPage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(contentPage).State = EntityState.Modified;
                db.SaveChanges();
                cacheService.Invalidate("contentpages");
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("Content page \"{0}\" has been updated".TA(), contentPage.Title));
            }
            return View(contentPage);
        }

        // GET: Admin/ContentPage/Delete/5
        [AccessAuthorize(OperatorRoles.CONTENT + OperatorRoles.DELETE)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContentPage contentPage = db.ContentPages.Find(id);
            if (contentPage == null)
            {
                return HttpNotFound();
            }
            return View(contentPage);
        }

        // POST: Admin/ContentPage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.CONTENT + OperatorRoles.DELETE)]
        public ActionResult DeleteConfirmed(int id)
        {
            ContentPage contentPage = db.ContentPages.Find(id);
            db.ContentPages.Remove(contentPage);
            db.SaveChanges();
            cacheService.Invalidate("contentpages");
            return RedirectToAction("Index")
                .WithWarning(string.Format("Content page \"{0}\" has been deleted".TA(), contentPage.Title));
        }
    }
}