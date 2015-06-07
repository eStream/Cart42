using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Estream.Cart42.Web.Areas.Admin.Models;

namespace Estream.Cart42.Web.Domain
{
    public class ProductSection
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string Title { get; set; }

        // Text or JSON depending on the type
        public string Settings { get; set; }

        public ProductSectionType Type { get; set; }

        public ProductSectionPosition Position { get; set; }

        public int Priority { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }

    public enum ProductSectionType
    {
        Text = 1,
        SimilarProducts = 2,
        Reviews = 3
    }

    public enum ProductSectionPosition
    {
        Tabs = 1
    }

    public class ProductSectionTextSettings
    {
        public string Text { get; set; }
    }
}