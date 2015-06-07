using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estream.Cart42.Web.Domain
{
    public class ShoppingCartItem
    {
        [Key]
        public int Id { get; set; }

        public Guid ShoppingCardID { get; set; }

        public int ProductId { get; set; }

        public int? ProductSkuId { get; set; }

        public int Quantity { get; set; }

        public string Options { get; set; }

        public decimal ItemPrice { get; set; }

        [ForeignKey("ShoppingCardID")]
        public virtual ShoppingCart ShoppingCart { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("ProductSkuId")]
        public virtual ProductSku ProductSku { get; set; }
    }
}