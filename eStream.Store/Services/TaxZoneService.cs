using System;
using System.Linq;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Services
{
    public class TaxZoneService : ITaxZoneService
    {
        private readonly DataContext db;
        private readonly ITaxRateService taxRateService;
        private readonly ICountryService countryService;
        private readonly IRegionService regionService;

        public TaxZoneService(DataContext db, ITaxRateService taxRateService, ICountryService countryService, IRegionService  regionService)
        {
            this.db = db;
            this.taxRateService = taxRateService;
            this.countryService = countryService;
            this.regionService = regionService;
        }

        public IQueryable<TaxZone> FindAll()
        {
            return db.TaxZones;
        }

        public TaxZone Find(int id)
        {
            return db.TaxZones.Find(id);
        }

        public TaxZone Find(string countryCode, int? regionId)
        {
            TaxZone taxZone = null;
            var zones = FindAll();

            if (regionId.HasValue)
                taxZone = zones.FirstOrDefault(z =>
                    z.IsActive && z.Countries.Any(c => c.Code == countryCode) && z.Regions.Any(r => r.Id == regionId));
            if (taxZone == null)
                taxZone = zones.FirstOrDefault(z =>
                    z.IsActive && z.Countries.Any(c => c.Code == countryCode) && !z.Regions.Any());
            if (taxZone == null)
                taxZone = zones.FirstOrDefault(z =>
                    z.IsActive && !z.Countries.Any() && !z.Regions.Any());

            return taxZone;
        }

        public TaxZone AddOrUpdate(TaxZoneEditViewModel model)
        {

           TaxZone taxZone;

            if (model.Id == 0)
            {
                taxZone = Mapper.Map<TaxZone>(model);
                db.TaxZones.Add(taxZone);
            }
            else
            {
                taxZone = Find(model.Id);
                taxZone.Countries.Clear();
                taxZone.Regions.Clear();
                taxZone = Mapper.Map(model, taxZone);
            }

            var countryCodes = JsonConvert.DeserializeObject<string[]>(model.CountryCodesJson);
            foreach (string code in countryCodes)
            {
                Country country = countryService.Find(code);
                taxZone.Countries.Add(country);
            }
            var regionIds = JsonConvert.DeserializeObject<string[]>(model.RegionIdsJson);
            foreach (string id in regionIds)
            {
                Region region = regionService.Find(Convert.ToInt32(id));
                taxZone.Regions.Add(region);
            }
            
            db.SaveChanges();

            return taxZone;
        }

        public void Delete(int id)
        {
            TaxZone taxZone = Find(id);
            var taxRates = taxRateService.FindByZone(id).ToList();
            foreach (var taxRate in taxRates)
            {
                taxRateService.Delete(taxRate.Id);
            }
            db.TaxZones.Remove(taxZone);
            db.SaveChanges();
        }
    }
}