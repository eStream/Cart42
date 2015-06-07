using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public class TaxService : ITaxService
    {
        private readonly ITaxZoneService taxZoneService;
        private readonly ITaxRateService taxRateService;

        public TaxService(ITaxZoneService taxZoneService, ITaxRateService taxRateService)
        {
            this.taxZoneService = taxZoneService;
            this.taxRateService = taxRateService;
        }

        public decimal CalculateTax(string countryCode, int? regionId, int? taxClassId, decimal price)
        {
            TaxZone taxZone = taxZoneService.Find(countryCode, regionId);
            if (taxZone == null) return 0m;

            return taxRateService.CalculateTax(taxZone.Id, taxClassId, price);
        }
    }
}