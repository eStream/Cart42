using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DependencyResolution;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Models;
using Estream.Cart42.Web.Services;
using BlogPostViewModel = Estream.Cart42.Web.Models.BlogPostViewModel;

namespace Estream.Cart42.Web.Controllers
{
    public class BlogController : BaseController
    {
        private readonly IBlogService blogService;
        private readonly IBlogPostService blogPostService;
        private readonly IBlogPostCommentService blogPostCommentService;
        private readonly ICacheService cacheService;
        private readonly ICurrentUser currentUser;

        public BlogController(IBlogService blogService, IBlogPostService blogPostService, 
            IBlogPostCommentService blogPostCommentService, ICacheService cacheService,
            ICurrentUser currentUser)
        {
            this.blogService = blogService;
            this.blogPostService = blogPostService;
            this.blogPostCommentService = blogPostCommentService;
            this.cacheService = cacheService;
            this.currentUser = currentUser;
        }

        [ChildActionOnly]
        
        public ActionResult Links(int? blogId, int count = 4, string viewName = "_Links")
        {
            var cacheKey = string.Format("bloglinks_{0}_{1}", blogId, count);
            var cache = cacheService.Get(cacheKey,
                () =>
                {
                    var blog = blogId.HasValue
                        ? blogService.Find(blogId.Value)
                        : blogService.FindAll().OrderBy(b => b.Id).FirstOrDefault();

                    if (blog == null) return null;

                    var today = DateTime.Now.Date;

                    var blogPosts = blog.BlogPosts
                        .Where(p => p.PublishDate >= today)
                        .OrderByDescending(p => p.PublishDate)
                        .Take(count);

                    var model = new BlogLinksViewModels {BlogName = blog.Title};
                    foreach (var post in blogPosts)
                    {
                        model.BlogPosts.Add(post.Id, post.Title);
                    }

                    return model;
                },
                invalidateKeys: "blogs", absoluteExpiration: DateTime.Now.AddMinutes(60));

            if (cache == null) return new EmptyResult();

            return PartialView(viewName, cache);
        }

        public ActionResult Index(int id)
        {
            Blog blog = blogService.Find(id);

            if (blog == null)
            {
                return HttpNotFound();
            }

            var model = Mapper.Map<BlogIndexViewModel>(blog);
            var blogPosts = blog.BlogPosts.Where(b => b.PublishDate <= DateTime.Now).OrderByDescending(b => b.PublishDate).ToList();
            foreach (var post in blogPosts)
            {
                model.BlogPosts.Add(Mapper.Map<BlogPostViewModel>(post));
            }

            return View(model);
        }

        public ActionResult Post(int id)
        {
            BlogPost blogPost = blogPostService.Find(id);

            if (blogPost == null)
            {
                return HttpNotFound();
            }

            var model = Mapper.Map<BlogPostViewModel>(blogPost);
            if (blogPost.AllowComments)
            {
                var comments = blogPost.Comments.Where(c => c.IsApproved).OrderByDescending(c => c.DatePosted).ToList();
                foreach (var comment in comments)
                {
                    model.Comments.Add(Mapper.Map<BlogPostCommentAddViewModel>(comment));
                }

            }
            return View(model);
        }

        [HttpGet]
        public ActionResult AddPostComment(int id)
        {
            var model = new BlogPostCommentAddViewModel();
            model.BlogPostId = id;
            model.IsAnonymous = !User.Identity.IsAuthenticated;

            return PartialView("_AddPostComment", model);
        }

        [HttpPost]
        public ActionResult AddPostComment(BlogPostCommentAddViewModel model)
        {
          //  var comment = Mapper.Map<BlogPostComment>(model);
            if (User.Identity.IsAuthenticated)
            {
                model.UserId = currentUser.User.Id;
                model.Name = currentUser.User.FirstName;
                model.Email = currentUser.User.Email;
            }

            blogPostCommentService.AddOrUpdate(model);
            var action = RedirectToAction("Post", new {id = model.BlogPostId});
            return action.WithSuccess(string.Format("The comment has been added".TA()));
        }
    }
}