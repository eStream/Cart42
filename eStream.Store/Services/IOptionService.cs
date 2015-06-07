using System.Linq;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface IOptionService
    {
        Option Find(int id);
        IQueryable<Option> FindAll();
        IQueryable<Option> FindByCategory(int categoryId);

        Option AddOrUpdate(OptionEditViewModel model);
        
        void Delete(int id);
    }
}