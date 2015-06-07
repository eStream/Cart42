using System;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace Estream.Cart42.Web.Helpers
{
    public static class DataHelper
    {
        public static IQueryable<T> GetPage<T>(this IQueryable<T> query, int page, int pageSize)
        {
            if (page > 1)
                query = query.Skip((page - 1)*pageSize);
            query = query.Take(pageSize);
            return query;
        }
    }
}