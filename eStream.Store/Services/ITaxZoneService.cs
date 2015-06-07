using System.Linq;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface ITaxZoneService
    {
        IQueryable<TaxZone> FindAll();
        TaxZone Find(string countryCode, int? regionId);
        TaxZone AddOrUpdate(TaxZoneEditViewModel model);
        TaxZone Find(int id);
        void Delete(int id);
    }
}