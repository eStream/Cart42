using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;
using FluentValidation;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class TaxZoneEditViewModel : IHaveCustomMappings
    {
        public TaxZoneEditViewModel()
        {
            CountryCodesJson = "[]";
            RegionIdsJson = "[]";
        }

        public int Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        // Array of string json
        [Display(Name = "Countries")]
        public string CountryCodesJson { get; set; }

        // Array of string json
        [Display(Name = "Regions")]
        public string RegionIdsJson { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<TaxZone, TaxZoneEditViewModel>();
            Mapper.CreateMap<TaxZoneEditViewModel, TaxZone>();
        }
    }

    public class TaxZoneViewModelValidator : AbstractValidator<TaxZoneEditViewModel>
    {
        public TaxZoneViewModelValidator(ITaxZoneService taxZoneService)
        {
            RuleFor(z => z.Name).NotEmpty();
            RuleFor(z => z).Must(
                z => !taxZoneService.FindAll().Any(d => d.Id != z.Id && d.Name == z.Name))
                .WithName("Name")
                .WithMessage("Name is already used".TA());

            // TODO: Check for conflicts of countries or regions are defined in other tax zones
        }
    }

    public class TaxZoneIndexViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<TaxZone, TaxZoneIndexViewModel>();
        }
    }

    public class TaxZonesDeleteViewModel
    {
        public List<TaxZoneDeleteViewModel> TaxZones { get; set; }
    }

    public class TaxZoneDeleteViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}