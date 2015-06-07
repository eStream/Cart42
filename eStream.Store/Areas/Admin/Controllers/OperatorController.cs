using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class OperatorController : BaseController
    {
        private readonly IOperatorService operatorService;

        public OperatorController(IOperatorService operatorService)
        {
            this.operatorService = operatorService;
        }

        // GET: Admin/Operator
        [AccessAuthorize(OperatorRoles.OPERATORS)]
        public ActionResult Index()
        {
            var model = new OperatorsViewModel();

            var users = operatorService.FindAll();
            Mapper.Map(users, model.Operators);

            foreach (var op in model.Operators)
            {
                op.Roles = operatorService.GetRoles(op.Id);
            }

            return View(model);
        }

        [AccessAuthorize(OperatorRoles.OPERATORS + OperatorRoles.WRITE)]
        public ActionResult Create()
        {
            var model = new OperatorViewModel();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.OPERATORS + OperatorRoles.WRITE)]
        public ActionResult Create(OperatorViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    operatorService.AddOrUpdate(model);
                    return RedirectToAction("Index")
                        .WithSuccess(string.Format("Operator \"{0} {1}\" has been added".TA(),
                            model.FirstName, model.LastName));
                }
                catch (ArgumentException err)
                {
                    ModelState.AddModelError(err.ParamName ?? string.Empty, err.Message);
                }
            }

            return View(model);
        }

        // GET: Admin/Operator/Edit/5
        [AccessAuthorize(OperatorRoles.OPERATORS + OperatorRoles.WRITE)]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = operatorService.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var model = Mapper.Map<OperatorViewModel>(user);
            model.Roles = operatorService.GetRoles(id);
            
            return View(model);
        }

        // POST: Admin/Operator/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.OPERATORS + OperatorRoles.WRITE)]
        public ActionResult Edit(OperatorViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    operatorService.AddOrUpdate(model);
                    return RedirectToAction("Index")
                        .WithSuccess(string.Format("Operator \"{0} {1}\" has been updated".TA(),
                            model.FirstName, model.LastName));
                }
                catch (ArgumentException err)
                {
                    ModelState.AddModelError(err.ParamName ?? string.Empty, err.Message);
                }
            }

            return View(model);
        }

        [AccessAuthorize(OperatorRoles.OPERATORS + OperatorRoles.DELETE)]
        public ActionResult Delete(string[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            foreach (string id in ids)
            {
                operatorService.Delete(id);
            }

            return RedirectToAction("Index")
                .WithWarning("The selected operator have been deleted".TA());
        }
    }
}