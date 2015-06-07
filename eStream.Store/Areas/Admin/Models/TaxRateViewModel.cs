using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;
using FluentValidation;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class TaxRateEditViewModel : IHaveCustomMappings
    {
        public TaxRateEditViewModel()
        {
            ClassRates = new List<TaxClassRateEditViewModel>();
        }

        public int Id { get; set; }

        public int TaxZoneId { get; set; }
       
        [Display(Name = "Tax Zone")]
        public string TaxZoneName { get; set; }
        
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Default Rate")]
        public decimal Amount { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Order")]
        public int Order { get; set; }

        [IgnoreMap]
        public List<TaxClassRateEditViewModel> ClassRates { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<TaxRate, TaxRateEditViewModel>();
            Mapper.CreateMap<TaxRateEditViewModel, TaxRate>()
                .ForMember(m => m.ClassRates, opt => opt.Ignore());
        }
    }
    public class TaxRateEditViewModelValidator : AbstractValidator<TaxRateEditViewModel>
    {
        public TaxRateEditViewModelValidator(ITaxRateService taxRateService)
        {
            RuleFor(r => r.Amount).GreaterThanOrEqualTo(0);
            RuleFor(r => r.TaxZoneId).NotEmpty().WithMessage("Please select a tax zone".TA());
            RuleFor(r => r.Name).NotEmpty();
            RuleFor(r => r).Must(
                r => !taxRateService.FindAll().Any(d => d.Id != r.Id && d.Name == r.Name))
                .WithName("Name")
                .WithMessage("Name is already used".TA());
        }
    }

    public class TaxRateIndexViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        [Display(Name = "Tax Zone")]
        public string TaxZoneName { get; set; }

        public string Name { get; set; }

        [Display(Name = "Default Rate")]
        public decimal Amount { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<TaxRate, TaxRateIndexViewModel>();
        }
    }

    public class TaxRatesDeleteViewModel
    {
        public List<TaxRateDeleteViewModel> TaxRates { get; set; }
    }

    public class TaxRateDeleteViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }


    public class TaxClassRateEditViewModel
    {
        public int TaxClassId { get; set; }

        public string TaxClassName { get; set; }

        public decimal? Amount { get; set; }
    
    }
}