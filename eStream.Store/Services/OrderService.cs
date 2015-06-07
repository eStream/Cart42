using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public class OrderService : IOrderService
    {
        private readonly DataContext db;

        public OrderService(DataContext db)
        {
            this.db = db;
        }

        public IQueryable<Order> FindAll()
        {
            return db.Orders;
        }

        public Order Find(int id)
        {
            return db.Orders.Find(id);
        }

        public void SetStatus(int id, OrderStatus status)
        {
            var order = Find(id);
            order.Status = status;
            db.SaveChanges();

            // TODO: Send emails
        }       
    }
}