using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface IOrderService
    {
        IQueryable<Order> FindAll();
        Order Find(int id);
        void SetStatus(int id, OrderStatus status);
    }
}