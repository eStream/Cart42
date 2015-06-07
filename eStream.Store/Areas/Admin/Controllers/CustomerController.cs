using System;
using System.Collections.Generic;
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
using LinqKit;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class CustomerController : BaseController
    {
        public const int DEFAULT_PAGE_SIZE = 20;

        private readonly ICustomerService customerService;

        public CustomerController(DataContext db, ICustomerService customerService)
            : base(db)
        {
            this.customerService = customerService;
        }

        // GET: Admin/Customer
        [AccessAuthorize(OperatorRoles.CUSTOMERS)]
        public ActionResult Index(int page = 1, int pageSize = DEFAULT_PAGE_SIZE, string keywords = null,
            string orderColumn = null, bool orderAsc = true)
        {
            var model = prepareCustomerSearchViewModel(page, pageSize, keywords, orderColumn, orderAsc);

            return View(model);
        }
        [AccessAuthorize(OperatorRoles.CUSTOMERS)]
        public StandardJsonResult List(int page = 1, int pageSize = DEFAULT_PAGE_SIZE, string keywords = null,
            string orderColumn = null, bool orderAsc = true)
        {
            var model = prepareCustomerSearchViewModel(page, pageSize, keywords, orderColumn, orderAsc);
            return JsonSuccess(model);
        }

        private CustomerIndexViewModel prepareCustomerSearchViewModel(int page = 1, int pageSize = DEFAULT_PAGE_SIZE,
            string keywords = null, string orderColumn = null, bool orderAsc = true)
        {
            if (keywords == "null") keywords = null;
            var orderExpr = customerService.CreateOrderExpr(orderColumn).Expand();
            var customers = customerService.Find(keywords, orderExpr, orderAsc);
            var model = new CustomerIndexViewModel
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = customers.Count(),
                OrderColumn = orderColumn,
                OrderAsc = orderAsc,
                Keywords = keywords,
            };
            model.TotalPages = ((model.TotalItems - 1) / pageSize) + 1;
            if (model.TotalPages < model.Page)
                model.Page = model.TotalPages;
            model.Customers = Mapper.Map<List<CustomerSearchViewModel>>(customers.GetPage(model.Page, model.PageSize));

            // Calculate orders count and load billing address
            model.Customers.Each(delegate(CustomerSearchViewModel c)
                                 {
                                     c.OrdersCount = db.Orders.Count(o => o.UserId == c.Id);
                                     c.BillingAddress = Mapper.Map<AddressViewModel>(customerService.GetAddress(c.Id,
                                             AddressType.Billing));
                                     c.LastOrders = Mapper.Map<List<CustomerSearchOrderViewModel>> (
                                         db.Orders.Where(o => o.UserId == c.Id)
                                             .OrderByDescending(o => o.Id)
                                             .Take(10).ToList());
                                 });

            return model;
        }

        // GET: Admin/Customer/Create
        [AccessAuthorize(OperatorRoles.CUSTOMERS + OperatorRoles.WRITE)]
        public ActionResult Create()
        {
            var model = new CustomerViewModel();

            ViewBag.Countries = db.Countries.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();

            return View(model);
        }

        // POST: Admin/Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.CUSTOMERS + OperatorRoles.WRITE)]
        public ActionResult Create(CustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    customerService.AddOrUpdate(model);
                    return RedirectToAction("Index")
                        .WithSuccess(string.Format("Customer \"{0} {1}\" has been added".TA(), model.BillingAddress.FirstName,
                            model.BillingAddress.LastName));
                }
                catch (ArgumentException err)
                {
                    ModelState.AddModelError(err.ParamName ?? "", err.Message);
                }
            }

            ViewBag.Countries = db.Countries.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();

            return View(model);
        }

        // GET: Admin/Customer/Edit/5
        [AccessAuthorize(OperatorRoles.CUSTOMERS + OperatorRoles.WRITE)]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = customerService.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var model = Mapper.Map<CustomerViewModel>(user);
            model.BillingAddress = Mapper.Map<AddressViewModel>(
                customerService.GetAddress(user.Id, AddressType.Billing) ?? new Address());
            model.ShippingAddress = Mapper.Map<AddressViewModel>(
                customerService.GetAddress(user.Id, AddressType.Shipping) ?? new Address());

            ViewBag.Countries = db.Countries.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();

            return View(model);
        }

        // POST: Admin/Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.CUSTOMERS + OperatorRoles.WRITE)]
        public ActionResult Edit(CustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    customerService.AddOrUpdate(model);
                    return RedirectToAction("Index")
                        .WithSuccess(string.Format("Customer \"{0} {1}\" has been updated".TA(), 
                        model.BillingAddress.FirstName, model.BillingAddress.LastName));
                }
                catch (ArgumentException err)
                {
                    ModelState.AddModelError(err.ParamName, err.Message);
                }
            }

            ViewBag.Countries = db.Countries.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();

            return View(model);
        }

        // GET: Admin/Customer/Delete/5
        [AccessAuthorize(OperatorRoles.CUSTOMERS + OperatorRoles.DELETE)]
        public ActionResult Delete(string[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new CustomersDeleteViewModel();
            model.Customers = new List<CustomerDeleteViewModel>();

            foreach (string id in ids)
            {
                User user = customerService.Find(id);
                if (user == null) continue;

                model.Customers.Add(new CustomerDeleteViewModel
                {
                    Id = user.Id,
                    Name = user.FirstName + " " + user.LastName,
                    Email = user.Email
                });
            }
            return View(model);
        }

        // POST: Admin/Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.CUSTOMERS + OperatorRoles.DELETE)]
        public ActionResult DeleteConfirmed(string[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            foreach (string id in ids)
            {
               customerService.Delete(id);
            }

            return RedirectToAction("Index")
                .WithWarning("The selected customers have been deleted".TA());
        }
    }
}
