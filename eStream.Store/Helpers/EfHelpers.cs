using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace Estream.Cart42.Web.Helpers
{
    public static class EfHelpers
    {
        public static IOrderedQueryable<T> OrderByMember<T>(
        this IQueryable<T> query,
        Expression<Func<T, object>> expression,
        SortOrder sortOrder)
        {
            var body = expression.Body as UnaryExpression;

            if (body != null)
            {
                var memberExpression = body.Operand as MemberExpression;

                if (memberExpression != null)
                {
                    return
                        (IOrderedQueryable<T>)
                        query.Provider.CreateQuery(
                            Expression.Call(
                                typeof(Queryable),
                                sortOrder == SortOrder.Ascending ? "OrderBy" : "OrderByDescending",
                                new[] { typeof(T), memberExpression.Type },
                                query.Expression,
                                Expression.Lambda(memberExpression, expression.Parameters)));
                }
            }

            return sortOrder == SortOrder.Ascending ? query.OrderBy(expression) : query.OrderByDescending(expression);
        }

        public static IEnumerable<T> InChunksOf<T>(this IOrderedQueryable<T> queryable, int chunkSize)
        {
            return queryable.ChunksOfSize(chunkSize).SelectMany(chunk => chunk);
        }

        public static IEnumerable<T[]> ChunksOfSize<T>(this IOrderedQueryable<T> queryable, int chunkSize)
        {
            int chunkNumber = 0;
            while (true)
            {
                var query = (chunkNumber == 0)
                    ? queryable
                    : queryable.Skip(chunkNumber * chunkSize);
                var chunk = query.Take(chunkSize).ToArray();
                if (chunk.Length == 0)
                    yield break;
                yield return chunk;
                chunkNumber++;
            }
        }

        public static bool None<TSource>(this IEnumerable<TSource> source)
        {
            return !source.Any();
        }

        public static bool None<TSource>(this IEnumerable<TSource> source,
                                         Func<TSource, bool> predicate)
        {
            return !source.Any(predicate);
        }
    }
}