using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Models;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Controllers
{
    public class OptionController : BaseController
    {
        private readonly ICacheService cacheService;

        public OptionController(DataContext db, ICacheService cacheService) : base(db)
        {
            this.cacheService = cacheService;
        }

        [ChildActionOnly]
        public PartialViewResult FiltersMenu(int categoryId, int[] optionIds, string viewName)
        {
            OptionsFilterViewModel model;
            if (optionIds == null || optionIds.Length <= 1)
            {
                /* Cache if up to 1 filter is used; Do not cache for more than 1 filter as there 
                 * are way too many combinations */
                var cacheKey = string.Format("FiltersMenu({0},{1})", categoryId,
                    optionIds != null ? string.Join("_", optionIds) : "null");
                model = cacheService.Get(cacheKey, () => generateFilterModel(categoryId, optionIds),
                    invalidateKeys: "options,products", slidingExpiration: TimeSpan.FromMinutes(10));
            }
            else
            {
                model = generateFilterModel(categoryId, optionIds);
            }

            return PartialView(viewName ?? "_FiltersMenu", model);
        }

        private OptionsFilterViewModel generateFilterModel(int categoryId, int[] optionIds)
        {
            var model = new OptionsFilterViewModel();

            var products = db.Products.Where(p => p.Categories.Any(c => c.Id == categoryId));
            if (optionIds != null && optionIds.Any())
            {
                products = products.Where(p => optionIds.All(oId => p.Options.Any(o => o.Id == oId)));
            }

            var allOptions = db.Options
                .Where(o => o.Category.IncludeInFilters && products.Any(p => p.Options.Any(o2 => o2.Id == o.Id)))
                //.Include(o => o.Category)
                .Select(o => new
                             {
                                 Category = o.Category,
                                 Option = o,
                                 Count = products.Count(p => p.Options.Any(o2 => o2.Id == o.Id))
                             })
                .ToList();

            var allCategories = allOptions.Select(o => o.Category).Distinct();

            foreach (var optionCategory in allCategories)
            {
                var optionCategoryModel = new OptionFilterCategoryViewModel
                                          {
                                              Id = optionCategory.Id,
                                              Name = optionCategory.Name,
                                              Type = optionCategory.Type
                                          };

                if (optionIds != null && optionIds.Any()
                    && allOptions.Any(o => o.Option.OptionCategoryId == optionCategory.Id
                                           && optionIds.Contains(o.Option.Id)))
                {
                    var selectedOption = allOptions.First(o => o.Option.OptionCategoryId == optionCategory.Id
                                                               && optionIds.Contains(o.Option.Id));
                    optionCategoryModel.Options.Add(new OptionFilterViewModel
                                                    {
                                                        Id = selectedOption.Option.Id,
                                                        Name = selectedOption.Option.Name,
                                                        Description = selectedOption.Option.Description,
                                                        Count = selectedOption.Count,
                                                        ActiveFilter = true
                                                    });
                }
                else
                {
                    var categoryOptions = allOptions.Where(o => o.Option.OptionCategoryId == optionCategory.Id
                                                                && o.Count > 0).OrderByDescending(o => o.Count).Take(50);

                    foreach (var categoryOption in categoryOptions)
                    {
                        optionCategoryModel.Options.Add(new OptionFilterViewModel
                                                        {
                                                            Id = categoryOption.Option.Id,
                                                            Name = categoryOption.Option.Name,
                                                            Description = categoryOption.Option.Description,
                                                            Count = categoryOption.Count
                                                        });
                    }
                }

                if (optionCategoryModel.Options.Any(o => o.ActiveFilter)
                    || optionCategoryModel.Options.Count() > 1)
                    model.Categories.Add(optionCategoryModel);
            }

            // Reorder with selected on top
            model.Categories = model.Categories.OrderByDescending(c => c.Options.Any(o => o.ActiveFilter)).ToList();
            return model;
        }
    }
}