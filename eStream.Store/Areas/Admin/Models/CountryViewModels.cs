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
    public class CountryViewModel : IHaveCustomMappings
    {
        [Key, MaxLength(2)]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [MaxLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Country, CountryViewModel>();
            Mapper.CreateMap<CountryViewModel, Country>();
        }
    }
    public class CountryViewModelValidator : AbstractValidator<CountryViewModel>
    {
        public CountryViewModelValidator(ICountryService countryService)
        {
            RuleFor(r => r.Code).NotEmpty().Length(2);
            RuleFor(r => r.Name).NotEmpty();
            RuleFor(r => r).Must(
                r => !countryService.FindAll().Any(d => d.Code != r.Code && d.Name == r.Name))
                .WithName("Name")
                .WithMessage("Name is already used".TA());
        }
    }
}