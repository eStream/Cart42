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
    public class BlogPostCommentViewModel : IHaveCustomMappings
    {
        public BlogPostCommentViewModel()
        {
            DatePosted = DateTime.Now;
            IsApproved = false;
        }
        public int Id { get; set; }

        public string UserId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Message")]
        public string Message { get; set; }

        public bool IsApproved { get; set; }

        [Display(Name = "Date Posted")]
        public DateTime DatePosted { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<BlogPostComment, BlogPostCommentViewModel>();
            Mapper.CreateMap<BlogPostCommentViewModel, BlogPostComment>();
        }
    }
    public class BlogPostCommentViewModelValidator : AbstractValidator<BlogPostCommentViewModel>
    {
        public BlogPostCommentViewModelValidator()
        {
            RuleFor(c => c.Name).NotEmpty().Length(1, 500);
            RuleFor(c => c.Message).NotEmpty();
            RuleFor(c => c.Email).EmailAddress().NotEmpty().When(c => c.UserId == null);
        }
    }
    public class BlogPostCommentsViewModel
    {
        public List<BlogPostCommentViewModel> BlogPostComments { get; set; }
    }
}