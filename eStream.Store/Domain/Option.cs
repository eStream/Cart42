using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estream.Cart42.Web.Domain
{
    [DisplayName("Product option")]
    public class Option
    {
        public Option()
        {
            Products = new Collection<Product>();
            ProductSkus = new Collection<ProductSku>();
        }

        [Key]
        public int Id { get; set; }

        public int OptionCategoryId { get; set; }

        [ForeignKey("OptionCategoryId")]
        public virtual OptionCategory Category { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Product> Products { get; set; }

        public virtual ICollection<ProductSku> ProductSkus { get; set; }
    }
}