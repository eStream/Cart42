using System.Linq;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;

namespace Estream.Cart42.Web.Services
{
    public class OptionCategoryService : IOptionCategoryService
    {
        private readonly DataContext db;
        private readonly IOptionService optionService;

        public OptionCategoryService(DataContext db, IOptionService optionService)
        {
            this.db = db;
            this.optionService = optionService;
        }

        public IQueryable<OptionCategory> FindAll()
        {
            return db.OptionCategories;
        }

        public OptionCategory Find(int id)
        {
            return db.OptionCategories.Find(id);
        }

        public OptionCategory AddOrUpdate(OptionCategoryEditViewModel model)
        {
            OptionCategory optionCategory;
            if (model.Id == 0)
            {
                optionCategory = Mapper.Map<OptionCategory>(model);
                db.OptionCategories.Add(optionCategory);
            }
            else
            {
                optionCategory = Find(model.Id);
                Mapper.Map(model, optionCategory);

                if (model.Options != null)
                {
                    var options = optionService.FindByCategory(optionCategory.Id).ToList();
                    foreach (var option in options)
                    {
                        if (model.Options.None(o => o.Id == option.Id))
                        {
                            optionService.Delete(option.Id);
                        }
                    }
                }
            }

            db.SaveChanges();

            if (model.Options != null)
            {
                foreach (var optionModel in model.Options)
                {
                    optionModel.OptionCategoryId = optionCategory.Id;
                    optionService.AddOrUpdate(optionModel);
                }
            }

            return optionCategory;
        }

        public void Delete(int id)
        {
            var options = optionService.FindByCategory(id).ToList();
            foreach (var option in options)
            {
                optionService.Delete(option.Id);
            }
            var optionCategory = Find(id);
            db.OptionCategories.Remove(optionCategory);
            db.SaveChanges();
        }
    }
}