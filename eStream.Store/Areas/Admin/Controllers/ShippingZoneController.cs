using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class ShippingZoneController : BaseController
    {
        private readonly IShippingZoneService shippingZoneService;
        private readonly ICountryService countryService;

        public ShippingZoneController(IShippingZoneService shippingZoneService, ICountryService countryService)
        {
            this.shippingZoneService = shippingZoneService;
            this.countryService = countryService;
        }

        // GET: Admin/ShippingZone
        [AccessAuthorize(OperatorRoles.SETTINGS)]
        public ActionResult Index()
        {
            var model = new List<ShippingZoneIndexViewModel>();
            List<ShippingZone> shippingZones = shippingZoneService.FindAll().ToList();
            foreach (ShippingZone zone in shippingZones)
            {
                var zoneView = Mapper.Map<ShippingZoneIndexViewModel>(zone);

                model.Add(zoneView);
            }
            return View(model);
        }

        // GET: Admin/ShippingZones/Create
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Create()
        {
            var model = new ShippingZoneEditViewModel();
            ViewBag.Countries = countryService.FindAll().Where(c => c.IsActive).ToList();
            return View(model);
        }

        // POST: Admin/ShippingZones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Create(ShippingZoneEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                shippingZoneService.AddOrUpdate(model);
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("The shipping zone \"{0}\" has been added".TA(), model.Name));
            }

            ViewBag.Countries = countryService.FindAll().Where(c => c.IsActive).ToList();
            return View(model);
        }

        // GET: Admin/ShippingZones/Edit/5
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ShippingZone shippingZone = shippingZoneService.Find(id.Value);
            if (shippingZone == null)
            {
                return HttpNotFound();
            }
            var model = Mapper.Map<ShippingZoneEditViewModel>(shippingZone);

            string[] countryCodes = shippingZone.Countries.Select(c => c.Code).ToArray();
            model.CountryCodesJson = JsonConvert.SerializeObject(countryCodes);

            int[] regionIds = shippingZone.Regions.Select(r => r.Id).ToArray();
            model.RegionIdsJson = JsonConvert.SerializeObject(regionIds);

            ViewBag.Countries = countryService.FindAll().Where(c => c.IsActive || countryCodes.Contains(c.Code)).ToList();
            return View(model);
        }

        // POST: Admin/ShippingZones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(ShippingZoneEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                shippingZoneService.AddOrUpdate(model);
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("The shipping zone \"{0}\" has been updated".TA(), model.Name));
            }

            ViewBag.Countries = countryService.FindAll().Where(c => c.IsActive).ToList();
            return View(model);
        }

        // GET: Admin/ShippingZones/Delete/5
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.DELETE)]
        public ActionResult Delete(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new ShippingZonesDeleteViewModel();
            model.ShippingZones = new List<ShippingZoneDeleteViewModel>();

            foreach (int id in ids)
            {
                ShippingZone shippingZone = shippingZoneService.Find(id);
                if (shippingZone == null) continue;

                model.ShippingZones.Add(new ShippingZoneDeleteViewModel
                {
                    Id = shippingZone.Id,
                    Name = shippingZone.Name,
                });
            }
            return View(model);
        }

        // POST: Admin/ShippingZones/Delete/5
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
                shippingZoneService.Delete(id);
            }

            return RedirectToAction("Index")
                .WithWarning(string.Format("The shipping zones has been deleted".TA()));
        }

    }
}