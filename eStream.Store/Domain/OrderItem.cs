using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper;
using Estream.Cart42.Web.Models;

namespace Estream.Cart42.Web.Domain
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int? ProductSkuId { get; set; }

        public int Quantity { get; set; }

        // Json of OrderItemOption[]
        public string Options { get; set; }

        public decimal ItemPrice { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("ProductSkuId")]
        public virtual ProductSku ProductSku { get; set; }

        public virtual ICollection<ShipmentItem> ShipmentItems { get; set; }

        public virtual ICollection<ReturnItem> ReturnItems { get; set; }
    }

    public class OrderItemOption : IHaveCustomMappings
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<ShoppingCartItemOptionViewModel, OrderItemOption>();
        }
    }
}