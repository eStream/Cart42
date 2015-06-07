using System;
using System.Collections.Generic;
using System.Linq;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Models;

namespace Estream.Cart42.Web.Services
{
    public interface ICategoryService
    {
        IQueryable<Category> FindAll();

        Category Find(int id);

        IQueryable<Category> FindByParent(int? parentId);

        BreadcrumbViewModel GetBreadcrumbNodes(int categoryId);

        int GetProductsCount(int categoryId, bool includeChildCategories = false);

        IList<int> GetChildIds(int? categoryId);

        Category AddOrUpdate(CategoryEditViewModel model);
    }
}