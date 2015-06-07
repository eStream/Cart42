using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;
using FluentValidation;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class CategoryEditViewModel : BaseEditViewModel, IHaveCustomMappings
    {
        public CategoryEditViewModel()
        {
            IsVisible = true;
            SortOrder = 1;
        }

        public int Id { get; set; }

        [Display(Name = "Parent Category")]
        public int? ParentId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        [UIHint("RichTextEditor"), AllowHtml]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Visible in menu")]
        public bool IsVisible { get; set; }

        [Display(Name = "Sort order")]
        public int SortOrder { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Category, CategoryEditViewModel>();
            Mapper.CreateMap<CategoryEditViewModel, Category>();
        }
    }
     
    public class CategoryEditViewModelValidator : AbstractValidator<CategoryEditViewModel>
    {
        public CategoryEditViewModelValidator(ICategoryService categoryService)
        {
            RuleFor(c => c.Name).NotEmpty().Length(1, 500);
            RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
            RuleFor(c => c).Must(
                r => !categoryService.FindAll().Any(c => c.Id != r.Id && c.Name == r.Name && c.ParentId == r.ParentId))
                .WithName("Name")
                .WithMessage("Name is already used".TA());
            RuleFor(c => c.ParentId).NotEqual(c => c.Id).WithMessage("Select valid parent category".TA());
        }
    }

    public class CategoriesIndexViewModel
    {
        public List<CategoryIndexViewModel> Categories { get; set; }

        public string GetNameWithParent(int categoryId)
        {
            var category = Categories.Find(c => c.Id == categoryId);
            string name = category.Name;
            if (category.ParentId != null)
            {
                int? parentId = category.ParentId;
                do
                {
                    CategoryIndexViewModel parent = Categories.Find(c => c.Id == parentId);
                    name = parent.Name + " > " + name;
                    parentId = parent.ParentId;
                } while (parentId != null);
            }

            return name;
        }

        public int GetProductsCountWithChild(int categoryId)
        {
            var category = Categories.Find(c => c.Id == categoryId);
            var count = category.ProductsCount +
                        Categories.Where(c => c.ParentId == categoryId)
                            .Sum(childCategory => GetProductsCountWithChild(childCategory.Id));
            return count;
        }
    }

    public class CategoryIndexViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        [Display(Name = "Parent category")]
        public int? ParentId { get; set; }

        public string Name { get; set; }

        [Display(Name = "Visible in menu")]
        public bool IsVisible { get; set; }

        [Display(Name = "Sort order")]
        public int SortOrder { get; set; }

        [IgnoreMap]
        public int ProductsCount { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Category, CategoryIndexViewModel>();
        }
    }

    public class CategoriesDeleteViewModel
    {
        public List<CategoryDeleteViewModel> Categories { get; set; }
    }

    public class CategoryDeleteViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [IgnoreMap]
        public int ProductsCount { get; set; }
    }
}