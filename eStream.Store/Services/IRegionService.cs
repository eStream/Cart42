using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface IRegionService
    {
        Region Find(int id);
        IQueryable<Region> FindAll();
        IQueryable<Region> FindByCountryCode(string countryCode);
        Region AddOrUpdate(RegionViewModel region);
        void Delete(int id);
    }
}