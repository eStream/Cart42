using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;
using FluentValidation;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class ShippingZoneEditViewModel : IHaveCustomMappings
    {
        public ShippingZoneEditViewModel()
        {
            CountryCodesJson = "[]";
            RegionIdsJson = "[]";
        }
        public int Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Countries")]
        public string CountryCodesJson { get; set; }

        [Display(Name = "Regions")]
        public string RegionIdsJson { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<ShippingZone, ShippingZoneEditViewModel>();
            Mapper.CreateMap<ShippingZoneEditViewModel, ShippingZone>();
        }
    }

    public class ShippingZoneViewModelValidator : AbstractValidator<ShippingZoneEditViewModel>
    {
        public ShippingZoneViewModelValidator(IShippingZoneService shippingZoneService)
        {
            RuleFor(z => z.Name).NotEmpty();
            RuleFor(z => z).Must(
                z => !shippingZoneService.FindAll().Any(d => d.Id != z.Id && d.Name == z.Name))
                .WithName("Name")
                .WithMessage("Name is already used".TA());
        }
    }

    public class ShippingZoneIndexViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<ShippingZone, ShippingZoneIndexViewModel>();
        }
    }

    public class ShippingZonesDeleteViewModel
    {
        public List<ShippingZoneDeleteViewModel> ShippingZones { get; set; }
    }

    public class ShippingZoneDeleteViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}