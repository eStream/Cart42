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
    public class BlogController : BaseController
    {
        private readonly IBlogService blogService;
        private readonly IDeleterService deleterService;
        public BlogController(IBlogService blogService, IDeleterService deleterService)
        {
            this.deleterService = deleterService;
            this.blogService = blogService;
        }
        // GET: Admin/Blog
        [AccessAuthorize(OperatorRoles.BLOGS)]
        public ActionResult Index()
        {
            var blog = blogService.FindAll().ToList();
            var model = new BlogsViewModel
            {
                Blogs = Mapper.Map<List<BlogViewModel>>(blog)
            };
            return View(model);
        }
        [AccessAuthorize(OperatorRoles.BLOGS + OperatorRoles.WRITE)]
        public ActionResult Create()
        {
            var model = new BlogViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.BLOGS + OperatorRoles.WRITE)]
        public ActionResult Create(BlogViewModel model)
        {
            if (ModelState.IsValid)
            {
                blogService.AddOrUpdate(model);

                var action = RedirectToAction("Index");
                return action.WithSuccess(string.Format("The blog \"{0}\" has been added".TA(), model.Title));
            }
            return View(model);
        }

        [AccessAuthorize(OperatorRoles.BLOGS + OperatorRoles.WRITE)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Blog blog = blogService.Find(id.Value);
            if (blog == null)
            {
                return HttpNotFound();
            }
            var model = Mapper.Map<BlogViewModel>(blog);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.BLOGS + OperatorRoles.WRITE)]
        public ActionResult Edit(BlogViewModel model)
        {
            if (ModelState.IsValid)
            {
                blogService.AddOrUpdate(model);
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("The blog \"{0}\" has been updated".TA(), model.Title));
            }

            return View(model);
        }

        [AccessAuthorize(OperatorRoles.BLOGS + OperatorRoles.DELETE)]
        public ActionResult Delete(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new BlogsViewModel();
            model.Blogs = new List<BlogViewModel>();

            foreach (int id in ids)
            {
                Blog blog = blogService.Find(id);
                if (blog == null) continue;

                model.Blogs.Add(new BlogViewModel
                {
                    Id = blog.Id,
                    Title = blog.Title,
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
               deleterService.DeleteBlog(id);
            }

            return RedirectToAction("Index")
                .WithWarning(string.Format("The blog has been deleted".TA()));
        }
    }
}