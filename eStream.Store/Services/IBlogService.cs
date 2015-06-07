using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface IBlogService
    {
        IQueryable<Blog> FindAll();
        Blog Find(int id);
        Blog AddOrUpdate(BlogViewModel model);
        void SetApproveComments(int id, bool aprove);
    }
}