using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using CsvHelper.Configuration;

namespace Estream.Cart42.Web.Domain
{
    public class Product
    {
        public Product()
        {
            Categories = new Collection<Category>();
            Options = new Collection<Option>();
            Uploads = new Collection<Upload>();
            Sections = new Collection<ProductSection>();
            Skus = new Collection<ProductSku>();
        }

        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        [Index("SearchIndex", 1)]
        public string Sku { get; set; }

        public string UPC { get; set; }

        [Required, MaxLength(1000), Index("SearchIndex", 2)]
        public string Name { get; set; }

        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [DataType(DataType.Currency)]
        public decimal? CostPrice { get; set; }

        [DataType(DataType.Currency)]
        public decimal? RetailPrice { get; set; }

        [DataType(DataType.Currency)]
        public decimal? SalePrice { get; set; }
        
        public decimal Weight { get; set; }

        public int? Quantity { get; set; }

        [DataType(DataType.MultilineText)]
        [UIHint("RichTextEditor"), AllowHtml]
        public string Description { get; set; }

        [MaxLength(2000), Index("SearchIndex", 3)]
        public string Keywords { get; set; }

        public int? TaxClassId { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsVisible { get; set; }

        [IgnoreMap]
        public virtual ICollection<Category> Categories { get; set; }

        [IgnoreMap]
        public virtual ICollection<Option> Options { get; set; }

        [IgnoreMap]
        public virtual ICollection<Upload> Uploads { get; set; }

        [IgnoreMap]
        public virtual ICollection<ProductSection> Sections { get; set; }

        [IgnoreMap]
        public virtual ICollection<ProductSku> Skus { get; set; }
        
        [ForeignKey("TaxClassId")]
        public virtual TaxClass TaxClass { get; set; }
    }

    public sealed class ProductCsvMap : CsvClassMap<Product>
    {
        public ProductCsvMap()
        {
            Map(m => m.Id);
            Map(m => m.Sku);
            Map(m => m.Name);
            Map(m => m.Price);
            Map(m => m.CostPrice);
            Map(m => m.RetailPrice);
            Map(m => m.SalePrice);
            Map(m => m.Weight);
            Map(m => m.Quantity);
            Map(m => m.Description);
            Map(m => m.Keywords);
            Map(m => m.IsFeatured);
            Map(m => m.IsVisible);
        }
    }
}