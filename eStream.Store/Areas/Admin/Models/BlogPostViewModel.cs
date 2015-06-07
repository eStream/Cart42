using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using FluentValidation;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class BlogPostViewModel : IHaveCustomMappings
    {
        public BlogPostViewModel()
        {
            PublishDate = DateTime.Now;
        }
        public int Id { get; set; }

        public int BlogId { get; set; }

        public string UserId { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Blog")]
        public string BlogTitle { get; set; }

        [DataType(DataType.MultilineText)]
        [UIHint("RichTextEditor"), AllowHtml]
        [Display(Name = "Content")]
        public string Content { get; set; }

        [Display(Name = "Allow Comments")]
        public bool AllowComments { get; set; }

        [Display(Name = "Publish Date")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime PublishDate { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<BlogPost, BlogPostViewModel>();
            Mapper.CreateMap<BlogPostViewModel, BlogPost>();
        }

    }

    public class BlogPostsViewModel
    {
        public List<BlogPostViewModel> BlogPosts { get; set; }
    }

    public class BlogPostViewModelValidator : AbstractValidator<BlogPostViewModel>
    {
        public BlogPostViewModelValidator()
        {
            RuleFor(c => c.Title).NotEmpty().Length(1, 500);
            RuleFor(c => c.PublishDate).NotEmpty();
        }
    }
}