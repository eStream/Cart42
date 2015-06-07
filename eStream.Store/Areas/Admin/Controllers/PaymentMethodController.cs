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
    public class PaymentMethodController : BaseController
    {
        private readonly IPaymentMethodService paymentMethodService;
        private readonly ICountryService countryService;

        public PaymentMethodController(IPaymentMethodService paymentMethodService, ICountryService countryService)
        {
            this.paymentMethodService = paymentMethodService;
            this.countryService = countryService;
        }

        // GET: Admin/PaymentMethod
        [AccessAuthorize(OperatorRoles.SETTINGS)]
        public ActionResult Index()
        {
            var method = paymentMethodService.FindAll().ToList();
            var model = new PaymentMethodsViewModel
            {
                PaymentMethods = Mapper.Map<List<PaymentMethodViewModel>>(method)
            };
            return View(model);
        }

        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PaymentMethod method = paymentMethodService.Find(id.Value);
            if (method == null)
            {
                return HttpNotFound();
            }
            var model = Mapper.Map<PaymentMethodViewModel>(method);
            var countries = method.Countries.ToList();
            foreach (var country in countries)
            {
               model.CountryCodes.Add(country.Code); 
            }

            ViewBag.Countries = countryService.FindAll().Where(c => c.IsActive).ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(PaymentMethodViewModel model)
        {
            if (ModelState.IsValid)
            {
                paymentMethodService.Update(model);
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("Payment method has been updated".TA()));
            }
            ViewBag.Countries = countryService.FindAll().Where(c => c.IsActive).ToList();
            return View(model);
        }
    }
}