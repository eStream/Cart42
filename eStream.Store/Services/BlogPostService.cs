using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public class BlogPostService : IBlogPostService
    {
        private readonly DataContext db;
        private readonly ICacheService cacheService;

        public BlogPostService(DataContext db, ICacheService cacheService)
        {
            this.db = db;
            this.cacheService = cacheService;
        }

        public IQueryable<BlogPost> FindAll()
        {
            return db.BlogPosts;
        }

        public BlogPost Find(int id)
        {
            return db.BlogPosts.Find(id);
        }

        public BlogPost AddOrUpdate(BlogPostViewModel model)
        {
            BlogPost blogPost;
            if (model.Id == 0)
            {
                blogPost = Mapper.Map<BlogPost>(model);
                db.BlogPosts.Add(blogPost);
            }
            else
            {
                blogPost = Find(model.Id);
                Mapper.Map(model, blogPost);
            }

            db.SaveChanges();

            cacheService.Invalidate("blogs");

            return blogPost;
        }

        public void SetAllowComments(int id, bool allow)
        {
            var blogPost = db.BlogPosts.Find(id);
            blogPost.AllowComments = allow;
            db.SaveChanges();

            cacheService.Invalidate("blogs");
        }
    }
}