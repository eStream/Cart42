using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using FluentValidation;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class OptionCategoryEditViewModel : IHaveCustomMappings
    {
        public OptionCategoryEditViewModel()
        {
            Options = new List<OptionEditViewModel>();
        }

        public int Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Type")]
        public OptionCategoryType Type { get; set; }

        [Display(Name = "Include in filters")]
        public bool IncludeInFilters { get; set; }

        [IgnoreMap]
        public List<OptionEditViewModel> Options { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<OptionCategory, OptionCategoryEditViewModel>();
            Mapper.CreateMap<OptionCategoryEditViewModel, OptionCategory>();
        }
    }
    public class OptionCategoryEditViewModelValidator : AbstractValidator<OptionCategoryEditViewModel>
    {
        public OptionCategoryEditViewModelValidator()
        {
            RuleFor(c => c.Name).NotEmpty();
            RuleFor(c => c.Type).NotEmpty();
        }
    }

    public class OptionCategoriesIndexViewModel
    {
        public List<OptionCategoryIndexViewModel> OptionsCategories { get; set; }
    }

    public class OptionCategoriesDeleteViewModel
    {
        public List<OptionCategoryDeleteViewModel> OptionsCategories { get; set; }
    }

    public class OptionCategoryIndexViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Type")]
        public OptionCategoryType Type { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<OptionCategory, OptionCategoryIndexViewModel>();
        }
    }

    public class OptionCategoryDeleteViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Type")]
        public OptionCategoryType Type { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<OptionCategory, OptionCategoryDeleteViewModel>();
        }
    }
}