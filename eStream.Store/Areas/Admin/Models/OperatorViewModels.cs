using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using FluentValidation;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class OperatorsViewModel
    {
        public OperatorsViewModel()
        {
            Operators = new List<OperatorViewModel>();
        }

        public List<OperatorViewModel> Operators { get; set; }
    }

    public class OperatorViewModel : IHaveCustomMappings
    {
        public OperatorViewModel()
        {
            IsActive = true;
            Roles = new List<string>();
        }

        public string Id { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Last Login")]
        public string LastLogin { get; set; }

        [IgnoreMap]
        public List<string> Roles { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<User, OperatorViewModel>()
                .ForMember(m => m.Roles, opt => opt.Ignore());
            Mapper.CreateMap<OperatorViewModel, User>()
                .ForMember(m => m.UserName, opt => opt.MapFrom(i => i.Email))
                .ForMember(m => m.Id, opt => opt.Condition(i => !i.IsSourceValueNull))
                .ForMember(m => m.Roles, opt => opt.Ignore());
        }
    }

    public class OperatorViewModelValidator : AbstractValidator<OperatorViewModel>
    {
        public OperatorViewModelValidator()
        {
            RuleFor(m => m.FirstName).NotEmpty();
            RuleFor(m => m.LastName).NotEmpty();
            RuleFor(m => m.Email).NotEmpty().EmailAddress();
            RuleFor(m => m.NewPassword).Length(5, 50);
        }
    }
}