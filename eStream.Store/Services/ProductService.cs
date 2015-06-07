using System;
using System.Linq;
using System.Transactions;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Services
{
    public class ProductService : IProductService
    {
        private readonly DataContext db;
        private readonly IProductFinder productFinder;
        private readonly ICategoryService categoryService;
        private readonly IProductSkuService productSkuService;
        private readonly ICacheService cacheService;

        public ProductService(DataContext db, IProductFinder productFinder, ICategoryService categoryService,
            IProductSkuService productSkuService, ICacheService cacheService)
        {
            this.db = db;
            this.productFinder = productFinder;
            this.categoryService = categoryService;
            this.productSkuService = productSkuService;
            this.cacheService = cacheService;
        }

        public Product CreateOrUpdate(ProductEditViewModel productEditViewModel)
        {
            // Add or update product domain object
            Product product;
            if (productEditViewModel.Id > 0)
            {
                product = productFinder.Find(productEditViewModel.Id);
                product = Mapper.Map(productEditViewModel, product);
            }
            else
            {
                product = Mapper.Map<Product>(productEditViewModel);
                db.Products.Add(product);
            }

            // Set categories
            var categoryIds = string.IsNullOrEmpty(productEditViewModel.CategoryIds)
                ? new int[0]
                : productEditViewModel.CategoryIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => Convert.ToInt32(c)).ToArray();

            if (product.Id > 0)
            {
                foreach (var category in product.Categories.ToList())
                {
                    if (!categoryIds.Contains(category.Id))
                    {
                        product.Categories.Remove(category);
                    }
                }
            }
            foreach (int categoryId in categoryIds)
            {
                if (!product.Categories.Any(c => c.Id == categoryId))
                {
                    Category category = categoryService.Find(categoryId);
                    product.Categories.Add(category);
                }
            }

            // Add photos
            string[] uploadIds = string.IsNullOrEmpty(productEditViewModel.UploadIds)
                ? new string[0]
                : productEditViewModel.UploadIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (product.Id > 0)
            {
                foreach (Upload upload in product.Uploads.ToList())
                {
                    if (!uploadIds.Contains(upload.Id.ToString()))
                    {
                        db.Uploads.Remove(upload);
                    }
                }
            }
            foreach (string uploadId in uploadIds)
            {
                if (string.IsNullOrWhiteSpace(uploadId)) continue;
                Guid id = Guid.Parse(uploadId);
                Upload upload = db.Uploads.First(u => u.Id == id);
                upload.Product = product;
            }

            // Set options
            var optionIds = string.IsNullOrEmpty(productEditViewModel.OptionIds)
                ? new int[0]
                : productEditViewModel.OptionIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => Convert.ToInt32(c)).ToArray();

            if (product.Id > 0)
            {
                foreach (var option in product.Options.ToList())
                {
                    if (!optionIds.Contains(option.Id))
                    {
                        product.Options.Remove(option);
                    }
                }
            }
            foreach (int optionId in optionIds)
            {
                if (!product.Options.Any(o => o.Id == optionId))
                {
                    Option option = db.Options.Find(optionId);
                    product.Options.Add(option);
                }
            }

            if (product.Id > 0)
            {
                // Delete removed sections
                var sectionIds = productEditViewModel.Sections.Where(s => s.Id > 0).Select(s => s.Id).ToArray();
                foreach (var sectionToDelete in product.Sections.Where(s => !sectionIds.Contains(s.Id)).ToList())
                {
                    db.ProductSections.Remove(sectionToDelete);
                }
            }

            // Add/update sections
            foreach (var sectionViewModel in productEditViewModel.Sections)
            {
                var section = sectionViewModel.Id != 0
                    ? product.Sections.First(s => s.Id == sectionViewModel.Id)
                    : new ProductSection();
                Mapper.Map(sectionViewModel, section);

                if (section.Id == 0)
                    product.Sections.Add(section);
            }

            if (product.Id > 0)
            {
                // Delete removed skus
                var skus = productEditViewModel.Skus.Skus.Where(s => s.Id > 0).Select(s => s.Id).ToArray();
                foreach (var skusToDelete in product.Skus.Where(s => !skus.Contains(s.Id)).ToList())
                {
                    productSkuService.Delete(skusToDelete.Id);
                }
            }

            // Add/update skus
            foreach (var skuViewModel in productEditViewModel.Skus.Skus)
            {
                var sku = skuViewModel.Id != 0
                    ? product.Skus.FirstOrDefault(s => s.Id == skuViewModel.Id)
                    : new ProductSku();
                if (sku == null) continue;

                Mapper.Map(skuViewModel, sku);

                var skuOptionIds = JsonConvert.DeserializeObject<int[]>(skuViewModel.OptionIds);
                var skuUploadIds = JsonConvert.DeserializeObject<Guid[]>(skuViewModel.UploadIds);

                if (sku.Id > 0)
                {
                    foreach (var upload in sku.Uploads.ToList())
                    {
                        if (!skuUploadIds.Contains(upload.Id))
                        {
                            sku.Uploads.Remove(upload);
                        }
                    }
                }
                else
                {
                    product.Skus.Add(sku);
                }

                foreach (var optionId in skuOptionIds)
                {
                    var option = db.Options.Find(optionId);
                    if (option != null && !sku.Options.Any(o => o.Id == optionId))
                        sku.Options.Add(option);
                }
                foreach (var uploadId in skuUploadIds)
                {
                    var upload = db.Uploads.Find(uploadId);
                    if (upload != null && !sku.Uploads.Any(u => u.Id == uploadId))
                        sku.Uploads.Add(upload);
                }

            }

            db.SaveChanges();

            cacheService.Invalidate("products");
                
            return product;
        }

        public void SetFeatured(int id, bool featured)
        {
            var product = productFinder.Find(id);
            product.IsFeatured = featured;
            db.SaveChanges();

            cacheService.Invalidate("featured");
        }

        public void SetVisible(int id, bool visible)
        {
            var product = productFinder.Find(id);
            product.IsVisible = visible;
            db.SaveChanges();

            cacheService.Invalidate("products");
        }

        public void RemoveQuantity(int id, int quantity)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions {IsolationLevel = IsolationLevel.ReadCommitted}))
            {
                var product = productFinder.Find(id);
                product.Quantity -= quantity;

                db.SaveChanges();
                scope.Complete();
            }
        }
    }
}