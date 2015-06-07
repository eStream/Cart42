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
    public class RegionViewModel: IHaveCustomMappings
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [MaxLength(2)]
        public string CountryCode { get; set; }

        [MaxLength(500)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Region, RegionViewModel>();
            Mapper.CreateMap<RegionViewModel, Region>();
        }
    }

    public class RegionsViewModel
    {
        public List<RegionViewModel> Regions { get; set; }
    }

    public class RegionViewModelValidator : AbstractValidator<RegionViewModel>
    {
        public RegionViewModelValidator(IRegionService regionService)
        {
            RuleFor(r => r.CountryCode).NotEmpty().Length(2);
            RuleFor(r => r.Name).NotEmpty();
            RuleFor(r => r).Must(
                r => !regionService.FindAll().Any(d => d.Id != r.Id && d.Name == r.Name && d.CountryCode == r.CountryCode))
                .WithName("Name")
                .WithMessage("Name is already used".TA());
        }
    }
}