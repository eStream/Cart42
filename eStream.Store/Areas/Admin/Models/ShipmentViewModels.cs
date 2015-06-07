using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AutoMapper;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class ShipmentIndexViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        public OrderViewModel Order { get; set; }

        public DateTime Date { get; set; }

        public string TrackingNo { get; set; }

        public List<ShipmentItemIndexViewModel> Items { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Shipment, ShipmentIndexViewModel>();
        }
    }

    public class ShipmentItemIndexViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        public int OrderItemId { get; set; }

        public int Quantity { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<ShipmentItem, ShipmentItemIndexViewModel>();
        }
    }

    public class ShipmentEditViewModel : IHaveCustomMappings
    {
        public ShipmentEditViewModel()
        {
            Items = new List<ShipmentItemEditViewModel>();
            Date = DateTime.Now;
            UpdateOrderStatus = true;
        }

        public int Id { get; set; }

        public int OrderId { get; set; }

        public DateTime Date { get; set; }

        [Display(Name = "Tracking Number")]
        public string TrackingNo { get; set; }

        public OrderViewModel Order { get; set; }

        public List<ShipmentItemEditViewModel> Items { get; set; }

        [Display(Name = "Order Status")]
        public bool UpdateOrderStatus { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Shipment, ShipmentEditViewModel>();
            Mapper.CreateMap<ShipmentEditViewModel, Shipment>();
        }
    }

    public class ShipmentItemEditViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        public int OrderItemId { get; set; }

        public int Quantity { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<ShipmentItem, ShipmentItemEditViewModel>();
            Mapper.CreateMap<ShipmentItemEditViewModel, ShipmentItem>();
        }
    }
}