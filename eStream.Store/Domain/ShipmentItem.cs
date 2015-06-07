using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estream.Cart42.Web.Domain
{
    public class ShipmentItem
    {
        [Key]
        public int Id { get; set; }

        public int ShipmentId { get; set; }

        public int OrderItemId { get; set; }

        public int Quantity { get; set; }

        [ForeignKey("ShipmentId")]
        public virtual Shipment Shipment { get; set; }

        [ForeignKey("OrderItemId")]
        public virtual OrderItem OrderItem { get; set; }
    }
}