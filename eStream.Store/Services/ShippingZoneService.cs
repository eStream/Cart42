using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Services
{
    public class ShippingZoneService : IShippingZoneService
    {
        private readonly DataContext db;
        private readonly ICountryService countryService;
        private readonly IRegionService regionService;
        private readonly IShippingMethodService shippingMethodService;

        public ShippingZoneService(DataContext db, ICountryService countryService, IRegionService regionService, IShippingMethodService shippingMethodService)
        {
            this.db = db;
            this.countryService = countryService;
            this.regionService = regionService;
            this.shippingMethodService = shippingMethodService;
        }

        public IQueryable<ShippingZone> FindAll()
        {
            return db.ShippingZones;
        }

        public ShippingZone Find(int id)
        {
            return db.ShippingZones.Find(id);
        }

        public ShippingZone Find(string countryCode, int? regionId)
        {
            // Find zone for country and region (if supplied)
            ShippingZone zone = db.ShippingZones.FirstOrDefault(z =>
                z.IsActive && z.Countries.Any(c => c.Code == countryCode)
                && ((regionId == null && !z.Regions.Any()) || (z.Regions.Any(r => r.Id == regionId))));

            // Find zone for country (all regions)
            if (zone == null && regionId.HasValue)
                zone = db.ShippingZones.FirstOrDefault(z => z.IsActive && z.Countries.Any(c => c.Code == countryCode));

            // Fine global zone (catchall)
            if (zone == null)
                zone = db.ShippingZones.FirstOrDefault(z => z.IsActive && !z.Countries.Any() && !z.Regions.Any());

            return zone;
        }

        public ShippingZone AddOrUpdate(ShippingZoneEditViewModel model)
        {

            ShippingZone shippingZone;

            if (model.Id == 0)
            {
                shippingZone = Mapper.Map<ShippingZone>(model);
                db.ShippingZones.Add(shippingZone);
            }
            else
            {
                shippingZone = Find(model.Id);
                shippingZone.Countries.Clear();
                shippingZone.Regions.Clear();
                shippingZone = Mapper.Map(model, shippingZone);
            }

            var countryCodes = JsonConvert.DeserializeObject<string[]>(model.CountryCodesJson);
            foreach (string code in countryCodes)
            {
                Country country = countryService.Find(code);
                shippingZone.Countries.Add(country);
            }
            var regionIds = JsonConvert.DeserializeObject<string[]>(model.RegionIdsJson);
            foreach (string id in regionIds)
            {
                Region region = regionService.Find(Convert.ToInt32(id));
                shippingZone.Regions.Add(region);
            }

            db.SaveChanges();

            return shippingZone;
        }

        public void Delete(int id)
        {
            ShippingZone shippingZone = Find(id);
            var shippingMethods = shippingMethodService.FindAll().Where(m => m.ShippingZoneId == id).ToList();
            foreach (var shippingMethod in shippingMethods)
            {
                shippingMethodService.Delete(shippingMethod.Id);
            }

            db.ShippingZones.Remove(shippingZone);
            db.SaveChanges();
        }
    }
}