using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Models;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class OptionCategoryController : BaseController
    {
        private readonly IOptionCategoryService optionCategoryService;
        private readonly IOptionService optionService;
        private readonly ISettingService settingService;

        public OptionCategoryController(IOptionCategoryService optionCategoryService, IOptionService optionService,
            ISettingService settingService)
        {
            this.optionCategoryService = optionCategoryService;
            this.optionService = optionService;
            this.settingService = settingService;
        }

        // GET: Admin/OptionCategory
        [AccessAuthorize(OperatorRoles.INVENTORY)]
        public ActionResult Index()
        {
            var optionCat = optionCategoryService.FindAll().ToList();
            var model = new OptionCategoriesIndexViewModel
            {
                OptionsCategories = Mapper.Map<List<OptionCategoryIndexViewModel>>(optionCat)
            };

            return View(model);
        }

        [AccessAuthorize(OperatorRoles.INVENTORY)]
        public JsonResult ListJson()
        {
            IOrderedEnumerable<OptionCategory> categories = optionCategoryService.FindAll().ToList().OrderBy(c => c.Name);
            return Json(categories.Select(c =>
                new
                {
                    c.Id,
                    c.Name,
                }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListOptionsJson(int? categoryId)
        {
            IQueryable<Option> options = categoryId.HasValue
                ? optionService.FindByCategory(categoryId.Value)
                : optionService.FindAll();

            return Json(options.Select(c =>
                new
                {
                    CategoryId = c.OptionCategoryId,
                    c.Id,
                    c.Name,
                }), JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/OptionCategory/Create
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.WRITE)]
        public ActionResult Create()
        {
            var model = new OptionCategoryEditViewModel();
            
            // Add two empty options to get started
            model.Options.Add(new OptionEditViewModel());
            model.Options.Add(new OptionEditViewModel());

            return View(model);
        }

        // POST: Admin/OptionCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.WRITE)]
        public ActionResult Create([Bind(Exclude = "Id")] OptionCategoryEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                optionCategoryService.AddOrUpdate(model);
                
                var action = RedirectToAction("Index");

                if (settingService.Get<bool>(SettingField.ShowOptionTutorial))
                {
                    settingService.Set(SettingField.ShowOptionTutorial, false);
                    action = RedirectToAction("Welcome", "Home");
                }

                return action.WithSuccess(string.Format("Option category \"{0}\" has been added".TA(), model.Name));
            }

            return View(model);
        }

        // GET: Admin/OptionCategory/Edit/5
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.WRITE)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OptionCategory optionCategory = optionCategoryService.Find(id.Value);
            if (optionCategory == null)
            {
                return HttpNotFound();
            }
            var model = Mapper.Map<OptionCategoryEditViewModel>(optionCategory);
            model.Options = Mapper.Map<List<OptionEditViewModel>>(optionCategory.Options);

            return View(model);
        }

        // POST: Admin/OptionCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.WRITE)]
        public ActionResult Edit(OptionCategoryEditViewModel model)
        {
            if (ModelState.IsValid) 
            {
                var optionCategory = optionCategoryService.AddOrUpdate(model);
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("Option category \"{0}\" has been updated".TA(), optionCategory.Name));
            }
            return View(model);
        }

        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.DELETE)]
        public ActionResult Delete(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new OptionCategoriesDeleteViewModel();
            model.OptionsCategories = new List<OptionCategoryDeleteViewModel>();

            foreach (int id in ids)
            {
                OptionCategory optionCategory = optionCategoryService.Find(id);
                if (optionCategory == null) continue;

                model.OptionsCategories.Add(new OptionCategoryDeleteViewModel
                {
                    Id = optionCategory.Id,
                    Name = optionCategory.Name,
                });
            }
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.DELETE)]
        public ActionResult DeleteConfirmed(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            foreach (int id in ids)
            {
                optionCategoryService.Delete(id);
            }

            return RedirectToAction("Index")
                .WithWarning(string.Format("The option categories have been deleted".TA()));
        }
    }
}