using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Http.Routing;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Models;
using UrlHelper = System.Web.Mvc.UrlHelper;

namespace Estream.Cart42.Web.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly DataContext db;
        private readonly HttpRequestBase httpRequest;
        private readonly ICacheService cacheService;

        public CategoryService(DataContext db, HttpRequestBase httpRequest, ICacheService cacheService)
        {
            this.db = db;
            this.httpRequest = httpRequest;
            this.cacheService = cacheService;
        }

        public IQueryable<Category> FindAll()
        {
            return db.Categories;
        }

        public Category Find(int id)
        {
            return db.Categories.Find(id);
        }

        public IQueryable<Category> FindByParent(int? parentId)
        {
            return db.Categories.Where(c => c.ParentId == parentId);
        }

        public BreadcrumbViewModel GetBreadcrumbNodes(int categoryId)
        {
            Category category = Find(categoryId);
            var nodes = new List<Tuple<string, string>>();
            int count = 0;
            var urlHelper = new UrlHelper(httpRequest.RequestContext);
            while (++count < 10)
            {
                nodes.Insert(0,
                    new Tuple<string, string>(
                        urlHelper.Action("Details", "Category", new { id = category.Id, slug = @category.Name.ToSlug() }),
                        category.Name));

                if (category.ParentId == null) break;
                category = category.Parent;
            }
            return new BreadcrumbViewModel {Nodes = nodes};
        }

        public int GetProductsCount(int categoryId, bool includeChildCategories)
        {
            return GetProductsCount(categoryId, includeChildCategories, 1);
        }

        public int GetProductsCount(int categoryId, bool includeChildCategories, int counter)
        {
            if (counter++ > 100) return 0; // Problem

            var count = db.Products.Count(p => p.Categories.Any(c => c.Id == categoryId));
            if (includeChildCategories)
            {
                var childCategoryIds = db.Categories.First(c => c.Id == categoryId).ChildCategories.Select(c => c.Id);
                count += childCategoryIds.Sum(id => GetProductsCount(id, true, counter));
            }
            return count;
        }

        public IList<int> GetChildIds(int? categoryId)
        {
            var ids = new List<int>();

            var childCategories = FindByParent(categoryId);
            foreach (var childCategory in childCategories.ToList())
            {
                ids.Add(childCategory.Id);
                ids.AddRange(GetChildIds(childCategory.Id));
            }

            return ids;
        }

        public Category AddOrUpdate(CategoryEditViewModel model)
        {
            Category category;
            if (model.Id == 0)
            {
                category = Mapper.Map<Category>(model);
                db.Categories.Add(category);
            }
            else
            {
                category = Find(model.Id);
                Mapper.Map(model, category);

                if (model.ParentId != null)
                {
                    // Check for recursion
                    var parentId = model.ParentId;
                    int counter = 0;
                    while (parentId.HasValue)
                    {
                        parentId = Find(parentId.Value).ParentId;
                        if (++counter >= 20)
                        {
                            model.ParentId = null;
                            break;
                        }
                    }
                }
            }

            db.SaveChanges();

            cacheService.Invalidate("categories");

            return category;
        }
    }
}