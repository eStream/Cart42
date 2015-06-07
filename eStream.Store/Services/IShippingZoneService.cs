using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface IShippingZoneService
    {
        IQueryable<ShippingZone> FindAll();
        ShippingZone Find(string countryCode, int? regionId);
        ShippingZone Find(int id);
        ShippingZone AddOrUpdate(ShippingZoneEditViewModel model);
        void Delete(int id);
    }
}