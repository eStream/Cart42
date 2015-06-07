using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Models;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Controllers
{
    public class SearchController : BaseController
    {
        private readonly ICategoryService categoryService;

        public SearchController(ICategoryService categoryService)
        {
            this.categoryService = categoryService;
        }

        [ChildActionOnly]
        [OutputCache(Duration = 5*60, VaryByParam = "*")]
        public PartialViewResult SearchBox(int? categoryId, string keywords)
        {
            var model = new SearchViewModel {CategoryId = categoryId, Keywords = keywords};
            model.Categories = categoryService.FindByParent(categoryId).ToArray();
            return PartialView("_SearchBox", model);
        }

        [ChildActionOnly]
        public ActionResult Breadcrumb(int? categoryId, string keywords)
        {
            var model = categoryId.HasValue
                ? categoryService.GetBreadcrumbNodes(categoryId.Value)
                : new BreadcrumbViewModel {Nodes = new List<Tuple<string, string>>()};

            model.Nodes.Add(new Tuple<string, string>(Url.Action("Results", "Search", new {keywords}),
                string.Format("Search results for \"{0}\"".T(), keywords)));

            return PartialView("_Breadcrumb", model);
        }

        // GET: /Search/Results?CategoryId=5&Keywords=test
        public ActionResult Results(int? categoryId, string keywords)
        {
            var model = new SearchResultsViewModel {CategoryId = categoryId, Keywords = keywords};
            ViewBag.SearchKeywords = keywords;
            return View(model);
        }
    }
}