using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class CategoryController : BaseController
    {
        private readonly ICategoryDeleter categoryDeleter;
        private readonly ISettingService settingService;
        private readonly ICategoryService categoryService;

        public CategoryController(ICategoryService categoryService, ICategoryDeleter categoryDeleter,
            ISettingService settingService)
        {
            this.categoryService = categoryService;
            this.categoryDeleter = categoryDeleter;
            this.settingService = settingService;
        }

        // GET: Admin/Category
        [AccessAuthorize(OperatorRoles.INVENTORY)]
        public ActionResult Index()
        {
            var model = new CategoriesIndexViewModel();
            IQueryable<Category> categories = categoryService.FindAll();
            model.Categories = Mapper.Map<List<CategoryIndexViewModel>>(categories);
            foreach (CategoryIndexViewModel category in model.Categories)
            {
                category.ProductsCount = categoryService.GetProductsCount(category.Id);
            }
            return View(model);
        }

        [AccessAuthorize(OperatorRoles.INVENTORY)]
        public JsonResult ListJson()
        {
            var model = new CategoriesIndexViewModel();
            IQueryable<Category> categories = categoryService.FindAll();
            model.Categories = Mapper.Map<List<CategoryIndexViewModel>>(categories);

            var jsonObject = from c in model.Categories
                let nameWithParent = model.GetNameWithParent(c.Id)
                orderby nameWithParent
                select new
                {
                    c.Id,
                    c.Name,
                    NameWithParent = nameWithParent
                };
            return Json(jsonObject, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/Category/Create
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.WRITE)]
        public ActionResult Create()
        {
            var model = new CategoryEditViewModel();

            loadParentId();
            return View(model);
        }

        // POST: Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.WRITE)]
        public ActionResult Create([Bind(Exclude = "Id")] CategoryEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Category category = categoryService.AddOrUpdate(model);

                ActionResult action = RedirectToAction("Index");

                if (settingService.Get<bool>(SettingField.ShowCategoryTutorial))
                {
                    settingService.Set(SettingField.ShowCategoryTutorial, false);
                    action = RedirectToAction("Welcome", "Home");
                }

                if (model.OnCompleteAction == OnCompleteActionType.AddNew)
                {
                    loadParentId();
                    action = View(new CategoryEditViewModel {OnCompleteAction = OnCompleteActionType.AddNew});
                }
                if (model.OnCompleteAction == OnCompleteActionType.CloneNew)
                {
                    loadParentId();
                    action = View(model);
                }

                return action
                    .WithSuccess(string.Format("Category \"{0}\" has been added".TA(), category.Name));
            }

            loadParentId();
            return View(model);
        }

        private void loadParentId()
        {
            ViewBag.ParentId = new SelectList(categoryService.FindAll().ToList().OrderBy(c => c.NameWithParent), "Id",
                "NameWithParent");
        }

        // GET: Admin/Category/Edit/5
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.WRITE)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = categoryService.Find(id.Value);
            if (category == null)
            {
                return HttpNotFound();
            }

            var model = Mapper.Map<CategoryEditViewModel>(category);

            loadParentId();
            return View(model);
        }

        // POST: Admin/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.WRITE)]
        public ActionResult Edit(CategoryEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Category category = categoryService.AddOrUpdate(model);
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("Category \"{0}\" has been updated".TA(), category.Name));
            }

            loadParentId();
            return View(model);
        }

        // GET: Admin/Category/Delete/5
        [AccessAuthorize(OperatorRoles.INVENTORY + OperatorRoles.DELETE)]
        public ActionResult Delete(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new CategoriesDeleteViewModel();
            model.Categories = new List<CategoryDeleteViewModel>();

            foreach (int id in ids)
            {
                Category category = categoryService.Find(id);
                if (category == null) continue;

                model.Categories.Add(new CategoryDeleteViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    ProductsCount = categoryService.GetProductsCount(id, true)
                });
            }
            return View(model);
        }

        // POST: Admin/Category/Delete/5
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
                categoryDeleter.Delete(id);
            }

            return RedirectToAction("Index")
                .WithWarning("The selected categories have been deleted".TA());
        }
    }
}