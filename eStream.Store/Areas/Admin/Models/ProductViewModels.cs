using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Models;
using Estream.Cart42.Web.Services;
using FluentValidation;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class ProductEditViewModel : BaseEditViewModel, IHaveCustomMappings
    {
        public ProductEditViewModel()
        {
            Sections = new List<ProductSectionEditViewModel>();
            Skus = new ProductSkusEditViewModel();
            IsVisible = true;
        }

        public int Id { get; set; }

        [MaxLength(100)]
        [Display(Name = "SKU")]
        public string Sku { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Cost Price")]
        public decimal? CostPrice { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Retail Price")]
        public decimal? RetailPrice { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Sale Price")]
        public decimal? SalePrice { get; set; }

        [Display(Name = "Weight")]
        public decimal Weight { get; set; }

        [Display(Name = "Quantity")]
        public int? Quantity { get; set; }

        [DataType(DataType.MultilineText)]
        [UIHint("RichTextEditor"), AllowHtml]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Keywords")]
        public string Keywords { get; set; }

        [Display(Name = "Tax Class")]
        public int? TaxClassId { get; set; }

        [Display(Name = "Featured")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Visible")]
        public bool IsVisible { get; set; }

        [Display(Name = "Additional sections")]
        [UIHint("ProductSections")]
        [IgnoreMap]
        public List<ProductSectionEditViewModel> Sections { get; set; }

        [Display(Name = "Skus")]
        [UIHint("Skus")]
        [IgnoreMap]
        public ProductSkusEditViewModel Skus { get; set; }

        [UIHint("CategoriesSelector")]
        [Display(Name = "Categories")]
        [IgnoreMap]
        public string CategoryIds { get; set; }

        [UIHint("ImageUploader")]
        [Display(Name = "Images")]
        [IgnoreMap]
        public string UploadIds { get; set; }

        [UIHint("OptionsSelector")]
        [Display(Name = "Options")]
        [IgnoreMap]
        public string OptionIds { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Product, ProductEditViewModel>();
            Mapper.CreateMap<ProductEditViewModel, Product>();
        }
    }

    public class ProductSkusEditViewModel
    {
        public ProductSkusEditViewModel()
        {
            Skus = new List<ProductSkuEditViewModel>();
        }

        public List<ProductSkuEditViewModel> Skus { get; set; }
    }

    public class ProductSkuEditViewModel : IHaveCustomMappings
    {
        public ProductSkuEditViewModel()
        {
            OptionIds = "[]";
            UploadIds = "[]";
            Options = "[]";
        }

        public int Id { get; set; }

        public string Sku { get; set; }

        public string UPC { get; set; }

        public int? Quantity { get; set; }

        public decimal? Price { get; set; }

        public decimal? Weight { get; set; }

        // Json of ids
        public string OptionIds { get; set; }

        // Json of ids
        public string UploadIds { get; set; }

        // Json
        public string Options { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<ProductSku, ProductSkuEditViewModel>()
                .ForMember(m => m.OptionIds, opt => opt.MapFrom(
                    s => JsonConvert.SerializeObject(s.Options.Select(o => o.Id).ToArray())))
                .ForMember(m => m.UploadIds, opt => opt.MapFrom(
                    s => JsonConvert.SerializeObject(s.Uploads.Select(u => u.Id).ToArray())))
                .ForMember(m => m.Options, opt => opt.MapFrom(
                    s => JsonConvert.SerializeObject(s.Options.Select(o =>
                        new
                        {
                            category = o.Category.Name,
                            option = o.Name
                        }).ToArray())));
            Mapper.CreateMap<ProductSkuEditViewModel, ProductSku>();
        }
    }

    public class ProductSkuGeneratorViewModel
    {
        public ProductSkuGeneratorViewModel()
        {
            Prefix = "SKU";
            Skus = new List<ProductSkuEditViewModel>();
            OptionIds = new List<int>();
        }

        public string Prefix { get; set; }
        public List<ProductSkuEditViewModel> Skus { get; set; }
        public List<int> OptionIds { get; set; }
    }

    public class ProductsIndexViewModel : IPageableViewModel, ISortableViewModel
    {
        public ProductsIndexViewModel()
        {
            Page = 1;
            OrderAsc = true;
            Keywords = string.Empty;
        }

        public IList<ProductSearchViewModel> Products { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public int TotalItems { get; set; }

        public string OrderColumn { get; set; }

        public bool OrderAsc { get; set; }

        public string Keywords { get; set; }
    }

    public class ProductSearchViewModel : IMapFrom<Product>
    {
        public int Id { get; set; }

        public string Sku { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public decimal? CostPrice { get; set; }

        public decimal? RetailPrice { get; set; }

        public decimal? SalePrice { get; set; }

        public decimal Weight { get; set; }

        public int? Quantity { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsVisible { get; set; }

        public string Description { get; set; }
    }

    public class ProductEditViewModelValidator : AbstractValidator<ProductEditViewModel>
    {
        public ProductEditViewModelValidator()
        {
            RuleFor(r => r.CategoryIds).NotEmpty().WithMessage("Please select at least one category".TA());
            RuleFor(r => r.Name).NotEmpty();
        }
    }
}