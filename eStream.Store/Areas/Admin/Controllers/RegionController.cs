using System.Collections.Generic;
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
    public class RegionController : BaseController
    {
        private readonly IRegionService regionService;
        private readonly ICountryService countryService;

        // GET: Admin/Regions
        public RegionController(IRegionService regionService, ICountryService countryService)
        {
            this.regionService = regionService;
            this.countryService = countryService;
        }

        [AccessAuthorize(OperatorRoles.SETTINGS)]
        public ActionResult Index(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
                return RedirectToAction("Index", "Country");

            var regions = regionService.FindByCountryCode(countryCode).ToList();
            var country = countryService.Find(countryCode);

            ViewBag.CountryCode = countryCode;
            ViewBag.CountryName = country.Name;
            return View(regions);
        }

        // GET: Admin/Regions/Create
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Create(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
                return RedirectToAction("Index", "Country");

            var region = new Region {CountryCode = countryCode};
            var model = Mapper.Map<RegionViewModel>(region);
            var country = countryService.Find(countryCode);
            ViewBag.CountryName = country.Name;
            ViewBag.CountryCode = countryCode;
            return View(model);
        }

        // POST: Admin/Regions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Create([Bind(Exclude = "Id")] RegionViewModel model)
        {
            if (ModelState.IsValid)
            {
                regionService.AddOrUpdate(model);
                return RedirectToAction("Index", "Region", new {countryCode = model.CountryCode})
                    .WithSuccess("The region has been added".TA()); ;
            }
            return View(model);
        }

        // GET: Admin/Regions/Edit/5
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Region region = regionService.Find(id.Value);
            if (region == null)
            {
                return HttpNotFound();
            }

            var model = Mapper.Map<RegionViewModel>(region);
            var country = countryService.Find(region.CountryCode);

            ViewBag.CountryName = country.Name;
            return View(model);
        }

        // POST: Admin/Regions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Edit(RegionViewModel model)
        {
            if (ModelState.IsValid)
            {
                regionService.AddOrUpdate(model);
                return RedirectToAction("Index", "Region", new {countryCode = model.CountryCode})
                    .WithSuccess("The region has been updated".TA());
            }
            return View(model);
        }

        // GET: Admin/Regions/Delete/5
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.DELETE)]
        public ActionResult Delete(int[] ids)
        {

            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new RegionsViewModel();
            model.Regions = new List<RegionViewModel>();

            foreach (int id in ids)
            {
                Region region = regionService.Find(id);
                if (region == null) continue;
                if (ViewBag.CountryCode == null) { ViewBag.CountryCode = region.CountryCode; }

                model.Regions.Add(new RegionViewModel
                {
                    Id = region.Id,
                    Name = region.Name,
                });
            }
           return View(model);
        }

        // POST: Admin/Regions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.DELETE)]
        public ActionResult DeleteConfirmed(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var region = regionService.Find(ids.First());
            var countryCode = region.CountryCode;
            foreach (int id in ids)
            {
                regionService.Delete(id);
            }
            return RedirectToAction("Index", "Region", new { countryCode }).WithWarning("The selected regions have been deleted".TA());
        }
    }
}