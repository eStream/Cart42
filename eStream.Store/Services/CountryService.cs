using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public class CountryService : ICountryService
    {
        private readonly DataContext db;
        private readonly IRegionService regionService;

        public CountryService(DataContext db, IRegionService regionService)
        {
            this.regionService = regionService;
            this.db = db;
        }

        public IQueryable<Country> FindAll()
        {
            return db.Countries;
        }

        public Country Find(string code)
        {
            return db.Countries.Find(code);
        }

        public Country AddOrUpdate(CountryViewModel model)
        {
            Country country = null;

            var dbCountry = Find(model.Code);
            if (dbCountry == null)
            {
                country = Mapper.Map<Country>(model);
                db.Countries.Add(country);
            }
            else
            {
                Mapper.Map(model, dbCountry);
            }

            db.SaveChanges();
            return dbCountry ?? country;
        }

        public void Delete(string code)
        {
            var regions = regionService.FindByCountryCode(code).ToList();
            foreach (var region in regions)
            {
                regionService.Delete(region.Id);
            }

            var country = Find(code);
            db.Countries.Remove(country);
            db.SaveChanges();
        }

        public void SetActive(string code, bool active)
        {
            var country = db.Countries.Find(code);
            country.IsActive = active;
            db.SaveChanges();
        }
    }
}