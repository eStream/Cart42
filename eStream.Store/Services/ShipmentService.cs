using System.Linq;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly DataContext db;

        public ShipmentService(DataContext db)
        {
            this.db = db;
        }

        public Shipment Find(int id)
        {
            return db.Shipments.Find(id);
        }

        public IQueryable<Shipment> FindAll()
        {
            return db.Shipments;
        }

        public IQueryable<Shipment> FindByOrder(int orderId)
        {
            return db.Shipments.Where(s => s.OrderId == orderId);
        }

        public Shipment AddOrUpdate(ShipmentEditViewModel model)
        {
            Shipment shipment;
            if (model.Id == 0)
            {
                shipment = Mapper.Map<Shipment>(model);
                db.Shipments.Add(shipment);
            }
            else
            {
                shipment = Find(model.Id);
                Mapper.Map(model, shipment);
            }

            db.SaveChanges();
            return shipment;
        }

        public void Delete(int id)
        {
            var shipment = Find(id);

            foreach (var shipmentItem in shipment.Items.ToList())
            {
                db.ShipmentItems.Remove(shipmentItem);
            }

            db.Shipments.Remove(shipment);

            db.SaveChanges();
        }
    }
}