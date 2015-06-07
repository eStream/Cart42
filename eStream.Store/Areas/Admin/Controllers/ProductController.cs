using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;
using LinqKit;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class ProductController : BaseController
    {
        public const int DEFAULT_PAGE_SIZE = 20;

        private readonly IProductFinder productFinder;
        private readonly IDeleterService deleterService;
        private readonly IProductService productService;
        private readonly ITaxClassService taxClassService;
        private readonly ISettingService settingService;

        public ProductController(DataContext db, IProductFinder productFinder, IDeleterService deleterService,
            IProductService productService, ITaxClassService taxClassService, ISettingService settingService) : base(db)
        {
            this.productFinder = productFinder;
            this.deleterService = deleterService;
            this.productService = productService;
            this.taxClassService = taxClassService;
            this.settingService = settingService;
        }

        // GET: Admin/Product
        [AccessAuthorize(OperatorRoles.INVENTORY)]
        public ActionResult Index(int page = 1, int pageSize = DEFAULT_PAGE_SIZE, string keywords = null,
            int? categoryId = null, bool? featured = null, bool? visible = null, string orderColumn = null,
            bool orderAsc = true)
        {
            var model = prepareProductSearchViewModel(page, pageSize, keywords, categoryId, featured, visible,
                orderColumn, orderAsc);

            return View(model);
        }

        [AccessAuthorize(OperatorRoles.INVENTORY)]
        public StandardJsonResult List(int page = 1, int pageSize = DEFAULT_PAGE_SIZE, string keywords = null,
            int? categoryId = null, bool? featured = null, bool? visible = null, string orderColumn = null,
            bool orderAsc = true)
        {
            var model = prepareProductSearchViewModel(page, pageSize, keywords, categoryId, featured, visible,
                orderColumn, orderAsc);
            return JsonSuccess(model);
        }

        private ProductsIndexViewModel prepareProductSearchViewModel(int page = 1, int pageSize = DEFAULT_PAGE_SIZE,
            string keywords = null, int? categoryId = null, bool? featured = null, bool? visible = null,
            string orderColumn = null, bool orderAsc = true)
        {
            if (keywords == "null") keywords = null;
            var orderExpr = productFinder.CreateOrderExpr(orderColumn).Expand();
            var products = productFinder.Find(categoryId, false, keywords, featured, visible, 
                orderExpr: orderExpr, orderAsc: orderAsc);
            var model = new ProductsIndexViewModel
                        {
                            Page = page,
                            PageSize = pageSize,
                            TotalItems = products.Count(),
                            OrderColumn = orderColumn,
                            OrderAsc = orderAsc,
                            Keywords = keywords,
                        };
            model.TotalPages = ((model.TotalItems - 1)/pageSize) + 1;
            if (model.TotalPages < model.Page)
                model.Page = model.TotalPages;
            model.Products = Mapper.Map<List<ProductSearchViewModel>>(products.GetPage(model.Page, model.PageSize));
            return model;
        }

        // GET: Admin/Product/Create
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.WRITE)]
        public ActionResult Create()
        {
            var model = new ProductEditViewModel();

            ViewBag.TaxClassId = new SelectList(taxClassService.FindAll().ToList().OrderBy(r => r.Name), "Id", "Name");
            return View(model);
        }

        // POST: Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.WRITE)]
        public ActionResult Create([Bind(Exclude = "Id")]ProductEditViewModel productEditViewModel)
        {
            if (ModelState.IsValid)
            {
                var product = productService.CreateOrUpdate(productEditViewModel);

                var action = RedirectToAction("Index");

                if (settingService.Get<bool>(SettingField.ShowProductTutorial))
                {
                    settingService.Set(SettingField.ShowProductTutorial, false);
                    action = RedirectToAction("Welcome", "Home");
                }

                return action.WithSuccess(string.Format("Product \"{0}\" has been added".TA(), product.Name));
            }

            ViewBag.TaxClassId = new SelectList(taxClassService.FindAll().ToList().OrderBy(r => r.Name), "Id", "Name");
            return View(productEditViewModel);
        }

        // GET: Admin/Product/Edit/5
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.WRITE)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = productFinder.Find(id.Value);
            if (product == null)
            {
                return HttpNotFound();
            }

            var productEditModel = Mapper.Map<ProductEditViewModel>(product);
            productEditModel.CategoryIds = string.Join(",", product.Categories.Select(c => c.Id).ToArray());
            productEditModel.UploadIds = string.Join(",", product.Uploads.Select(u => u.Id).ToArray());
            productEditModel.OptionIds = string.Join(",", product.Options.Select(u => u.Id).ToArray());
            productEditModel.Sections = Mapper.Map<List<ProductSectionEditViewModel>>(product.Sections);
            productEditModel.Skus = new ProductSkusEditViewModel
                                    {
                                        Skus = Mapper.Map<List<ProductSkuEditViewModel>>(product.Skus)
                                    };

            ViewBag.TaxClassId = new SelectList(taxClassService.FindAll().ToList().OrderBy(r => r.Name), "Id", "Name");
            return View(productEditModel);
        }

        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.WRITE)]
        public ActionResult Edit(ProductEditViewModel productEditViewModel)
        {
            if (ModelState.IsValid)
            {
                var product = productService.CreateOrUpdate(productEditViewModel);
                
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("Product \"{0}\" have been updated".TA(), product.Name));
            }

            ViewBag.TaxClassId = new SelectList(taxClassService.FindAll().ToList().OrderBy(r => r.Name), "Id", "Name");
            return View(productEditViewModel);
        }

        // GET: Admin/Product/Delete/5
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.DELETE)]
        public ActionResult Delete(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<Product> products = productFinder.FindAll().Where(p => ids.Contains(p.Id)).ToList();
            if (!products.Any())
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // POST: Admin/Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.DELETE)]
        public ActionResult DeleteConfirmed(int[] ids)
        {
            ids.ForEach(id => deleterService.DeleteProduct(id));
            return RedirectToAction("Index")
                .WithWarning("The selected products have been deleted".TA());
        }

        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.WRITE)]
        public StandardJsonResult SetFeatured(int productId, bool featured)
        {
            productService.SetFeatured(productId, featured);

            return JsonSuccess(true);
        }

        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.WRITE)]
        public StandardJsonResult SetVisible(int productId, bool visible)
        {
            productService.SetVisible(productId, visible);

            return JsonSuccess(true);
        }

        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.WRITE)]
        public ActionResult GenerateSkus(ProductSkuGeneratorViewModel model)
        {
            var options = db.Options.Where(o => model.OptionIds.Any(id => o.Id == id)
                                                && o.Category.Type != OptionCategoryType.Text).ToArray();
            var categoryIds = options.Select(o => o.OptionCategoryId).Distinct().ToArray();

            var optionsPerm = OptionIdsPermutation(categoryIds, options);
            var skus = model.Skus;
            foreach (var optList in optionsPerm)
            {
                var sku = new ProductSkuEditViewModel();
                sku.Sku = (model.Prefix.AsNullIfEmpty() ?? "SKU-") + MiscHelpers.RandomText(8);
                sku.OptionIds = JsonConvert.SerializeObject(optList);

                if (skus.Any(s => s.OptionIds == sku.OptionIds))
                    continue;

                sku.Options = JsonConvert.SerializeObject(optList.Select(
                    id => new
                          {
                              category = db.Options.First(o => o.Id == id).Category.Name,
                              option = db.Options.First(o => o.Id == id).Name
                          }));
                skus.Add(sku);
            }
            
            return JsonSuccess(skus);
        }

        private List<List<int>> OptionIdsPermutation(int[] categoryIds, Option[] options)
        {
            var result = new List<List<int>>();
            foreach (var option in options.Where(o => o.OptionCategoryId == categoryIds.First()))
            {
                if (categoryIds.Length > 1)
                {
                    foreach (var subOptions in OptionIdsPermutation(categoryIds.Skip(1).ToArray(), options))
                    {
                        var optionIds = new List<int>();
                        optionIds.Add(option.Id);
                        optionIds.AddRange(subOptions);
                        result.Add(optionIds);
                    }
                }
                else
                {
                    var optionIds = new List<int>();
                    optionIds.Add(option.Id);
                    result.Add(optionIds);
                }
            }
            return result;
        }
    }
}