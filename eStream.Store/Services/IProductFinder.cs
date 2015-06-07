using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface IProductFinder
    {
        IQueryable<Product> FindAll();

        Product Find(int id);

        IQueryable<Product> Find(int? categoryId = null, bool includeChildCategories = false,
            string keywords = null, bool? featured = null, bool? visible = true, int[] optionIds = null,
            Expression<Func<Product, object>> orderExpr = null, bool orderAsc = true);

        Expression<Func<Product, object>> CreateOrderExpr(string orderColumn);
    }
}