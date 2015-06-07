using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Models
{
    public class ProductGridViewModel
    {
        public ProductGridViewModel()
        {
            Page = 1;
        }

        public ProductViewModel[] Products { get; set; }

        public int Page { get; set; }

        public int TotalPages { get; set; }

        public int TotalProducts { get; set; }

        public int? CategoryId { get; set; }

        public string Keywords { get; set; }

        public bool? Featured { get; set; }
    }

    public class ProductViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        public string Sku { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public Guid? PrimaryPhotoId { get; set; }

        public string Url { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Product, ProductViewModel>()
                .ForMember(p => p.Price, opt => opt.MapFrom(p => p.SalePrice ?? p.Price));
        }
    }

    public class ProductDetailsViewModel : IHaveCustomMappings
    {
        public ProductDetailsViewModel()
        {
            OptionCategories = new List<ProductOptionCategoryViewModel>();
            Skus = new List<ProductSkuViewModel>();
            Sections = new List<ProductSectionViewModel>();
        }

        public int Id { get; set; }

        public string Sku { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public decimal? RetailPrice { get; set; }

        public decimal? SalePrice { get; set; }

        public decimal Weight { get; set; }

        public int? Quantity { get; set; }

        public string Description { get; set; }

        public Guid[] PhotoIds { get; set; }

        [IgnoreMap]
        public List<ProductOptionCategoryViewModel> OptionCategories { get; set; }

        public List<ProductSkuViewModel> Skus { get; set; }
            
        [IgnoreMap]
        public List<ProductSectionViewModel> Sections { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Product, ProductDetailsViewModel>()
                .ForMember(p => p.PhotoIds,
                    opt => opt.MapFrom(p => p.Uploads.OrderBy(u => u.SortOrder).Select(u => u.Id).ToArray()));
        }
    }

    public class ProductSkuViewModel : IHaveCustomMappings
    {
        public ProductSkuViewModel()
        {
            UploadIds = new List<string>();
        }

        public int Id { get; set; }

        public string Sku { get; set; }

        public string UPC { get; set; }

        public int? Quantity { get; set; }

        public decimal? Price { get; set; }

        public decimal? Weight { get; set; }

        public string OptionsJson { get; set; }

        public List<string> UploadIds { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<ProductSku, ProductSkuViewModel>()
                .ForMember(m => m.OptionsJson, opt => opt.MapFrom(
                    s => JsonConvert.SerializeObject(s.Options.Select(o =>
                        new
                        {
                            categoryId = o.OptionCategoryId,
                            optionId = o.Id
                        }).ToArray())))
                .ForMember(m => m.UploadIds, opt => opt.MapFrom(s => s.Uploads.Select(u => u.Id).ToArray()));
        }
    }

    public class ProductSectionViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        public string Title { get; set; }

        // Text or JSON depending on the type
        public string Settings { get; set; }

        public ProductSectionType Type { get; set; }

        public ProductSectionPosition Position { get; set; }

        public int Priority { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<ProductSection, ProductSectionViewModel>();
        }
    }

    public class ProductOptionCategoryViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public OptionCategoryType Type { get; set; }

        [IgnoreMap]
        public List<ProductOptionViewModel> Options { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<OptionCategory, ProductOptionCategoryViewModel>();
        }
    }

    public class ProductOptionViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool Selected { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Option, ProductOptionViewModel>();
        }
    }
}