using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ExceptionServices;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Models;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class CustomerIndexViewModel : IPageableViewModel, ISortableViewModel
    {
        public CustomerIndexViewModel()
        {
            Page = 1;
            OrderAsc = true;
            Keywords = string.Empty;
        }

        public IList<CustomerSearchViewModel> Customers { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public int TotalItems { get; set; }

        public string OrderColumn { get; set; }

        public bool OrderAsc { get; set; }

        public string Keywords { get; set; }
    }

    public class CustomerSearchViewModel : IHaveCustomMappings
    {
        public CustomerSearchViewModel()
        {
            BillingAddress = new AddressViewModel {Type = AddressType.Billing};
            LastOrders = new List<CustomerSearchOrderViewModel>();
        }

        public string Id { get; set; }


        public string Name
        {
            get { return FirstName + " " + LastName; }
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime DateRegistered { get; set; }

        [IgnoreMap]
        public AddressViewModel BillingAddress { get; set; }

        [IgnoreMap]
        public int OrdersCount { get; set; }

        [IgnoreMap]
        public List<CustomerSearchOrderViewModel> LastOrders { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<User, CustomerSearchViewModel>();
        }
    }

    public class CustomerSearchOrderViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public decimal Total { get; set; }

        public DateTime DatePlaced { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Order, CustomerSearchOrderViewModel>()
                .ForMember(m => m.Status, opt => opt.MapFrom(s => s.Status.DisplayName()));
        }
    }

    public class CustomerViewModel : IHaveCustomMappings
    {
        private string firstName;
        private string lastName;

        public CustomerViewModel()
        {
            BillingAddress = new AddressViewModel { Type = AddressType.Billing };
            ShippingAddress = new AddressViewModel { Type = AddressType.Shipping };
        }

        [Key]
        public string Id { get; set; }

        [Display(Name = "Email")]
        [Required, EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        public string FirstName
        {
            get
            {
                if (firstName == null && !string.IsNullOrEmpty(Email))
                {
                    return Email.Split('@')[0];
                }
                return firstName ?? string.Empty;
            }
            set { firstName = value; }
        }

        public string LastName
        {
            get { return lastName ?? string.Empty; }
            set { lastName = value; }
        }

        public string Company { get; set; }

        public string PhoneNumber { get; set; }

        [IgnoreMap]
        public AddressViewModel BillingAddress { get; set; }

        [IgnoreMap]
        public AddressViewModel ShippingAddress { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<User, CustomerViewModel>();
            Mapper.CreateMap<CustomerViewModel, User>()
                .ForMember(m => m.UserName, opt => opt.MapFrom(i => i.Email))
                .ForMember(m => m.Id, opt => opt.Condition(i => !i.IsSourceValueNull));
        }
    }

    public class CustomersDeleteViewModel
    {
        public List<CustomerDeleteViewModel> Customers { get; set; }
    }

    public class CustomerDeleteViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }
    }

}