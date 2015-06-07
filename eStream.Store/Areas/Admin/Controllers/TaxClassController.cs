using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class TaxClassController : BaseController
    {
        private readonly ITaxClassService taxClassService;

        public TaxClassController(ITaxClassService taxClassService)
        {
            this.taxClassService = taxClassService;
        }

        // GET: Admin/TaxClasse
        [AccessAuthorize(OperatorRoles.SETTINGS)]
        public ActionResult Index()
        {
            return View(taxClassService.FindAll().ToList());  // db.TaxClass
        }
        // GET: Admin/TaxClasse/Create
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/TaxClasse/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Create([Bind(Exclude = "Id")] TaxClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                var taxClass = taxClassService.AddOrUpdate(model);
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("Tax class \"{0}\" has been added".TA(), taxClass.Name));
            }

            return View(model);
        }

        // GET: Admin/TaxClasse/Edit/5
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaxClass taxClass = taxClassService.Find(id.Value);
            if (taxClass == null)
            {
                return HttpNotFound();
            }
            var model = Mapper.Map<TaxClassViewModel>(taxClass);
            return View(model);
        }

        // POST: Admin/TaxClasse/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(TaxClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                var taxClass = taxClassService.AddOrUpdate(model);
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("Tax class \"{0}\" has been updated".TA(), taxClass.Name));
            }
            return View(model);
        }

        // GET: Admin/TaxClasse/Delete/5
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.DELETE)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaxClass taxClass = taxClassService.Find(id.Value);
            if (taxClass == null)
            {
                return HttpNotFound();
            }
            return View(taxClass);
        }

        // POST: Admin/TaxClasse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.DELETE)]
        public ActionResult DeleteConfirmed(int id)
        {
            taxClassService.Delete(id);
            return RedirectToAction("Index")
                .WithWarning(string.Format("Tax class has been deleted".TA()));
        }
    }
}
