using System;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using LinqKit;

namespace Estream.Cart42.Web.Services
{
    public class ProductFinder : IProductFinder
    {
        private readonly DataContext db;
        private readonly ICategoryService categoryService;

        public ProductFinder(DataContext db, ICategoryService categoryService)
        {
            this.db = db;
            this.categoryService = categoryService;
        }

        public IQueryable<Product> FindAll()
        {
            return db.Products;
        }

        public Product Find(int id)
        {
            return db.Products.Find(id);
        }

        public IQueryable<Product> Find(int? categoryId = null, bool includeChildCategories = false,
            string keywords = null, bool? featured = null, bool? visible = true, int[] optionIds = null,
            Expression<Func<Product, object>> orderExpr = null, bool orderAsc = true)
        {
            IQueryable<Product> products = db.Products.AsQueryable();
            Expression<Func<Product, bool>> whereExpr = PredicateBuilder.True<Product>();

            #region Category filter

            if (categoryId.HasValue)
            {
                Expression<Func<Product, bool>> categoriesExpr = p => p.Categories.Any(c => c.Id == categoryId);
                if (includeChildCategories)
                {
                    var childIds = categoryService.GetChildIds(categoryId);
                    foreach (var childId in childIds)
                    {
                        categoriesExpr = categoriesExpr.Or(p => p.Categories.Any(c => c.Id == childId));
                    }
                }
                whereExpr = whereExpr.And(categoriesExpr.Expand());
            }

            #endregion

            #region Keywords filter

            if (!string.IsNullOrWhiteSpace(keywords))
            {
                Expression<Func<Product, bool>> keywordsExpr = PredicateBuilder.False<Product>();

                foreach (string keyword in keywords.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries))
                {
                    // Check for noise
                    if (keyword.Length <= 1) continue;

                    //TODO: Check for noise words from list

                    string key = keyword;
                    keywordsExpr = keywordsExpr.Or(p => p.Sku == key || p.Name.Contains(key) || p.Keywords.Contains(key));
                }

                whereExpr = whereExpr.And(keywordsExpr.Expand());
            }

            #endregion

            #region Other filters

            if (featured.HasValue)
                whereExpr = whereExpr.And(p => p.IsFeatured == featured);

            if (visible.HasValue)
                whereExpr = whereExpr.And(p => p.IsVisible == visible);

            #endregion

            #region Option filters

            if (optionIds != null && optionIds.Any())
            {
                Expression<Func<Product, bool>> optionsExpr = PredicateBuilder.True<Product>();
                foreach (var optionId in optionIds)
                {
                    optionsExpr = optionsExpr.And(p => p.Options.Any(o => o.Id == optionId));
                }
                whereExpr = whereExpr.And(optionsExpr.Expand());
            }

            #endregion

            // Apply filters
            products = products.Where(whereExpr.Expand());

            // Apply sorting
            products = products.OrderByMember(orderExpr ?? (p => p.Id),
                orderAsc ? SortOrder.Ascending : SortOrder.Descending);

            return products;
        }

        public Expression<Func<Product, object>> CreateOrderExpr(string orderColumn)
        {
            if (orderColumn == null) return null;

            switch (orderColumn)
            {
                case "sku":
                    return p => p.Sku;
                case "name":
                    return p => p.Name;
                case "price":
                    return p => p.Price;
                case "quantity":
                    return p => p.Quantity;
                default:
                    return null;
            }
        }
    }
}