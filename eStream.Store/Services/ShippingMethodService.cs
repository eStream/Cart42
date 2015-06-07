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
    public class ShippingMethodService : IShippingMethodService
    {
        private readonly DataContext db;

        public  ShippingMethodService(DataContext db)
        {
            this.db = db;
        }

        public IQueryable<ShippingMethod> FindAll()
        {
            return db.ShippingMethods;
        }

        public ShippingMethod Find(int id)
        {
            return db.ShippingMethods.Find(id);
        }

        public IQueryable<ShippingMethod> FindByZone(int shippingZoneId)
        {
            return db.ShippingMethods.Where(z => z.ShippingZone.Id == shippingZoneId && z.ShippingZone.IsActive);
        }

        public ShippingMethod AddOrUpdate(ShippingMethodEditViewModel model)
        {
            ShippingMethod shippingMethod;

            if (model.Id == 0)
            {
                shippingMethod = Mapper.Map<ShippingMethod>(model);
                db.ShippingMethods.Add(shippingMethod);
            }
            else
            {
                shippingMethod = Find(model.Id);
                shippingMethod = Mapper.Map(model, shippingMethod);
            }
            db.SaveChanges();

            return shippingMethod;
        }

        public void Delete(int id)
        {
            ShippingMethod shippingMethod = Find(id);
            db.ShippingMethods.Remove(shippingMethod);
            db.SaveChanges();
        }
    }
}