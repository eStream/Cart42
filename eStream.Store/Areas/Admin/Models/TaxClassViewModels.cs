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
    public class TaxClassViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<TaxClass, TaxClassViewModel>();
            Mapper.CreateMap<TaxClassViewModel, TaxClass>();
        }
    }

    public class TaxClassViewModelValidator : AbstractValidator<TaxClassViewModel>
    {
        public TaxClassViewModelValidator(ITaxClassService taxClassService)
        {
            RuleFor(r => r.Name).NotEmpty();
            RuleFor(r => r).Must(
                r => !taxClassService.FindAll().Any(d => d.Id != r.Id && d.Name == r.Name))
                .WithName("Name")
                .WithMessage("Name is already used".TA());
        }
    }
}