using System.Collections.Generic;
using System.Linq;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface IOperatorService
    {
        IQueryable<User> FindAll();
        User Find(string id);
        User AddOrUpdate(OperatorViewModel model);
        List<string> GetRoles(string userId);
        void Delete(string id);
    }
}