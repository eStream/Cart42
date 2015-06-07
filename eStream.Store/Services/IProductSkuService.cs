using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface IProductSkuService
    {
        ProductSku Find(int id);
        IQueryable<ProductSku> FindAll();
        IQueryable<ProductSku> Find(int productId, IEnumerable<int> optionIds);

        void RemoveQuantity(int id, int quantity);

        void Delete(int id);
    }
}