using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface ICustomerService
    {
        IQueryable<User> FindAll();

        User Find(string id);

        IQueryable<User> Find(string keywords = null, Expression<Func<User, object>> orderExpr = null, 
            bool orderAsc = true);

        Address GetAddress(string userId, AddressType type);

        Expression<Func<User, object>> CreateOrderExpr(string orderColumn);

        User AddOrUpdate(CustomerViewModel model);

        void LoginUser(HttpContextBase context, User user);

        void Delete(string id);
    }
}