using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Models
{
    public class AddToCartViewModel
    {
        public int ProductId { get; set; }

        public int? Quantity { get; set; }

        public string Options { get; set; }
    }

    public class ShoppingCartViewModel
    {
        public List<ShoppingCartItemViewModel> Items { get; set; }
    }

    public class ShoppingCartItemViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Display(Name = "Sku")]
        public string ProductSku { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public string[] Options { get; set; }

        [Display(Name = "Item Price")]
        public decimal ItemPrice { get; set; }

        [Display(Name = "Item Total")]
        public decimal ItemTotal
        {
            get { return Quantity*ItemPrice; }
        }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<ShoppingCartItem, ShoppingCartItemViewModel>()
                .ForMember(m => m.Options, opt => opt.MapFrom(i => OptionHelper.GetOptionNames(i.Options)))
                .ForMember(m => m.ProductSku, opt => opt.MapFrom(f => f.ProductSku != null ? f.ProductSku.Sku : null));
        }
    }

    public class ShoppingCartItemOptionViewModel
    {
        public int id { get; set; }
    }

    public class ShoppingCartCheckoutViewModel : IHaveCustomMappings
    {
        public ShoppingCartCheckoutViewModel()
        {
            BillingAddress = new AddressViewModel { Type = AddressType.Billing };
            ShippingAddress = new AddressViewModel { Type = AddressType.Shipping };
            ShippingMethods = new List<ShoppingCartShippingMethodsViewModel>();
            SameShippingAddress = true;
        }

        public ShoppingCartCheckoutViewModel(ISettingService settings) : this()
        {
            TaxName = settings.Get<string>(SettingField.TaxLabel);
        }

        public string Email { get; set; }

        public decimal Subtotal
        {
            get
            {
                if (ShoppingCartItems == null || !ShoppingCartItems.Any()) return 0;
                return ShoppingCartItems.Sum(i => i.Quantity*i.ItemPrice);
            }
        }

        public int ShippingMethodId { get; set; }

        public int PaymentMethodId { get; set; }

        public string TaxName { get; set; }

        public decimal TaxRate { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal ShippingAmount { get; set; }

        public string UserComments { get; set; }

        public List<ShoppingCartItemCheckoutViewModel> ShoppingCartItems { get; set; }

        public AddressViewModel BillingAddress { get; set; }

        public bool SameShippingAddress { get; set; }

        public AddressViewModel ShippingAddress { get; set; }

        public List<ShoppingCartShippingMethodsViewModel> ShippingMethods { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<ShoppingCart, ShoppingCartCheckoutViewModel>()
                .ConstructUsingServiceLocator();
        }
    }

    public class ShoppingCartItemCheckoutViewModel : IMapFrom<ShoppingCartItem>
    {
        public int ProductId { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public string[] Options { get; set; }

        [Display(Name = "Item Price")]
        public decimal ItemPrice { get; set; }
    }

    public class ShoppingCartShippingMethodsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }
}