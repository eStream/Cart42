using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Estream.Cart42.Web.Domain
{
    public class User : IdentityUser
    {
        public User()
        {
            DateRegistered = DateTime.Now;
            Addresses = new Collection<Address>();
            Orders = new Collection<Order>();
            Returns = new Collection<Return>();
        }

        public const string ADMIN_ROLE = "admin";
        public const string OPERATOR_ROLE = "operator";
        public const string CUSTOMER_ROLE = "customer";

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string Company { get; set; }

        public decimal StoreCredit { get; set; }

        public DateTime DateRegistered { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }

        public virtual ICollection<Order> Orders { get; set; }

        public virtual ICollection<Return> Returns { get; set; }

        public virtual ICollection<BlogPostComment> BlogPostComments { get; set; }

        public virtual ICollection<BlogPost> BlogPosts { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            ClaimsIdentity userIdentity =
                await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}