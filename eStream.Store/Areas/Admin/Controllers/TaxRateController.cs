using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Estream.Cart42.Web.Helpers;
using System.Net;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class TaxRateController : BaseController
    {
        private readonly ITaxRateService taxRateService;
        private readonly ITaxZoneService taxZoneService;
        private readonly ITaxClassService taxClassService;
        private readonly ISettingService settingService;

        public TaxRateController(ITaxRateService taxRateService, ITaxZoneService taxZoneService, 
            ITaxClassService taxClassService, ISettingService settingService)
        {
            this.taxRateService = taxRateService;
            this.taxZoneService = taxZoneService;
            this.taxClassService = taxClassService;
            this.settingService = settingService;
        }

        // GET: Admin/TaxRate
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Index()
        {
            var model = new List<TaxRateIndexViewModel>();

            if (settingService.Get<bool>(SettingField.ShowTaxRateTutorial))
            {
                settingService.Set(SettingField.ShowTaxRateTutorial, false);
            }

            List<TaxRate> taxRates = taxRateService.FindAll().ToList();
            foreach (TaxRate rate in taxRates)
            {
                var rateView = Mapper.Map<TaxRateIndexViewModel>(rate);

                model.Add(rateView);
            }

            return View(model);
        }

        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Create()
        {
            var model = new TaxRateEditViewModel();
            foreach (var taxClass in taxClassService.FindAll().ToList())
            {
                model.ClassRates.Add(new TaxClassRateEditViewModel { TaxClassId = taxClass.Id, TaxClassName = taxClass.Name });
            }

            ViewBag.TaxZoneId = new SelectList(taxZoneService.FindAll().Where(z => z.IsActive).ToList(), "Id", "Name");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Create(TaxRateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var taxRate = taxRateService.AddOrUpdate(model);

                return RedirectToAction("Index")
                    .WithSuccess(string.Format("The tax rate \"{0}\" has been added".TA(), taxRate.Name));
            }

            ViewBag.TaxZoneId = new SelectList(taxZoneService.FindAll().Where(z => z.IsActive).ToList(), "Id", "Name");
            return View(model);
        }

        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaxRate taxRate = taxRateService.Find(id.Value);
            var model = Mapper.Map<TaxRateEditViewModel>(taxRate);

            foreach (var taxClass in taxClassService.FindAll().ToList())
            {
                var taxClassRateEditViewModel = new TaxClassRateEditViewModel
                {
                    TaxClassId = taxClass.Id,
                    TaxClassName = taxClass.Name,
                };

                var taxClassRate = taxRate.ClassRates.FirstOrDefault(r => r.TaxClassId == taxClass.Id);
                if (taxClassRate != null)
                    taxClassRateEditViewModel.Amount = taxClassRate.Amount;
                model.ClassRates.Add(taxClassRateEditViewModel);
            }
            ViewBag.TaxZoneId =
                new SelectList(taxZoneService.FindAll().Where(z => z.IsActive || z.Id == model.TaxZoneId).ToList(), "Id",
                    "Name", model.TaxZoneId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(TaxRateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                taxRateService.AddOrUpdate(model);
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("The tax rate \"{0}\" has been updated".TA(), model.Name));
            }

            ViewBag.TaxZoneId =
                new SelectList(taxZoneService.FindAll().Where(z => z.IsActive || z.Id == model.TaxZoneId).ToList(), "Id",
                    "Name", model.TaxZoneId);
            return View(model);
        }

        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.DELETE)]
        public ActionResult Delete(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new TaxRatesDeleteViewModel();
            model.TaxRates = new List<TaxRateDeleteViewModel>();

            foreach (int id in ids)
            {
                TaxRate taxRate = taxRateService.Find(id);
                if (taxRate == null) continue;

                model.TaxRates.Add(new TaxRateDeleteViewModel
                {
                    Id = taxRate.Id,
                    Name = taxRate.Name,
                });
            }
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.DELETE)]
        public ActionResult DeleteConfirmed(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            foreach (int id in ids)
            {
                taxRateService.Delete(id);
            }

            return RedirectToAction("Index")
                .WithWarning(string.Format("The tax rate has been deleted".TA()));
        }
    }
}