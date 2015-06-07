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
    public class TaxRateService : ITaxRateService
    {
        private readonly DataContext db;

        public TaxRateService(DataContext db)
        {
            this.db = db;
        }

        public IQueryable<TaxRate> FindAll()
        {
            return db.TaxRates;
        }

        public TaxRate Find(int id)
        {
            return db.TaxRates.Find(id);
        }

        public IQueryable<TaxRate> FindByZone(int zoneId)
        {
            return db.TaxRates.Where(t => t.TaxZoneId == zoneId);
        }

        public void Delete(int id)
        {
            var taxRate = Find(id);
            var classRates = taxRate.ClassRates.Where(r => r.TaxRateId == taxRate.Id).ToList();
            foreach (var taxClassRate in classRates)
            {
                db.TaxClassRates.Remove(taxClassRate);
            }
            db.TaxRates.Remove(taxRate);
            db.SaveChanges();
        }

        public TaxRate AddOrUpdate(TaxRateEditViewModel model)
        {
            TaxRate taxRate;

            if (model.Id == 0)
            {
                taxRate = Mapper.Map<TaxRate>(model);
                db.TaxRates.Add(taxRate);
            }
            else
            {
                taxRate = Find(model.Id);
                taxRate = Mapper.Map(model, taxRate);

                var classRatesToDelete = taxRate.ClassRates.ToList().Where(r => !model.ClassRates.Any(cr => cr.TaxClassId == r.TaxClassId)
                    || model.ClassRates.Any(cr => cr.TaxClassId == r.TaxClassId && cr.Amount == null));
                foreach (var taxClassRate in classRatesToDelete)
                {
                    db.TaxClassRates.Remove(taxClassRate);
                }
            }

            foreach (var classRateView in model.ClassRates)
            {
                if (classRateView.Amount == null) continue;
                TaxClassRate classRate = null;
                if (model.Id != 0)
                {
                    classRate = taxRate.ClassRates.FirstOrDefault(r => r.TaxClassId == classRateView.TaxClassId);

                    // If rate exists in db and is changed in view
                    if (classRate != null)
                    {
                        classRate.Amount = classRateView.Amount.Value;
                    }
                }
                if (classRate == null)
                {
                    classRate = new TaxClassRate
                    {
                        TaxRateId = taxRate.Id,
                        TaxClassId = classRateView.TaxClassId,
                        Amount = classRateView.Amount.Value
                    };
                    db.TaxClassRates.Add(classRate);
                }
            }

            db.SaveChanges();
            return taxRate;
        }

        public decimal CalculateTax(int taxZoneID, int? taxClassId, decimal price)
        {
            decimal taxableAmount = price;
            decimal tax = 0m;
            List<IGrouping<int, TaxRate>> taxRateGrouped =
                db.TaxRates.Where(r => r.TaxZoneId == taxZoneID).GroupBy(r => r.Order).OrderBy(r => r.Key).ToList();
            foreach (var rateGroup in taxRateGrouped)
            {
                decimal groupTax = 0m;
                foreach (TaxRate rate in rateGroup)
                {
                    if (taxClassId.HasValue && rate.ClassRates.Any(cr => cr.TaxClassId == taxClassId))
                    {
                        TaxClassRate classRate = rate.ClassRates.First(cr => cr.TaxClassId == taxClassId);
                        groupTax += (taxableAmount * classRate.Amount) / 100m;
                    }
                    else
                    {
                        groupTax += (taxableAmount * rate.Amount) / 100m;
                    }
                }

                tax += groupTax;
                taxableAmount += groupTax;
            }

            return tax;
        }
    }
}