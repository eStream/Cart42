using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Services;
using Glimpse.AspNet.Tab;

namespace Estream.Cart42.Web.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly ICategoryService categoryService;
        private readonly ICacheService cacheService;

        public CategoryController(ICategoryService categoryService, ICacheService cacheService)
        {
            this.categoryService = categoryService;
            this.cacheService = cacheService;
        }

        [ChildActionOnly]
        //[OutputCache(Duration = 5*60, VaryByParam = "*", VaryByCustom = "lang,categories")]
        public ActionResult Index(int? parentId, string viewName, bool includeSubcategories = false)
        {
            var categories = categoryService.FindByParent(parentId);
            if (includeSubcategories)
                categories = categories.Include(c => c.ChildCategories);

            var cacheKey = string.Format("categories_{0}_{1}", parentId, includeSubcategories);
            List<Category> model = cacheService.Get(cacheKey, () => categories.ToList(), invalidateKeys: "categories",
                absoluteExpiration: DateTime.Now.AddMinutes(60));

            return PartialView(viewName ?? "_CategoriesMenu", model);
        }

        [ChildActionOnly]
        [OutputCache(Duration = 5 * 60, VaryByParam = "*", VaryByCustom = "lang,categories")]
        public ActionResult Breadcrumb(int id)
        {
            var model = categoryService.GetBreadcrumbNodes(id);
            return PartialView("_Breadcrumb", model);
        }

        // GET: Category/Details/5
        public ActionResult Details(int id, int[] opt = null)
        {
            Category category = categoryService.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = id;
            ViewBag.OptionIds = opt;
            return View(category);
        }
    }
}