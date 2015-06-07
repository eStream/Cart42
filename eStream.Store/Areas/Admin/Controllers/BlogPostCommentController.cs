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
using Estream.Cart42.Web.Models;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class BlogPostCommentController : BaseController
    {
        
        private readonly IBlogPostCommentService blogPostCommentService;
        private readonly IDeleterService deleterService;
        public BlogPostCommentController(IBlogPostCommentService blogPostCommentService, IDeleterService deleterService)
        {
            this.deleterService = deleterService;
            this.blogPostCommentService = blogPostCommentService;
        }
        // GET: Admin/BlogPostComment
        [AccessAuthorize(OperatorRoles.BLOGS)]
        public ActionResult Index()
        {
            var comment = blogPostCommentService.FindAll().Where(c => c.IsApproved).ToList();
            var model = new BlogPostCommentsViewModel
            {
                BlogPostComments = Mapper.Map<List<BlogPostCommentViewModel>>(comment)
            };
            return View(model);
        }

        [AccessAuthorize(OperatorRoles.BLOGS)]
        public ActionResult ForApproval()
        {
            var comment = blogPostCommentService.FindAll().Where(c => c.IsApproved == false).ToList();
            var model = new BlogPostCommentsViewModel
            {
                BlogPostComments = Mapper.Map<List<BlogPostCommentViewModel>>(comment)
            };
            return View(model);
        }

        [AccessAuthorize(OperatorRoles.BLOGS + OperatorRoles.WRITE)]
        public ActionResult Approve(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            foreach (int id in ids)
            {
                blogPostCommentService.SetApproved(id, true);
            }

            return RedirectToAction("ForApproval")
                .WithWarning(string.Format("The comment has been approved".TA()));
        }

        [AccessAuthorize(OperatorRoles.BLOGS + OperatorRoles.WRITE)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BlogPostComment comment = blogPostCommentService.Find(id.Value);
            if (comment == null)
            {
                return HttpNotFound();
            }
            var model = Mapper.Map<BlogPostCommentAddViewModel>(comment);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.BLOGS + OperatorRoles.WRITE)]
        public ActionResult Edit(BlogPostCommentAddViewModel model)
        {
            if (ModelState.IsValid)
            {
                blogPostCommentService.AddOrUpdate(model);
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("The comment \"{0}\" has been updated".TA(), model.Name));
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
            var model = new BlogPostCommentsViewModel();
            model.BlogPostComments = new List<BlogPostCommentViewModel>();

            foreach (int id in ids)
            {
                BlogPostComment comment = blogPostCommentService.Find(id);
                if (comment == null) continue;

                model.BlogPostComments.Add(new BlogPostCommentViewModel
                {
                    Id = comment.Id,
                    Name = comment.Name,
                });
            }
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.BLOGS + OperatorRoles.WRITE)]
        public ActionResult DeleteConfirmed(int[] ids,string fromAction)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            foreach (int id in ids)
            {
                deleterService.DeleteBlogPostComment(id);
            }

            return RedirectToAction(fromAction??"Index")
                .WithWarning(string.Format("The comment has been deleted".TA()));
        }
    }
}