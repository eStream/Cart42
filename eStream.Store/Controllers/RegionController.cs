using System.Linq;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Helpers;

namespace Estream.Cart42.Web.Controllers
{
    public class RegionController : BaseController
    {
        // POST: Region/List
        public RegionController(DataContext db) : base(db)
        {
        }

        public StandardJsonResult List(string countryCode)
        {
            var result = db.Regions.Where(r => r.CountryCode == countryCode)
                .Select(r => new {r.Id, r.Code, r.CountryCode, r.Name})
                .ToList();
            return JsonSuccess(result);
        }
    }
}