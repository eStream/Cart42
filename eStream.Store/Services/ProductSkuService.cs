using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using IsolationLevel = System.Data.IsolationLevel;

namespace Estream.Cart42.Web.Services
{
    public class ProductSkuService : IProductSkuService
    {
        private readonly DataContext db;

        public ProductSkuService(DataContext db)
        {
            this.db = db;
        }

        public ProductSku Find(int id)
        {
            return db.ProductSkus.Find(id);
        }

        public IQueryable<ProductSku> FindAll()
        {
            return db.ProductSkus;
        }

        public IQueryable<ProductSku> Find(int productId, IEnumerable<int> optionIds)
        {
            var result = from s in db.ProductSkus
                where s.ProductId == productId
                      && s.Options.All(o => optionIds.Contains(o.Id))
                select s;

            return result;
        }

        public void RemoveQuantity(int id, int quantity)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions {IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted}))
            {
                var sku = Find(id);
                sku.Quantity -= quantity;

                db.SaveChanges();
                scope.Complete();
            }
        }

        public void Delete(int id)
        {
            var sku = Find(id);

            foreach (var orderItem in sku.OrderItems.ToList())
            {
                db.OrderItems.Remove(orderItem);
            }

            foreach (var cartItem in sku.ShoppingCartItems.ToList())
            {
                db.ShoppingCartItems.Remove(cartItem);
            }

            db.ProductSkus.Remove(sku);

            db.SaveChanges();
        }
    }
}