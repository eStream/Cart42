using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using FluentValidation;

namespace Estream.Cart42.Web.Models
{
    public class AddressViewModel : IHaveCustomMappings
    {
        public int Id { get; set; }

        public AddressType Type { get; set; }

        public bool IsPrimary { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Company")]
        public string Company { get; set; }

        [Display(Name = "Address")]
        public string Address1 { get; set; }

        [Display(Name = "Address 2")]
        public string Address2 { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        public string CityStateZip
        {
            get
            {
                if (string.IsNullOrEmpty(City) && string.IsNullOrEmpty(RegionName) && string.IsNullOrEmpty(ZipPostal))
                    return string.Empty;
                return string.Format("{0}, {1} {2}",
                    City, RegionName, ZipPostal);
            }
        }

        [Display(Name = "Country")]
        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        [Display(Name = "Region")]
        public int? RegionId { get; set; }

        public string RegionOther { get; set; }

        public string RegionName { get; set; }

        [Display(Name = "Zip/Postal Code")]
        public string ZipPostal { get; set; }

        [Display(Name = "Phone")]
        public string Phone { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<Address, AddressViewModel>()
                .ForMember(m => m.CountryName, opt => opt.MapFrom(i => i.Country.Name))
                .ForMember(m => m.RegionName, opt => opt.MapFrom(i => i.RegionOther ?? i.Region.Name));

            Mapper.CreateMap<AddressViewModel, Address>()
                .ForMember(m => m.RegionOther, opt => opt.MapFrom(i => i.RegionId == null ? i.RegionOther : null));
        }
    }

    [DontAutoWireup]
    public class AddressViewModelValidator : AbstractValidator<AddressViewModel>
    {
        public AddressViewModelValidator()
        {
            RuleFor(a => a.FirstName).NotEmpty();
            RuleFor(a => a.LastName).NotEmpty();
            RuleFor(a => a.Address1).NotEmpty();
            RuleFor(a => a.CountryCode).NotEmpty();
            RuleFor(a => a.RegionOther).NotEmpty().When(a => !a.RegionId.HasValue);
            RuleFor(a => a.Phone).NotEmpty();
        }
    }
}