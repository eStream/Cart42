using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Models;
using Estream.Cart42.Web.Services;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class ShipmentController : BaseController
    {
        private readonly IShipmentService shipmentService;
        private readonly IOrderService orderService;
        private readonly IOptionService optionService;

        public ShipmentController(IShipmentService shipmentService, IOrderService orderService, IOptionService optionService)
        {
            this.shipmentService = shipmentService;
            this.orderService = orderService;
            this.optionService = optionService;
        }

        // GET: Admin/Shipment
        [AccessAuthorize(OperatorRoles.SHIPMENTS)]
        public ActionResult Index(int? orderId = null)
        {
            IQueryable<Shipment> shipments = orderId.HasValue
                ? shipmentService.FindByOrder(orderId.Value)
                : shipmentService.FindAll();
            var model = Mapper.Map<List<ShipmentIndexViewModel>>(shipments.OrderByDescending(s => s.Id));

            return View(model);
        }

        [AccessAuthorize(OperatorRoles.SHIPMENTS + OperatorRoles.WRITE)]
        public ActionResult Create(int orderId)
        {
            var order = orderService.Find(orderId);
            var model = new ShipmentEditViewModel();
            model.OrderId = orderId;
            model.Order = Mapper.Map<OrderViewModel>(order);

            var prevShipments = shipmentService.FindByOrder(orderId).ToList();

            foreach (var orderItemViewModel in model.Order.Items)
            {
                var prevShippedQty =
                    prevShipments.Sum(
                        s => s.Items.Where(i => i.OrderItemId == orderItemViewModel.Id)
                            .Select(i => i.Quantity).DefaultIfEmpty(0)
                            .Sum());

                if (orderItemViewModel.Quantity > prevShippedQty)
                {
                    model.Items.Add(new ShipmentItemEditViewModel
                                    {
                                        OrderItemId = orderItemViewModel.Id,
                                        Quantity = orderItemViewModel.Quantity - prevShippedQty
                                    });

                    var options = JsonConvert.DeserializeObject<List<OrderItemOption>>(orderItemViewModel.Options);

                    orderItemViewModel.OptionsDisplay = string.Join(" ", options.Select(o =>
                    {
                        var option = optionService.Find(o.Id);
                        if (option == null) return "[deleted]";
                        return string.Format("<strong>{0}</strong>: {1}", option.Category.Name, option.Name);
                    }).ToList());
                }
            }

            if (model.Items.None())
            {
                return RedirectToAction("Index")
                    .WithError(string.Format("All items for order #{0} are already shipped".TA(), order.Id));
            }

            return View(model);
        }

        [HttpPost]
        [AccessAuthorize(OperatorRoles.SHIPMENTS + OperatorRoles.WRITE)]
        public ActionResult Create(ShipmentEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                foreach (var item in model.Items.ToList())
                {
                    if (item.Quantity <= 0)
                        model.Items.Remove(item);
                }

                if (model.Items.None())
                {
                    return RedirectToAction("Create", new { orderId = model.OrderId })
                        .WithError("No products are selected".TA());
                }

                var shipment = shipmentService.AddOrUpdate(model);

                if (model.UpdateOrderStatus)
                {
                    var order = orderService.Find(model.OrderId);
                    var orderItemsQty = order.Items.Sum(i => i.Quantity);

                    var allShipments = shipmentService.FindByOrder(model.OrderId).ToList();
                    var shippedItemsQty = allShipments.Sum(s => s.Items.Sum(i => i.Quantity));

                    orderService.SetStatus(model.OrderId, orderItemsQty == shippedItemsQty
                        ? OrderStatus.Shipped
                        : OrderStatus.PartiallyShipped);
                }

                return RedirectToAction("Index")
                    .WithSuccess(string.Format("Shipment \"{0}\" has been added".TA(), shipment.Id));
            }

            return View(model);
        }
    }
}