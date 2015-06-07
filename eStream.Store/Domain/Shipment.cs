using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estream.Cart42.Web.Domain
{
    public class Shipment
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        public DateTime Date { get; set; }

        public string TrackingNo { get; set; }

        public int ShippingMethodId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        public virtual ICollection<ShipmentItem> Items { get; set; }
    }
}