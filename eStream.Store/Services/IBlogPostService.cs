using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface IBlogPostService
    {
        IQueryable<BlogPost> FindAll();
        BlogPost Find(int id);
        BlogPost AddOrUpdate(BlogPostViewModel model);
        void SetAllowComments(int id, bool allow);
    }
}