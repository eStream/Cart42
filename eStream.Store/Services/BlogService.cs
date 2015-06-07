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
    public class BlogService : IBlogService
    {
        private readonly DataContext db;
        private readonly ICacheService cacheService;

        public BlogService(DataContext db, ICacheService cacheService)
        {
            this.db = db;
            this.cacheService = cacheService;
        }

        public IQueryable<Blog> FindAll()
        {
            return db.Blogs;
        }

        public Blog Find(int id)
        {
            return db.Blogs.Find(id);
        }

        public Blog AddOrUpdate(BlogViewModel model)
        {
            Blog blog;
            if (model.Id == 0)
            {
                blog = Mapper.Map<Blog>(model);
                db.Blogs.Add(blog);
            }
            else
            {
                blog = Find(model.Id);
                Mapper.Map(model, blog);
            }

            db.SaveChanges();

            cacheService.Invalidate("blogs");
            
            return blog;
        }

        public void SetApproveComments(int id, bool aprove)
        {
            var blog = db.Blogs.Find(id);
            blog.ApproveComments = aprove;
            db.SaveChanges();

            cacheService.Invalidate("blogs");
        }
    }
}