using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Helpers;
using FluentValidation;

namespace Estream.Cart42.Web.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
    public class ExternalLoginConfirmationViewModelValidator : AbstractValidator<ExternalLoginConfirmationViewModel>
    {
        public ExternalLoginConfirmationViewModelValidator()
        {
            RuleFor(c => c.Email).EmailAddress().NotEmpty();
        }
    }

    public class ExternalLoginListViewModel
    {
        public string Action { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class ManageUserViewModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; }
    }
    public class ManageUserViewModelValidator : AbstractValidator<ManageUserViewModel>
    {
        public ManageUserViewModelValidator()
        {
            RuleFor(c => c.OldPassword).NotEmpty();
            RuleFor(c => c.NewPassword).Length(6, 100).NotEmpty();
            RuleFor(c => c.ConfirmPassword).Equal(c => c.NewPassword).WithMessage("The new password and confirmation password do not match.".T());
        }
    }

    public class LoginViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
    public class LoginViewModelValidator : AbstractValidator<LoginViewModel>
    {
        public LoginViewModelValidator()
        {
            RuleFor(c => c.Email).EmailAddress().NotEmpty();
            RuleFor(c => c.Password).NotEmpty();
        }
    }

    public class RegisterViewModel
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
    public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
    {
        public RegisterViewModelValidator()
        {
            RuleFor(c => c.FirstName).NotEmpty();
            RuleFor(c => c.LastName).NotEmpty();
            RuleFor(c => c.Email).EmailAddress().NotEmpty();
            RuleFor(c => c.Password).Length(6, 100).NotEmpty();
            RuleFor(c => c.ConfirmPassword).Equal(c => c.Password).WithMessage("The password and confirmation password do not match.".T());
        }
    }

    public class ResetPasswordViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
    public class ResetPasswordViewModelValidator : AbstractValidator<ResetPasswordViewModel>
    {
        public ResetPasswordViewModelValidator()
        {
            RuleFor(c => c.Email).EmailAddress().NotEmpty();
            RuleFor(c => c.Password).Length(6, 100).NotEmpty();
            RuleFor(c => c.ConfirmPassword).Equal(c => c.Password).WithMessage("The password and confirmation password do not match.".T());
        }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
    public class ForgotPasswordViewModelValidator : AbstractValidator<ForgotPasswordViewModel>
    {
        public ForgotPasswordViewModelValidator()
        {
            RuleFor(c => c.Email).EmailAddress().NotEmpty();
        }
    }

    public class AccountIndexViewModel
    {
        public AccountIndexViewModel()
        {
            Orders = new List<OrderViewModel>();
        }

        public List<OrderViewModel> Orders { get; set; }
    }

}