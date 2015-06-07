using System.Linq;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface IShipmentService
    {
        Shipment Find(int id);
        IQueryable<Shipment> FindAll();
        IQueryable<Shipment> FindByOrder(int orderId);

        Shipment AddOrUpdate(ShipmentEditViewModel model);
        
        void Delete(int id);
    }
}