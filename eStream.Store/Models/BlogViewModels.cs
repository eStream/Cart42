using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using FluentValidation;

namespace Estream.Cart42.Web.Models
{
    public class BlogLinksViewModels
    {
        public BlogLinksViewModels()
        {
            BlogPosts = new Dictionary<int, string>();
        }

        public string BlogName { get; set; }

        public Dictionary<int, string> BlogPosts { get; set; }
    }

    public class BlogIndexViewModel : IHaveCustomMappings
    {
        public BlogIndexViewModel()
        {
            BlogPosts = new List<BlogPostViewModel>();
        }

        public int Id { get; set; }

        public string Title { get; set; }

        [IgnoreMap]
        public List<BlogPostViewModel> BlogPosts { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Blog, BlogIndexViewModel>();
            Mapper.CreateMap<BlogIndexViewModel, Blog>();
        }
    }

    public class BlogPostViewModel : IHaveCustomMappings
    {
        public BlogPostViewModel()
        {
            Comments = new List<BlogPostCommentAddViewModel>();
        }
        public int Id { get; set; }

        public int BlogId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public bool AllowComments { get; set; }

        public DateTime PublishDate { get; set; }

        [IgnoreMap]
        public List<BlogPostCommentAddViewModel> Comments { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<BlogPost, BlogPostViewModel>();
            Mapper.CreateMap<BlogPostViewModel, BlogPost>();
        }
    }

    public class BlogPostCommentAddViewModel : IHaveCustomMappings
    {
        public BlogPostCommentAddViewModel()
        {
            DatePosted = DateTime.Now;
        }

        public int Id { get; set; }

        public int BlogPostId { get; set; }

        public string UserId { get; set; }

        public bool IsAnonymous { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        [DataType(DataType.MultilineText)]
        public string Message { get; set; }

        public DateTime DatePosted { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<BlogPostComment, BlogPostCommentAddViewModel>();
            Mapper.CreateMap<BlogPostCommentAddViewModel, BlogPostComment>();
        }
    }

    public class BlogPostCommentViewModelValidator : AbstractValidator<BlogPostCommentAddViewModel>
    {
        public BlogPostCommentViewModelValidator()
        {
            RuleFor(c => c.Message).NotEmpty();
            RuleFor(c => c.Name).NotEmpty();
            RuleFor(c => c.Email).NotEmpty();
            RuleFor(c => c.Email).EmailAddress();
        }
    }
}