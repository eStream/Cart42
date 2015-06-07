using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estream.Cart42.Web.Domain
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public OrderStatus Status { get; set; }

        public decimal Subtotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal ShippingAmount { get; set; }

        public decimal Discount { get; set; }

        public decimal Total { get; set; }

        public DateTime DatePlaced { get; set; }

        public DateTime DateUpdated { get; set; }

        public string IPAddress { get; set; }

        public string UserComments { get; set; }

        public string StaffComments { get; set; }

        public int BillingAddressId { get; set; }

        public int ShippingAddressId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("BillingAddressId")]
        public virtual Address BillingAddress { get; set; }

        [ForeignKey("ShippingAddressId")]
        public virtual Address ShippingAddress { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; }

        public virtual ICollection<Shipment> Shipments { get; set; }

        public virtual ICollection<Return> Returns { get; set; }
    }

    public enum OrderStatus
    {
        [Display(Name = "Awaiting Payment")] AwaitingPayment,
        [Display(Name = "Awaiting Fulfillment")] AwaitingFulfillment,
        [Display(Name = "Awaiting Shipment")] AwaitingShipment,
        [Display(Name = "Partially Shipped")] PartiallyShipped,
        [Display(Name = "Shipped")] Shipped,

        [Display(Name = "Partially Backordered")] PartiallyBackordered,
        [Display(Name = "Backordered")] Backordered,

        [Display(Name = "Partially Returned")] PartiallyReturned,
        [Display(Name = "Returned")] Returned,
        [Display(Name = "Cancelled")] Cancelled
    }
}