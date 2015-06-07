using System;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public class TaxClassService : ITaxClassService
    {
        private readonly DataContext db;

        public TaxClassService(DataContext db)
        {
            this.db = db;
        }

        public IQueryable<TaxClass> FindAll()
        {
            return db.TaxClasses;
        }

        public TaxClass AddOrUpdate(TaxClassViewModel model)
        {
            TaxClass taxClass;
            if (model.Id == 0)
            {
                taxClass = Mapper.Map<TaxClass>(model);
                db.TaxClasses.Add(taxClass);
            }
            else
            {
                taxClass = Find(model.Id);
                Mapper.Map(model, taxClass);
            }

            db.SaveChanges();
            return taxClass;
        }

        public TaxClass Find(int id)
        {
            return db.TaxClasses.Find(id);
        }

        public void Delete(int id)
        {
            var taxClass = Find(id);
            var products = db.Products.Where(p => p.TaxClassId == taxClass.Id).ToList();
            foreach (var product in products)
            {
                product.TaxClassId = null;
            }
            var taxClassRates =  db.TaxClassRates.Where(cr => cr.TaxClassId == taxClass.Id).ToList();
            foreach (var taxClassRate in taxClassRates)
            {
                db.TaxClassRates.Remove(taxClassRate);
            }
            
            db.TaxClasses.Remove(taxClass);
            db.SaveChanges();
        }
    }
}