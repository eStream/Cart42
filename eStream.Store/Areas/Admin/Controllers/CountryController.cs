using System.Data.Entity;
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

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class CountryController : BaseController
    {
        private readonly ICountryService countryService;
        public CountryController(ICountryService countryService)
        {
            this.countryService = countryService;
        }

        // GET: Admin/Countries
        [AccessAuthorize(OperatorRoles.SETTINGS)]
        public ActionResult Index()
        {
            return View(countryService.FindAll().ToList());
        }

        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(string code)
        {
            if (code == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Country country = countryService.Find(code);
            if (country == null)
            {
                return HttpNotFound();
            }
            var model = Mapper.Map<CountryViewModel>(country);
            return View(model);
        }

        // POST: Admin/Countries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(CountryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var country = countryService.AddOrUpdate(model);
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("Details for country \"{0}\" have been updated".TA(), country.Name));
            }
            return View(model);
        }

        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult SetActive(string[] codes, bool active)
    {
            foreach (var code in codes)
            {
                countryService.SetActive(code, active);
            }
        
        return RedirectToAction("Index")
            .WithSuccess(string.Format("The selected countries has been marked as {0} ".TA(), active ? "active".TA() : "inactive".TA())); ;
    }
    }
}