using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Antlr.Runtime;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Glimpse.Core.Configuration;

namespace Estream.Cart42.Web.Helpers
{
    public class PaymentHelper
    {
        private readonly DataContext db;
        private readonly OrderHelper orderHelper;

        public PaymentHelper(DataContext db, OrderHelper orderHelper)
        {
            this.db = db;
            this.orderHelper = orderHelper;
        }

        public void LogPayment(int orderId, string paymentMethodClass, PaymentStatus status, decimal? amount, string notes)
        {
            Order order = db.Orders.Find(orderId);
            var paymentMethodId = db.PaymentMethods.First(p => p.ClassName == paymentMethodClass).Id;
            if (order != null)
            {
                var payment = new Payment
                {
                    Date = DateTime.Now,
                    Amount = amount ?? order.Total,
                    OrderId = order.Id,
                    PaymentMethodId = paymentMethodId,
                    Status = status,
                    Notes = notes,
                    UserId = order.UserId
                };
                db.Payments.Add(payment);
                db.SaveChanges();

                if (status == PaymentStatus.Completed)
                {
                    // Clear shopping cart

                    if (amount == null || amount == order.Total)
                    {
                        orderHelper.OrderPaid(orderId);
                    }
                    else
                    {
                        orderHelper.OrderPaidPartially(orderId);
                    }
                }
            }
        }
    }
}