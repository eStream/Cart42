using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution;
using Estream.Cart42.Web.DependencyResolution.PaymentMethods;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Integrations.Payments;
using Estream.Cart42.Web.Models;
using LinqKit;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Controllers
{
    [Authorize]
    public class OrderController : BaseController
    {
        public OrderController(DataContext db, ICurrentUser currentUser)
            : base(db, currentUser)
        {
        }

        // GET: Order/Payment?orderId=5&paymentMethodId=2
        public ActionResult Payment(int orderId, int paymentMethodId)
        {
            Order order = db.Orders.Include(o => o.BillingAddress).Include(o => o.ShippingAddress)
                .Include(o => o.Items).Include(o => o.Items.Select(i => i.Product))
                .First(o => o.Id == orderId);
            PaymentMethod paymentMethod = db.PaymentMethods.Find(paymentMethodId);

            switch (paymentMethod.Type)
            {
                case PaymentMethodType.Manual:
                    // Consider order placed and clear shopping cart
                    string userId = currentUser.User.Id;
                    order.Status = OrderStatus.AwaitingPayment;
                    var payment = new Payment
                                  {
                                      UserId = userId,
                                      OrderId = order.Id,
                                      PaymentMethodId = paymentMethod.Id,
                                      Status = PaymentStatus.Pending,
                                      Amount = order.Total,
                                      Date = DateTime.Now
                                  };
                    db.Payments.Add(payment);

                    ShoppingCart cart = db.ShoppingCarts.FirstOrDefault(c => c.UserId == userId);
                    if (cart != null)
                    {
                        Extensions.ForEach(cart.ShoppingCartItems.ToArray(), i => db.ShoppingCartItems.Remove(i));
                        db.ShoppingCarts.Remove(cart);
                    }

                    db.SaveChanges();

                    return RedirectToAction("Completed", new {orderId, paymentId = payment.Id});
                case PaymentMethodType.Hosted:
                    // Find payment method implementation
                    var paymentClass = ControllerContext.HttpContext.GetContainer()
                        .GetInstance<IPaymentMethod>(paymentMethod.ClassName);
                    if (paymentClass == null)
                        throw new NotImplementedException(paymentMethod.Name);

                        if (paymentMethod.Settings != null)
                        paymentClass.Settings =
                            JsonConvert.DeserializeObject<IPaymentMethodSettings>(paymentMethod.Settings);
                    if (paymentClass is IPaymentMethodIpn)
                        ((IPaymentMethodIpn) paymentClass).IpnUrl = Url.Action("Ipn", "Payment",
                            new {id = paymentMethod.Id}, Request.Url.Scheme);
                    return paymentClass.Process(order);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // GET: Order/Completed?orderId=5&paymentId=5
        public ActionResult Completed(int orderId, int paymentId)
        {
            Order order = db.Orders.Find(orderId);
            Payment payment = db.Payments.Find(paymentId);

            var model = new OrderCompletedViewModel
                        {
                            OrderId = order.Id,
                        };

            if (payment.Status == PaymentStatus.Pending && payment.PaymentMethod.Type == PaymentMethodType.Manual)
            {
                model.Message = payment.PaymentMethod.Settings;
            }

            return View(model);
        }
    }
}