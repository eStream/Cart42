using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Models;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Controllers
{
    public class ProductController : BaseController
    {
        private readonly ISettingService settings;
        private readonly ICategoryService categoryService;
        private readonly IProductFinder productFinder;
        private readonly IUploadService uploadService;

        public ProductController(ISettingService settings, ICategoryService categoryService,
            IProductFinder productFinder, IUploadService uploadService)
        {
            this.settings = settings;
            this.categoryService = categoryService;
            this.productFinder = productFinder;
            this.uploadService = uploadService;
        }

        [ChildActionOnly]
        public ActionResult Index(int? categoryId, string viewName, string keywords = null, bool? featured = null,
            int? count = null, int? pageSize = null, int[] optionIds = null)
        {
            ProductGridViewModel model = loadProductGridViewModel(categoryId, 1,
                pageSize ?? settings.Get<int>(SettingField.ProductsPerPage), keywords, featured, count,
                optionIds);

            return PartialView(viewName, model);
        }

        public JsonResult List(int? categoryId, int page = 1, int pageSize = 0, string keywords = null,
            bool? featured = null, int[] optionIds = null)
        {
            if (pageSize == 0)
                pageSize = settings.Get<int>(SettingField.ProductsPerPage);

            ProductGridViewModel model = loadProductGridViewModel(categoryId, page, pageSize, keywords: keywords,
                featured: featured, optionIds: optionIds);

            return JsonSuccess(model);
        }

        [ChildActionOnly]
        public ActionResult Related(int productId, string viewName, int? count = null)
        {
            var categoryId = productFinder.Find(productId).Categories.First().Id;

            ProductGridViewModel model = loadProductGridViewModel(categoryId, 1,
                count ?? settings.Get<int>(SettingField.ProductsPerPage), maxCount: count,
                excludeProductId: productId);
            
            return PartialView(viewName, model);
        }

        private ProductGridViewModel loadProductGridViewModel(int? categoryId, int page, int pageSize, string keywords = null, bool? featured = null, 
            int? maxCount = null, int[] optionIds = null, int? excludeProductId = null)
        {
            IQueryable<Product> products = productFinder.Find(categoryId, keywords: keywords, featured: featured,
                optionIds: optionIds);
            if (excludeProductId.HasValue)
                products = products.Where(p => p.Id != excludeProductId);
            if (maxCount.HasValue)
                products = products.Take(maxCount.Value);
            var count = products.Count();

            if (count == 0)
            {
                products = productFinder.Find(categoryId, keywords: keywords, includeChildCategories: true,
                    featured: featured, optionIds: optionIds);
                if (excludeProductId.HasValue)
                    products = products.Where(p => p.Id != excludeProductId);
                if (maxCount.HasValue)
                    products = products.Take(maxCount.Value);
                count = products.Count();
            }

            var model = new ProductGridViewModel();
            model.Page = page;
            model.TotalProducts = count;
            model.TotalPages = ((model.TotalProducts - 1)/pageSize) + 1;
            model.Products = Mapper.Map<ProductViewModel[]>(products.GetPage(page, pageSize));
            foreach (var productViewModel in model.Products)
            {
                productViewModel.Url = Url.Action("Details",
                    new {id = productViewModel.Id, slug = productViewModel.Name.ToSlug()});
                productViewModel.PrimaryPhotoId = uploadService.FindPrimaryIdByProduct(productViewModel.Id);

            }
            model.CategoryId = categoryId;
            model.Keywords = keywords;
            model.Featured = featured;
            return model;
        }

        // GET: Product/Details/5
        public ActionResult Details(int id)
        {
            Product product = productFinder.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            var model = Mapper.Map<ProductDetailsViewModel>(product);
            var optionCategories = product.Options.Select(o => o.Category).Distinct();
            model.OptionCategories = Mapper.Map<List<ProductOptionCategoryViewModel>>(optionCategories);
            foreach (var optionCategory in model.OptionCategories)
            {
                var optionCategoryId = optionCategory.Id;
                optionCategory.Options = Mapper.Map<List<ProductOptionViewModel>>(
                    product.Options.Where(o => o.OptionCategoryId == optionCategoryId));
            }
            var productSections = product.Sections.ToList();
            model.Sections = Mapper.Map<List<ProductSectionViewModel>>(productSections);

            return View(model);
        }

        [ChildActionOnly]
        [OutputCache(Duration = 30 * 60, VaryByParam = "*")]
        public ActionResult Breadcrumb(int id)
        {
            Product product = productFinder.Find(id);
            var model = categoryService.GetBreadcrumbNodes(product.Categories.Select(c => c.Id).First());
            model.Nodes.Add(new Tuple<string, string>(Url.Action("Details", "Product",
                new {id = product.Id, slug = product.Name.ToSlug()}), product.Name));

            return PartialView("_Breadcrumb", model);
        }
    }
}