using System.Linq;
using Estream.Cart42.Web.DAL;

namespace Estream.Cart42.Web.Services
{
    public class CategoryDeleter : ICategoryDeleter
    {
        private readonly DataContext db;
        private readonly ICategoryService categoryService;
        private readonly IDeleterService deleterService;
        private readonly ICacheService cacheService;

        public CategoryDeleter(DataContext db, ICategoryService categoryService,
            IDeleterService deleterService, ICacheService cacheService)
        {
            this.db = db;
            this.categoryService = categoryService;
            this.deleterService = deleterService;
            this.cacheService = cacheService;
        }

        public void Delete(int id)
        {
            var category = categoryService.Find(id);
            if (category == null) return;

            foreach (var childCategory in category.ChildCategories.ToList())
            {
                if (childCategory.Id == id) continue;
                Delete(childCategory.Id);
            }

            foreach (var product in category.Products.ToList())
            {
                if (product.Categories.Count() > 1)
                {
                    product.Categories.Remove(category);
                }
                else
                {
                    deleterService.DeleteProduct(product.Id);
                }
            }

            db.Categories.Remove(category);
            db.SaveChanges();

            cacheService.Invalidate("categories");
        }
    }
}