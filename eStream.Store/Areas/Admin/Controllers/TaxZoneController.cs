using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class TaxZoneController : BaseController
    {
        private readonly ITaxZoneService taxZoneService;
        private readonly ICountryService countryService;

        public TaxZoneController(ITaxZoneService taxZoneService, ICountryService countryService)           
        {
            this.taxZoneService = taxZoneService;
            this.countryService = countryService;
        }

        // GET: Admin/TaxZones
        [AccessAuthorize(OperatorRoles.SETTINGS)]
        public ActionResult Index()
        {
            var model = new List<TaxZoneIndexViewModel>();

            List<TaxZone> taxZones = taxZoneService.FindAll().ToList();
            foreach (TaxZone zone in taxZones)
            {
                var zoneView =  Mapper.Map<TaxZoneIndexViewModel>(zone);

                model.Add(zoneView);
            }

            return View(model);
        }

        // GET: Admin/TaxZones/Create
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Create()
        {
            var model = new TaxZoneEditViewModel();
            ViewBag.Countries = countryService.FindAll().Where(c => c.IsActive).ToList();
            return View(model);
        }

        // POST: Admin/TaxZones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Create(TaxZoneEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                taxZoneService.AddOrUpdate(model);
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("The tax zone \"{0}\" has been added".TA(), model.Name));
            }

            ViewBag.Countries = countryService.FindAll().Where(c => c.IsActive).ToList();
            return View(model);
        }

        // GET: Admin/TaxZones/Edit/5
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TaxZone taxZone = taxZoneService.Find(id.Value);
            if (taxZone == null)
            {
                return HttpNotFound();
            }
            var model = Mapper.Map<TaxZoneEditViewModel>(taxZone);

            string[] countryCodes = taxZone.Countries.Select(c => c.Code).ToArray();
            model.CountryCodesJson = JsonConvert.SerializeObject(countryCodes);

            int[] regionIds = taxZone.Regions.Select(r => r.Id).ToArray();
            model.RegionIdsJson = JsonConvert.SerializeObject(regionIds);

            ViewBag.Countries = countryService.FindAll().Where(c => c.IsActive || countryCodes.Contains(c.Code)).ToList();
            return View(model);
        }

        // POST: Admin/TaxZones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(TaxZoneEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                taxZoneService.AddOrUpdate(model);
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("The tax zone \"{0}\" has been updated".TA(), model.Name));
            }

            ViewBag.Countries = countryService.FindAll().Where(c => c.IsActive).ToList();
            return View(model);
        }

        // GET: Admin/TaxZones/Delete/5
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.DELETE)]
        public ActionResult Delete(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new TaxZonesDeleteViewModel();
            model.TaxZones = new List<TaxZoneDeleteViewModel>();

            foreach (int id in ids)
            {
                TaxZone taxZone = taxZoneService.Find(id);
                if (taxZone == null) continue;

                model.TaxZones.Add(new TaxZoneDeleteViewModel
                {
                    Id = taxZone.Id,
                    Name = taxZone.Name,
                });
            }
            return View(model);
        }

        // POST: Admin/TaxZones/Delete/5
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
                taxZoneService.Delete(id);
            }

            return RedirectToAction("Index")
                .WithWarning("The selected tax zones have been deleted".TA());
        }
    }
}