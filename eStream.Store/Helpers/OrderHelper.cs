using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Helpers
{
    public class OrderHelper
    {
        private readonly DataContext db;

        public OrderHelper(DataContext db)
        {
            this.db = db;
        }

        public void OrderPaid(int orderId)
        {
            var order = db.Orders.Find(orderId);
            order.Status = OrderStatus.AwaitingFulfillment;
            db.SaveChanges();

            //TODO: Send notifications
        }

        public void OrderPaidPartially(int orderId)
        {
            var order = db.Orders.Find(orderId);

            //TODO: Send notifications
        }
    }
}