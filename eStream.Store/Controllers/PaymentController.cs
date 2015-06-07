using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Elmah;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.PaymentMethods;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Integrations.Payments;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Controllers
{
    public class PaymentController : BaseController
    {
        public PaymentController(DataContext db) : base(db)
        {
        }

        // GET: Payment/Success
        public ActionResult Success(FormCollection form)
        {
            return View();
        }

        // GET: Payment/Error
        public ActionResult Error(FormCollection form)
        {
            return View();
        }

        // POST: Payment/Ipn
        [HttpPost]
        public ActionResult Ipn(int id)
        {
            PaymentMethod paymentMethod = db.PaymentMethods.Find(id);
            if (paymentMethod == null)
                return HttpNotFound();

            // Find payment method implementation
            Type paymentType = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault(p => typeof (IPaymentMethod).IsAssignableFrom(p)
                                     && p.Name == paymentMethod.ClassName);
            if (paymentType == null)
                throw new NotImplementedException(paymentMethod.Name);
            var paymentClass = (IPaymentMethod) Activator.CreateInstance(paymentType);
            if (paymentMethod.Settings != null)
                paymentClass.Settings = JsonConvert.DeserializeObject<IPaymentMethodSettings>(paymentMethod.Settings);

            if (paymentClass is IPaymentMethodIpn)
            {
                IpnResult result = ((IPaymentMethodIpn) paymentClass).Ipn(Request);
                return Content(result.Response);
            }

            return new EmptyResult();
        }
    }
}