using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using FluentValidation;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class OptionEditViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        [Display(Name = "Option category")]
        public int OptionCategoryId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Option, OptionEditViewModel>();
            Mapper.CreateMap<OptionEditViewModel, Option>();
        }
    }
    public class OptionEditViewModelValidator : AbstractValidator<OptionEditViewModel>
    {
        public OptionEditViewModelValidator()
        {
            RuleFor(c => c.Name).NotEmpty()
                .WithMessage("Please specify option value".TA());
        }
    }

    public class OptionsIndexViewModel
    {
        public List<OptionIndexViewModel> Options { get; set; }
    }

    public class OptionIndexViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Option, OptionIndexViewModel>()
                .ForMember(o => o.CategoryName, opt => opt.MapFrom(s => s.Category.Name));
        }
    }

    public class OptionDeleteViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Option, OptionDeleteViewModel>()
                .ForMember(o => o.CategoryName, opt => opt.MapFrom(s => s.Category.Name));
        }
    }
}