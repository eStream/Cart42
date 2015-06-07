using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class BlogPostController : BaseController
    {
        private readonly IBlogService blogService;
        private readonly IBlogPostService blogPostService;
        private readonly IDeleterService deleterService;
        public BlogPostController(IBlogPostService blogPostService, IBlogService blogService, IDeleterService deleterService)
        {
            this.deleterService = deleterService;
            this.blogService = blogService;
            this.blogPostService = blogPostService;
        }

        // GET: Admin/BlogPost
        [AccessAuthorize(OperatorRoles.BLOGS)]
        public ActionResult Index()
        {
            var blogPost = blogPostService.FindAll().ToList();
            var model = new BlogPostsViewModel
            {
                BlogPosts = Mapper.Map<List<BlogPostViewModel>>(blogPost)
            };

            return View(model);
        }
        [AccessAuthorize(OperatorRoles.BLOGS + OperatorRoles.WRITE)]
        public ActionResult Create()
        {
            var model = new BlogPostViewModel();
            ViewBag.BlogId = new SelectList(blogService.FindAll(), "Id", "Title");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.BLOGS + OperatorRoles.WRITE)]
        public ActionResult Create(BlogPostViewModel model)
        {
            if (ModelState.IsValid)
            {
                blogPostService.AddOrUpdate(model);

                var action = RedirectToAction("Index");
                return action.WithSuccess(string.Format("The blog post \"{0}\" has been added".TA(), model.Title));
            }
            ViewBag.BlogId = new SelectList(blogService.FindAll(), "Id", "Title");
            return View(model);
        }

        [AccessAuthorize(OperatorRoles.BLOGS + OperatorRoles.WRITE)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BlogPost blogPost = blogPostService.Find(id.Value);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            var model = Mapper.Map<BlogPostViewModel>(blogPost);
            ViewBag.BlogId = new SelectList(blogService.FindAll(), "Id", "Title");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.BLOGS + OperatorRoles.WRITE)]
        public ActionResult Edit(BlogPostViewModel model)
        {
            if (ModelState.IsValid)
            {
                blogPostService.AddOrUpdate(model);
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("The blog post \"{0}\" has been updated".TA(), model.Title));
            }
            ViewBag.BlogId = new SelectList(blogService.FindAll(), "Id", "Title");
            return View(model);
        }

        [AccessAuthorize(OperatorRoles.BLOGS + OperatorRoles.DELETE)]
        public ActionResult Delete(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new BlogPostsViewModel();
            model.BlogPosts = new List<BlogPostViewModel>();

            foreach (int id in ids)
            {
                BlogPost blogPost = blogPostService.Find(id);
                if (blogPost == null) continue;

                model.BlogPosts.Add(new BlogPostViewModel
                {
                    Id = blogPost.Id,
                    Title = blogPost.Title,
                });
            }
            return View(model);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.BLOGS + OperatorRoles.DELETE)]
        public ActionResult DeleteConfirmed(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            foreach (int id in ids)
            {
                deleterService.DeleteBlogPost(id);
            }

            return RedirectToAction("Index")
                .WithWarning(string.Format("The blog post has been deleted".TA()));
        }
    }
}