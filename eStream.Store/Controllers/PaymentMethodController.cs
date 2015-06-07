using System.Linq;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;

namespace Estream.Cart42.Web.Controllers
{
    public class PaymentMethodController : BaseController
    {
        public PaymentMethodController(DataContext db) : base(db)
        {
        }

        public StandardJsonResult List(string countryCode)
        {
            var result = db.PaymentMethods.Where(
                p => p.IsActive && (!p.Countries.Any() || p.Countries.Any( c => c.Code == countryCode)))
                .Select(p => new
                             {
                                 p.Id, p.Name, p.Type
                             });

            return JsonSuccess(result.ToList());
        }
    }
}