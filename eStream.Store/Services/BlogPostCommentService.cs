using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Models;

namespace Estream.Cart42.Web.Services
{
    public class BlogPostCommentService : IBlogPostCommentService
    {
        private readonly DataContext db;            
        public BlogPostCommentService(DataContext db)
        {
            this.db = db;
        }

        public IQueryable<BlogPostComment> FindAll()
        {
            return db.BlogPostComments;
        }

        public BlogPostComment Find(int id)
        {
            return db.BlogPostComments.Find(id);
        }

        public void SetApproved(int id, bool approve)
        {
            var comment = db.BlogPostComments.Find(id);
            comment.IsApproved = approve;
            db.SaveChanges();
        }

        public BlogPostComment AddOrUpdate(BlogPostCommentAddViewModel model)
        {
            BlogPostComment comment;
            if (model.Id == 0)
            {
                comment = Mapper.Map<BlogPostComment>(model);
                db.BlogPostComments.Add(comment);
            }
            else
            {
                comment = Find(model.Id);
                Mapper.Map(model, comment);

            }

            db.SaveChanges();

            return comment;
        }
    }
}