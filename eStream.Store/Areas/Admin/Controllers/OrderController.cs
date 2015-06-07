using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class OrderController : BaseController
    {
        private readonly ICustomerService customerService;
        private readonly IProductFinder productFinder;
        private readonly IShippingService shippingService;
        private readonly ITaxService taxService;
        private readonly ISettingService settingService;
        private readonly IOrderService orderService;
        private readonly IOptionService optionService;

        public OrderController(DataContext db, ICustomerService customerService, IProductFinder productFinder,
            IShippingService shippingService, ITaxService taxService, ISettingService settingService,
            IOrderService orderService, IOptionService optionService) : base(db)
        {
            this.customerService = customerService;
            this.productFinder = productFinder;
            this.shippingService = shippingService;
            this.taxService = taxService;
            this.settingService = settingService;
            this.orderService = orderService;
            this.optionService = optionService;
        }

        // GET: Admin/Order
        [AccessAuthorize(OperatorRoles.ORDERS)]
        public ActionResult Index()
        {
            List<Order> orders = db.Orders
                .Include(o => o.BillingAddress)
                .Include(o => o.ShippingAddress)
                .OrderByDescending(o => o.Id)
                .ToList();

            var model = Mapper.Map<List<OrderViewModel>>(orders);

            foreach (var orderViewModel in model)
            {
                if (orderViewModel.Status == OrderStatus.Cancelled || orderViewModel.Status == OrderStatus.Returned
                    || orderViewModel.Status == OrderStatus.Shipped) continue;

                // TODO: Check actual shipments
                orderViewModel.CanBeShipped = true;
            }

            return View(model);
        }

        // POST: Admin/Order/UpdateStatus?orderId=5&status=1
        [AccessAuthorize(OperatorRoles.ORDERS)]
        public StandardJsonResult UpdateStatus(int orderId, int status)
        {
            Order order = db.Orders.Find(orderId);
            if (order == null)
                return JsonError("Unable to find order".TA());

            order.Status = (OrderStatus) status;
            db.SaveChanges();

            return JsonSuccess(string.Format("The status for order #{0} has been updated".TA(), orderId));
        }

        [AccessAuthorize(OperatorRoles.ORDERS + OperatorRoles.WRITE)]
        public ActionResult Create()
        {
            var model = new OrderEditViewModel();
            ViewBag.Countries = db.Countries.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
            return View(model);
        }

        [HttpPost]
        [AccessAuthorize(OperatorRoles.ORDERS + OperatorRoles.WRITE)]
        public ActionResult Create(OrderEditViewModel model)
        {
            if (!ModelState.IsValid)
                return JsonValidationError();

            var user = customerService.FindAll().FirstOrDefault(u => u.Email == model.UserEmail);
            if (user == null)
            {
                var userModel = new CustomerViewModel
                                {
                                    FirstName = model.BillingAddress.FirstName,
                                    LastName = model.BillingAddress.LastName,
                                    Company = model.BillingAddress.Company,
                                    PhoneNumber = model.BillingAddress.Phone,
                                    Email = model.UserEmail
                                };
                try
                {
                    user = customerService.AddOrUpdate(userModel);
                }
                catch (ArgumentException err)
                {
                    return JsonError(err.Message);
                }
            }

            // Get addresses
            var billingAddress = Mapper.Map<Address>(model.BillingAddress);
            billingAddress.Type = AddressType.Billing;

            var shippingAddress = Mapper.Map<Address>(model.SameShippingAddress
                ? model.BillingAddress
                : model.ShippingAddress);
            shippingAddress.Type = AddressType.Shipping;

            var defaultBillingAddress = customerService.GetAddress(user.Id, AddressType.Billing);
            if (defaultBillingAddress == null || defaultBillingAddress.FirstName == null)
            {
                // Add default billing address
                defaultBillingAddress = Mapper.Map<Address>(model.BillingAddress);
                defaultBillingAddress.Type = AddressType.Billing;
                defaultBillingAddress.IsPrimary = true;
                if (defaultBillingAddress.Id == 0)
                    user.Addresses.Add(defaultBillingAddress);
            }

            var defaultShippingAddress = customerService.GetAddress(user.Id, AddressType.Shipping);
            if (defaultShippingAddress == null || defaultBillingAddress.FirstName == null)
            {
                // Add default shipping address
                defaultShippingAddress = Mapper.Map<Address>(model.SameShippingAddress
                    ? model.BillingAddress
                    : model.ShippingAddress);
                defaultShippingAddress.Type = AddressType.Shipping;
                defaultShippingAddress.IsPrimary = true;
                if (defaultShippingAddress.Id == 0)
                    user.Addresses.Add(defaultShippingAddress);
            }

            db.SaveChanges();

            // Create order
            var order = new Order
                        {
                            UserId = user.Id,
                            BillingAddress = billingAddress,
                            ShippingAddress = shippingAddress,
                            DatePlaced = DateTime.Now,
                            DateUpdated = DateTime.Now,
                            IPAddress = Request.UserHostAddress,
                            UserComments = model.UserComments,
                            Status = OrderStatus.AwaitingPayment,
                            Discount = model.Discount,
                            ShippingAmount = model.ShippingAmount,
                        };

            db.Orders.Add(order);

            var itemDiscount = 0m;
            if (model.Discount < 0)
            {
                itemDiscount = model.Discount / model.Items.Count;
            }

            foreach (var item in model.Items)
            {
                Product product = productFinder.Find(item.ProductId);

                var orderItem = new OrderItem
                                {
                                    Order = order,
                                    ProductId = item.ProductId,
                                    ProductSkuId = item.ProductSkuId,
                                    Quantity = item.Quantity,
                                    Options = item.Options,
                                    ItemPrice = item.ItemPrice
                                };

                db.OrderItems.Add(orderItem);

                order.Subtotal += orderItem.Quantity * orderItem.ItemPrice;

                order.TaxAmount += taxService.CalculateTax(billingAddress.CountryCode, billingAddress.RegionId,
                    product.TaxClassId, (orderItem.ItemPrice + itemDiscount) * orderItem.Quantity);
            }

            order.Total = order.Subtotal + order.Discount + order.ShippingAmount;
            if (!settingService.Get<bool>(SettingField.TaxIncludedInPrices))
                order.Total += order.TaxAmount;

            db.SaveChanges();

            return JsonSuccess(new { orderId = order.Id })
                .WithSuccess("Order created successfully".TA());
        }

        [AccessAuthorize(OperatorRoles.ORDERS + OperatorRoles.WRITE)]
        public ActionResult Edit(int id)
        {
            var order = orderService.Find(id);
            var model = Mapper.Map<OrderEditViewModel>(order);
            foreach (var item in model.Items)
            {
                string optionsDisplay = "";
                var options = JsonConvert.DeserializeObject<OrderItemOption[]>(item.Options);
                foreach (var optionId in options.Select(o => o.Id))
                {
                    var option = optionService.Find(optionId);
                    if (option == null) continue;
                    optionsDisplay += string.Format("<strong>{0}</strong>: {1} ", option.Category.Name, option.Name);
                }
                item.OptionsDisplay = optionsDisplay;
            }

            ViewBag.Countries = db.Countries.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
            return View(model);
        }

        [HttpPost]
        [AccessAuthorize(OperatorRoles.ORDERS + OperatorRoles.WRITE)]
        public ActionResult Edit(OrderEditViewModel model)
        {
            if (!ModelState.IsValid)
                return JsonValidationError();

            var order = orderService.Find(model.Id);

            Mapper.Map(model.BillingAddress, order.BillingAddress);
            Mapper.Map(model.ShippingAddress, order.ShippingAddress);

            order.DateUpdated = DateTime.Now;
            order.UserComments = model.UserComments;
            order.Discount = model.Discount;
            order.ShippingAmount = model.ShippingAmount;
            order.Subtotal = 0;
            order.TaxAmount = 0;

            var itemDiscount = 0m;
            if (model.Discount < 0)
            {
                itemDiscount = model.Discount / model.Items.Count;
            }

            foreach (var item in model.Items)
            {
                if (item.Id == 0)
                {
                    var orderItem = new OrderItem
                                    {
                                        Order = order,
                                        ProductId = item.ProductId,
                                        ProductSkuId = item.ProductSkuId,
                                        Quantity = item.Quantity,
                                        Options = item.Options,
                                        ItemPrice = item.ItemPrice
                                    };

                    db.OrderItems.Add(orderItem);
                }
                else
                {
                    var orderItem = order.Items.First(i => i.Id == item.Id);
                    Mapper.Map(item, orderItem);
                }

                order.Subtotal += item.Quantity * item.ItemPrice;

                Product product = productFinder.Find(item.ProductId);
                order.TaxAmount += taxService.CalculateTax(model.BillingAddress.CountryCode,
                    model.BillingAddress.RegionId, product.TaxClassId, (item.ItemPrice + itemDiscount)*item.Quantity);
            }

            order.Total = order.Subtotal + order.Discount + order.ShippingAmount;
            if (!settingService.Get<bool>(SettingField.TaxIncludedInPrices))
                order.Total += order.TaxAmount;

            db.SaveChanges();

            return JsonSuccess(new { orderId = order.Id })
                .WithSuccess("Order updated successfully".TA());
        }
        [AccessAuthorize(OperatorRoles.ORDERS)]
        public StandardJsonResult LoadUserByEmail(string email)
        {
            var user = customerService.FindAll().FirstOrDefault(c => c.Email == email);
            if (user == null) return JsonSuccess<string>(null);

            return JsonSuccess(new
                               {
                                   BillingAddress =
                                       Mapper.Map<AddressViewModel>(customerService.GetAddress(user.Id,
                                           AddressType.Billing)),
                                   ShippingAddress =
                                       Mapper.Map<AddressViewModel>(customerService.GetAddress(user.Id,
                                           AddressType.Shipping))
                               });
        }

        [AccessAuthorize(OperatorRoles.ORDERS)]
        public StandardJsonResult FindProducts(string keywords)
        {
            var products = productFinder.Find(keywords: keywords, visible: null)
                .Select(p => new { p.Id, p.Name }).Take(100).ToList();

            return JsonSuccess(products);
        }

        [AccessAuthorize(OperatorRoles.ORDERS)]
        public StandardJsonResult GetProductDetails(int id)
        {
            Product product = productFinder.Find(id);
            if (product == null)
            {
                return JsonError("Product not found".TA());
            }

            var model = Mapper.Map<ProductDetailsViewModel>(product);
            var optionCategories = product.Options.Select(o => o.Category).Distinct();
            model.OptionCategories = Mapper.Map<List<ProductOptionCategoryViewModel>>(optionCategories);
            foreach (var optionCategory in model.OptionCategories)
            {
                var optionCategoryId = optionCategory.Id;
                optionCategory.Options = Mapper.Map<List<ProductOptionViewModel>>(
                    product.Options.Where(o => o.OptionCategoryId == optionCategoryId));
            }

            return JsonSuccess(model);
        }

        [AccessAuthorize(OperatorRoles.ORDERS)]
        public StandardJsonResult GetShippingMethods(OrderEditViewModel model)
        {
            var result = new List<ShoppingCartShippingMethodsViewModel>();
            AddressViewModel address = model.SameShippingAddress ? model.BillingAddress : model.ShippingAddress;

            // Find zone for country and region (if supplied)
            ShippingZone zone = shippingService.FindZone(address.CountryCode, address.RegionId);

            if (zone != null && model.Items.Any())
            {
                List<ShippingMethod> methods = shippingService.FindMethods(zone.Id).ToList();
                foreach (ShippingMethod method in methods)
                {
                    decimal? cost = shippingService.CalculateShipping(method,
                        model.Items.Sum(i => i.Quantity),
                        model.Items.Sum(i => i.Quantity * db.Products.Find(i.ProductId).Weight),
                        model.Items.Sum(i => i.Quantity * i.ItemPrice), Mapper.Map<Address>(address));

                    if (!cost.HasValue) continue;

                    var methodView = new ShoppingCartShippingMethodsViewModel
                    {
                        Id = method.Id,
                        Name = method.Name,
                        Amount = cost.Value
                    };

                    result.Add(methodView);
                }
            }

            return JsonSuccess(result.OrderBy(s => s.Amount));
        }

        [AccessAuthorize(OperatorRoles.ORDERS)]
        public StandardJsonResult CalculateTax(OrderEditViewModel model)
        {
            var tax = 0m;

            var itemDiscount = 0m;
            if (model.Discount < 0)
            {
                itemDiscount = model.Discount / model.Items.Count;
            }

            foreach (var item in model.Items)
            {
                var taxClassId = productFinder.Find(item.ProductId).TaxClassId;
                tax += taxService.CalculateTax(model.BillingAddress.CountryCode, model.BillingAddress.RegionId,
                    taxClassId, item.Quantity * (item.ItemPrice + itemDiscount));
            }

            return JsonSuccess(tax);
        }
    }
}