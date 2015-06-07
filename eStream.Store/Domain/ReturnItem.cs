using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Estream.Cart42.Web.Domain
{
    public class ReturnItem
    {
        [Key]
        public int Id { get; set; }

        public int ReturnId { get; set; }

        public int OrderItemId { get; set; }

        public int Quantity { get; set; }

        [ForeignKey("ReturnId")]
        public virtual Return Return { get; set; }

        [ForeignKey("OrderItemId")]
        public virtual OrderItem OrderItem { get; set; }
    }
}