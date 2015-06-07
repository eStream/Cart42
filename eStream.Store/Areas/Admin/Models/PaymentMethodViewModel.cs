using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using FluentValidation;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class PaymentMethodViewModel : IHaveCustomMappings
    {
        public PaymentMethodViewModel()
        {
            CountryCodes = new List<string>();
        }

        public int Id { get; set; }

        [Display(Name = "Display Name")]
        public string Name { get; set; }

        [Display(Name = "Countries")]
        public List<string> CountryCodes { get; set; }

        public PaymentMethodType Type { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<PaymentMethod, PaymentMethodViewModel>()
                .ForMember(s => s.CountryCodes, opt => opt.Ignore());
            Mapper.CreateMap<PaymentMethodViewModel, PaymentMethod>()
                .ForMember(s => s.Type, opt => opt.Ignore())
                .ForMember(s => s.Countries, opt => opt.Ignore());
        }
    }

    public class PaymentMethodsViewModel
    {
        public List<PaymentMethodViewModel> PaymentMethods { get; set; }
    }

    public class PaymentMethodViewModelValidator : AbstractValidator<PaymentMethodViewModel>
    {
        public PaymentMethodViewModelValidator()
        {
            RuleFor(r => r.Name).NotEmpty();
        }
    }
}