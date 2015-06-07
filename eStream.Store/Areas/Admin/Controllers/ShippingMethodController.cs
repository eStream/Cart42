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

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class ShippingMethodController : BaseController
    {
        private readonly IShippingMethodService shippingMethodService;
        private readonly IShippingZoneService shippingZoneService;
        private readonly ISettingService settingService;

        public ShippingMethodController(IShippingMethodService shippingMethodService, 
            IShippingZoneService shippingZoneService, ISettingService settingService)
        {
            this.shippingMethodService = shippingMethodService;
            this.shippingZoneService = shippingZoneService;
            this.settingService = settingService;
        }

        // GET: Admin/ShippingMethod
        [AccessAuthorize(OperatorRoles.SETTINGS)]
        public ActionResult Index()
        {
            var model = new List<ShippingMethodIndexViewModel>();
            List<ShippingMethod> shippingMethods = shippingMethodService.FindAll().ToList();
            foreach (ShippingMethod zone in shippingMethods)
            {
                var zoneView = Mapper.Map<ShippingMethodIndexViewModel>(zone);

                model.Add(zoneView);
            }
           
            return View(model);
        }

        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Create()
        {
            var model = new ShippingMethodEditViewModel();
            ViewBag.ShippingZoneId = new SelectList(shippingZoneService.FindAll().ToList(), "Id", "Name");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Create(ShippingMethodEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                shippingMethodService.AddOrUpdate(model);

                var action = RedirectToAction("Index");

                if (settingService.Get<bool>(SettingField.ShowShippingRateTutorial))
                {
                    settingService.Set(SettingField.ShowShippingRateTutorial, false);
                    action = RedirectToAction("Welcome", "Home");
                }

                return action.WithSuccess(string.Format("The shipping method \"{0}\" has been added".TA(), model.Name));
            }
            ViewBag.ShippingZoneId = new SelectList(shippingZoneService.FindAll().ToList(), "Id", "Name");
            return View(model);
        }

        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ShippingMethod shippingMethod = shippingMethodService.Find(id.Value);
            if (shippingMethod == null)
            {
                return HttpNotFound();
            }
            var model = Mapper.Map<ShippingMethodEditViewModel>(shippingMethod);

            ViewBag.ShippingZoneId = new SelectList(shippingZoneService.FindAll().ToList(), "Id", "Name");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(ShippingMethodEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                shippingMethodService.AddOrUpdate(model);
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("The shipping method \"{0}\" has been updated".TA(), model.Name));
            }

            ViewBag.ShippingZoneId = new SelectList(shippingZoneService.FindAll().ToList(), "Id", "Name");
            return View(model);
        }

        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.DELETE)]
        public ActionResult Delete(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new ShippingMethodsDeleteViewModel();
            model.ShippingMethods = new List<ShippingMethodDeleteViewModel>();

            foreach (int id in ids)
            {
                ShippingMethod shippingMethod = shippingMethodService.Find(id);
                if (shippingMethod == null) continue;

                model.ShippingMethods.Add(new ShippingMethodDeleteViewModel
                {
                    Id = shippingMethod.Id,
                    Name = shippingMethod.Name,
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
                shippingMethodService.Delete(id);
            }

            return RedirectToAction("Index")
                .WithWarning(string.Format("The shipping method has been deleted".TA()));
        }
    }
}