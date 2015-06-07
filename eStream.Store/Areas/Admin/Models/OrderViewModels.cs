using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Models;
using FluentValidation;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class OrderViewModel : IMapFrom<Order>
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public OrderStatus Status { get; set; }

        public decimal Subtotal { get; set; }

        public decimal Discount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal ShippingAmount { get; set; }

        public decimal Total { get; set; }

        [Display(Name = "Date")]
        public DateTime DatePlaced { get; set; }

        public DateTime DateUpdated { get; set; }

        public string IPAddress { get; set; }

        public string UserComments { get; set; }

        public string StaffComments { get; set; }

        public AddressViewModel BillingAddress { get; set; }

        public AddressViewModel ShippingAddress { get; set; }

        public List<OrderItemViewModel> Items { get; set; }

        public bool CanBeShipped { get; set; }
    }

    public class OrderItemViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductSku { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        // Json of OrderItemOption[]
        public string Options { get; set; }

        [AllowHtml]
        public string OptionsDisplay { get; set; }

        public decimal ItemPrice { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<OrderItem, OrderItemViewModel>()
                .ForMember(m => m.ProductSku, opt => opt.MapFrom(f => f.ProductSku != null ? f.ProductSku.Sku : null));
        }
    }

    public class OrderEditViewModel : BaseEditViewModel, IHaveCustomMappings
    {
        public OrderEditViewModel()
        {
            BillingAddress = new AddressViewModel();
            ShippingAddress = new AddressViewModel();
            Items = new List<OrderItemEditViewModel>();
        }

        public int Id { get; set; }

        [Display(Name = "Customer Email")]
        public string UserEmail { get; set; }

        public OrderStatus Status { get; set; }

        public decimal Subtotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal ShippingAmount { get; set; }

        public decimal Discount { get; set; }

        public decimal Total { get; set; }

        public int ShippingMethodId { get; set; }

        public int PaymentMethodId { get; set; }

        public string TaxName { get; set; }

        public decimal TaxRate { get; set; }

        public string UserComments { get; set; }
        
        public string StaffComments { get; set; }

        public AddressViewModel BillingAddress { get; set; }

        public bool SameShippingAddress { get; set; }

        public AddressViewModel ShippingAddress { get; set; }

        public List<OrderItemEditViewModel> Items { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Order, OrderEditViewModel>();
            Mapper.CreateMap<OrderEditViewModel, Order>();
        }
    }

    public class OrderEditViewModelValidator : AbstractValidator<OrderEditViewModel>
    {
        public OrderEditViewModelValidator()
        {
            RuleFor(o => o.UserEmail).NotEmpty().EmailAddress();
            RuleFor(o => o.Discount).LessThanOrEqualTo(0);
            RuleFor(o => o.Items).Must(i => i.Count > 0)
                .WithMessage("At least one product is required".TA());
           
            RuleFor(o => o.BillingAddress).SetValidator(new AddressViewModelValidator());
            RuleFor(o => o.ShippingAddress).SetValidator(new AddressViewModelValidator())
                .Unless(o => o.SameShippingAddress);
        }
    }

    public class OrderItemEditViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int? ProductSkuId { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        [AllowHtml]
        public string OptionsDisplay { get; set; }

        // Json of OrderItemOption[]
        public string Options { get; set; }

        public decimal ItemPrice { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<OrderItem, OrderItemEditViewModel>();
            Mapper.CreateMap<OrderItemEditViewModel, OrderItem>();
        }
    }

    public class OrderItemEditViewModelValidator : AbstractValidator<OrderItemEditViewModel>
    {
        public OrderItemEditViewModelValidator()
        {
            RuleFor(i => i.ProductId).GreaterThan(0);
            RuleFor(i => i.Quantity).GreaterThan(0);
            RuleFor(i => i.ItemPrice).GreaterThan(0);
        }
    }
}