using System.Linq;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface IOptionCategoryService
    {
        IQueryable<OptionCategory> FindAll();
        OptionCategory AddOrUpdate(OptionCategoryEditViewModel model);
        OptionCategory Find(int id);
        void Delete(int id);
    }
}