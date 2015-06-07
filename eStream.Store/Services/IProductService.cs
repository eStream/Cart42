using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface IProductService
    {
        Product CreateOrUpdate(ProductEditViewModel productEditViewModel);
        void SetFeatured(int id, bool featured);
        void SetVisible(int id, bool visible);
        void RemoveQuantity(int id, int quantity);
    }
}