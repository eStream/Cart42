using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface ITaxRateService
    {
        IQueryable<TaxRate> FindAll();
        TaxRate Find(int id);
        IQueryable<TaxRate> FindByZone(int zoneID);
        void Delete(int id);
        TaxRate AddOrUpdate(TaxRateEditViewModel model);
        decimal CalculateTax(int taxZoneID, int? taxClassId, decimal price);
    }
}