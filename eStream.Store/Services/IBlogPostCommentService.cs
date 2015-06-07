using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Models;

namespace Estream.Cart42.Web.Services
{
    public interface IBlogPostCommentService
    {
        IQueryable<BlogPostComment> FindAll();
        BlogPostComment Find(int id);
        void SetApproved(int id, bool approve);
        BlogPostComment AddOrUpdate(BlogPostCommentAddViewModel model);
    }
}
