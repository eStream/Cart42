using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Services
{
    public class ShippingService : IShippingService
    {
        private readonly DataContext db;
        private readonly IShippingZoneService shippingZoneService;

        public ShippingService(DataContext db, IShippingZoneService shippingZoneService)
        {
            this.db = db;
            this.shippingZoneService = shippingZoneService;
        }

        public ShippingZone FindZone(string countryCode, int? regionId)
        {
            return shippingZoneService.Find(countryCode, regionId);
        }

        public IQueryable<ShippingMethod> FindMethods(int zoneId)
        {
            return db.ShippingMethods.Where(m => m.ShippingZoneId == zoneId);
        }

        public decimal? CalculateShipping(ShippingMethod method, int quantity, decimal totalWeight, decimal totalCost, Address address)
        {
            switch (method.Type)
            {
                case ShippingMethodType.Free:
                    if (method.FreeShippingMinTotal.HasValue && totalCost < method.FreeShippingMinTotal)
                        return null;
                    return 0m;
                case ShippingMethodType.Flat:
                    if (!method.FlatRateAmount.HasValue) return null;
                    return method.FlatRateAmount.Value * (method.FlatRatePerItem ? quantity : 1);
                case ShippingMethodType.ByWeight:
                    if (string.IsNullOrWhiteSpace(method.WeightRanges)) return null;
                    var weightRanges = JsonConvert.DeserializeObject<ShippingByRange[]>(method.WeightRanges);
                    ShippingByRange weightRange =
                        weightRanges.FirstOrDefault(r => totalWeight >= r.From && totalWeight <= r.To);
                    if (weightRange == null) return null;
                    return weightRange.Amount;
                case ShippingMethodType.ByTotal:
                    if (string.IsNullOrWhiteSpace(method.TotalRanges)) return null;
                    var totalRanges = JsonConvert.DeserializeObject<ShippingByRange[]>(method.WeightRanges);
                    ShippingByRange totalRange =
                        totalRanges.FirstOrDefault(r => totalCost >= r.From && totalCost <= r.To);
                    if (totalRange == null) return null;
                    return totalRange.Amount;
            }

            return null;
        }
    }
}