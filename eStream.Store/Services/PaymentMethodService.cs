using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly DataContext db;
        private readonly ICountryService countryService;

        public PaymentMethodService(DataContext db, ICountryService countryService)
        {
            this.db = db;
            this.countryService = countryService;
        }

        public IQueryable<PaymentMethod> FindAll()
        {
            return db.PaymentMethods;
        }

        public PaymentMethod Find(int id)
        {
            return db.PaymentMethods.Find(id);
        }

        public PaymentMethod Update(PaymentMethodViewModel model)
        {
            var method = db.PaymentMethods.First(m => m.Id == model.Id);
            Mapper.Map(model, method);
            method.Countries.Clear();
            foreach (var code in model.CountryCodes)
            {
                Country country = countryService.Find(code);
                method.Countries.Add(country); 
            }
            db.SaveChanges();
            return method;
        }
    }
}