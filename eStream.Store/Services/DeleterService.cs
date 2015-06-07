using System.Linq;
using Elmah.ContentSyndication;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public class DeleterService : IDeleterService
    {
        private readonly DataContext db;
        private readonly ICacheService cacheService;

        public DeleterService(DataContext db, ICacheService cacheService)
        {
            this.db = db;
            this.cacheService = cacheService;
        }

        public void DeleteProduct(int id)
        {
            var product = db.Products.Find(id);

            foreach (var upload in product.Uploads.ToList())
            {
                db.Uploads.Remove(upload);
            }

            foreach (var productSection in product.Sections.ToList())
            {
                db.ProductSections.Remove(productSection);
            }

            foreach (var itemId in db.ShoppingCartItems.Select(s => s.Id).ToList())
            {
                DeleteShoppingCartItem(itemId);
            }

            foreach (var itemId in db.OrderItems.Select(s => s.Id).ToList())
            {
                DeleteOrderItem(itemId);
            }

            foreach (var optionId in product.Options.Select(s => s.Id).ToList())
            {
                DeleteOption(optionId);
            }

            foreach (var skuId in product.Skus.Select(s => s.Id).ToList())
            {
                DeleteProductSku(skuId);
            }
            
            db.Products.Remove(product);
            db.SaveChanges();

            cacheService.Invalidate("products");
        }

        public void DeleteProductSku(int id)
        {
            var sku = db.ProductSkus.Find(id);

            foreach (var orderItemId in sku.OrderItems.Select(oi => oi.Id).ToList())
            {
                DeleteOrderItem(orderItemId);
            }

            foreach (var cartItemId in sku.ShoppingCartItems.Select(ci => ci.Id).ToList())
            {
                DeleteShoppingCartItem(cartItemId);
            }

            db.ProductSkus.Remove(sku);
            db.SaveChanges();
        }

        public void DeleteOrderItem(int id)
        {
            var item = db.OrderItems.Find(id);

            foreach (var returnItemId in item.ReturnItems.Select(ri => ri.Id).ToList())
            {
                DeleteReturnItem(returnItemId);
            }

            foreach (var shipmentItemId in item.ShipmentItems.Select(si => si.Id).ToList())
            {
                DeleteShipmentItem(shipmentItemId);
            }

            db.OrderItems.Remove(item);
            db.SaveChanges();
        }

        public void DeleteShoppingCartItem(int id)
        {
            var item = db.ShoppingCartItems.Find(id);
            db.ShoppingCartItems.Remove(item);
            db.SaveChanges();
        }

        public void DeleteReturnItem(int id)
        {
            var item = db.ReturnItems.Find(id);
            db.ReturnItems.Remove(item);
            db.SaveChanges();
        }

        public void DeleteShipmentItem(int id)
        {
            var item = db.ShipmentItems.Find(id);
            db.ShipmentItems.Remove(item);
            db.SaveChanges();
        }

        public void DeleteOption(int id)
        {
            var option = db.Options.Find(id);

            foreach (var skuId in option.ProductSkus.Select(s => s.Id).ToList())
            {
                DeleteProductSku(skuId);
            }

            db.Options.Remove(option);
            db.SaveChanges();
        }

        public void DeleteBlogPostComment(int id)
        {
            var comment = db.BlogPostComments.Find(id);
            db.BlogPostComments.Remove(comment);
            db.SaveChanges();
        }

        public void DeleteBlogPost(int id)
        {
            var blogPost = db.BlogPosts.Find(id);
            var comments = db.BlogPostComments.Where(c => c.BlogPostId == id).ToList();
            foreach (var comment in comments)
            {
                DeleteBlogPostComment(comment.Id);
            }
            db.BlogPosts.Remove(blogPost);
            db.SaveChanges();
            cacheService.Invalidate("blogs");
        }

        public void DeleteBlog(int id)
        {
            var blog = db.Blogs.Find(id);
            var posts = db.BlogPosts.Where(p => p.BlogId == id).ToList();
            foreach (var post in posts)
            {
                DeleteBlogPost(post.Id);
            }
            db.Blogs.Remove(blog);
            db.SaveChanges();
            cacheService.Invalidate("blogs");
        }

        public void DeleteReturn(int id)
        {
            Return rtn = db.Returns.Find(id);
            db.Returns.Remove(rtn);
            db.SaveChanges();
        }

        public void DeleteShipment(int id)
        {
            Shipment shipment = db.Shipments.Find(id);
            var items = shipment.Items.ToList();
            foreach (var shipmentItem in items)
            {
                DeleteShipmentItem(shipmentItem.Id);
            }
            db.Shipments.Remove(shipment);
            db.SaveChanges();

        }

        public void DeleteOrder(int id)
        {
            Order order = db.Orders.Find(id);
            var items = order.Items.ToList();
            foreach (var orderItem in items)
            {
                DeleteOrderItem(orderItem.Id);
            }
            var shipments = order.Shipments.ToList();
            foreach (var shipment in shipments)
            {
                DeleteShipment(shipment.Id);
            }
            var returns = order.Returns.ToList();
            foreach (var @return in returns)
            {
                DeleteReturn(@return.Id);
            }
            db.Orders.Remove(order);
            db.SaveChanges();
        }
    }
}