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
    public class BlogViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Approve Comments")]
        public bool ApproveComments { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Blog, BlogViewModel>();
            Mapper.CreateMap<BlogViewModel, Blog>();
        }

    }
    public class BlogsViewModel
    {
        public List<BlogViewModel> Blogs { get; set; }
    }

    public class BlogViewModelValidator : AbstractValidator<BlogViewModel>
    {
       public BlogViewModelValidator()
       {
           RuleFor(c => c.Title).NotEmpty();
       }
    }
}