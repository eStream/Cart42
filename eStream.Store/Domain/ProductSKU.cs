using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using AutoMapper;

namespace Estream.Cart42.Web.Domain
{
    public class ProductSku
    {
        public ProductSku()
        {
            Options = new Collection<Option>();
            Uploads = new Collection<Upload>();
        }

        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string Sku { get; set; }

        public string UPC { get; set; }

        public int? Quantity { get; set; }

        public decimal? Price { get; set; }

        public decimal? Weight { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [IgnoreMap]
        public virtual ICollection<Option> Options { get; set; }

        [IgnoreMap]
        public virtual ICollection<Upload> Uploads { get; set; }

        [IgnoreMap]
        public virtual ICollection<ShoppingCartItem> ShoppingCartItems { get; set; }

        [IgnoreMap]
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}