using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface IShippingService
    {
        ShippingZone FindZone(string countryCode, int? regionId);

        IQueryable<ShippingMethod> FindMethods(int zoneId);

        decimal? CalculateShipping(ShippingMethod method, int quantity, decimal totalWeight,
            decimal totalCost, Address address);
    }
}