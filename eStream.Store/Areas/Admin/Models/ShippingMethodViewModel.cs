using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Services;
using FluentValidation;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class ShippingMethodEditViewModel : IHaveCustomMappings
    {
        public ShippingMethodEditViewModel()
        {
            WeightRanges = new List<ShippingByRange>();
            TotalRanges = new List<ShippingByRange>();
        }

        public int Id { get; set; }

        [Display(Name = "Shipping Zone")]
        public int ShippingZoneId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Type")]
        public ShippingMethodType Type { get; set; }

        [Display(Name = "Minimum order total")]
        public decimal? FreeShippingMinTotal { get; set; }

        public bool FreeShippingExcludeFixedShipping { get; set; }

        [Display(Name = "Flat Rate")]
        public decimal? FlatRateAmount { get; set; }

        [Display(Name = "Rate is per Item")]
        public bool FlatRatePerItem { get; set; }

        [Display(Name = "Weight Ranges")]
        public List<ShippingByRange> WeightRanges { get; set; }

        [Display(Name = "Total Ranges")]
        public List<ShippingByRange> TotalRanges { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<ShippingMethod, ShippingMethodEditViewModel>()
                .ForMember(m => m.WeightRanges,
                    opt => opt.MapFrom(f => JsonConvert.DeserializeObject<List<ShippingByRange>>(f.WeightRanges)))
                .ForMember(m => m.TotalRanges,
                    opt => opt.MapFrom(f => JsonConvert.DeserializeObject<List<ShippingByRange>>(f.TotalRanges)));

            Mapper.CreateMap<ShippingMethodEditViewModel, ShippingMethod>()
                .ForMember(m => m.WeightRanges, opt => opt.MapFrom(f => JsonConvert.SerializeObject(f.WeightRanges)))
                .ForMember(m => m.TotalRanges, opt => opt.MapFrom(f => JsonConvert.SerializeObject(f.TotalRanges)));
        }
    }

    public class ShippingMethodViewModelValidator : AbstractValidator<ShippingMethodEditViewModel>
    {
        public ShippingMethodViewModelValidator()
        {
            RuleFor(c => c.Name).NotEmpty();
            RuleFor(c => c.Type).NotEmpty();
            RuleFor(c => c.ShippingZoneId).NotEmpty();
            RuleFor(c => c.FreeShippingMinTotal).GreaterThanOrEqualTo(0).When(c => c.Type == ShippingMethodType.Free);
            RuleFor(c => c.WeightRanges).NotNull().When(c => c.Type == ShippingMethodType.ByWeight);
            RuleFor(c => c.TotalRanges).NotNull().When(c => c.Type == ShippingMethodType.ByTotal);
            RuleFor(c => c.FlatRateAmount).NotEmpty().When(c => c.Type == ShippingMethodType.Flat);
            RuleFor(c => c.FlatRateAmount).GreaterThan(0).When(c => c.Type == ShippingMethodType.Flat);
        }
    }


    public class ShippingMethodIndexViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Shipping Zone")]
        public string ShippingZoneName { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<ShippingMethod, ShippingMethodIndexViewModel>();
        }
    }

    public class ShippingMethodsDeleteViewModel
    {
        public List<ShippingMethodDeleteViewModel> ShippingMethods { get; set; }
    }

    public class ShippingMethodDeleteViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}