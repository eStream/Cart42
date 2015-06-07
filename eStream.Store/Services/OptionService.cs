using System.Linq;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public class OptionService : IOptionService
    {
        private readonly DataContext db;
        private readonly ICacheService cacheService;

        public OptionService(DataContext db, ICacheService cacheService)
        {
            this.db = db;
            this.cacheService = cacheService;
        }

        public IQueryable<Option> FindAll()
        {
            return db.Options;
        }

        public IQueryable<Option> FindByCategory(int categoryId)
        {
            return FindAll().Where(o => o.OptionCategoryId == categoryId);
        }

        public Option Find(int id)
        {
            return db.Options.Find(id);
        }

        public Option AddOrUpdate(OptionEditViewModel model)
        {
            Option option;
            if (model.Id == 0)
            {
                option = Mapper.Map<Option>(model);
                db.Options.Add(option);
            }
            else
            {
                option = Find(model.Id);
                Mapper.Map(model, option);
            }

            db.SaveChanges();

            cacheService.Invalidate("options");
            
            return option;
        }

        public void Delete(int id)
        {
            var option = Find(id);
            db.Options.Remove(option);
            db.SaveChanges();

            cacheService.Invalidate("options");
        }
    }
}