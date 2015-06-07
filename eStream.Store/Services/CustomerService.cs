using System;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI.WebControls.Expressions;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using LinqKit;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;

namespace Estream.Cart42.Web.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly DataContext db;
        private readonly IDeleterService deleteService;
        private static string _customerRoleID;
        private static readonly object padlock = new object();
        private readonly UserManager<User> userManager;

        public CustomerService(DataContext db, IUserStore<User> userStore, IRoleStore<IdentityRole, string> roleStore,IDeleterService deleteService)
        {
            this.db = db;
            this.deleteService = deleteService;
            userManager = new UserManager<User>(userStore);

            if (_customerRoleID == null)
            {
                lock (padlock)
                {
                    if (_customerRoleID == null)
                    {
                        var roleManager = new RoleManager<IdentityRole>(roleStore);
                        // ReSharper disable PossibleMultipleWriteAccessInDoubleCheckLocking
                        _customerRoleID = roleManager.FindByName(User.CUSTOMER_ROLE).Id;
                        // ReSharper restore PossibleMultipleWriteAccessInDoubleCheckLocking
                    }
                }
            }
        }

        public IQueryable<User> FindAll()
        {
            return db.Users.Where(u => u.Roles.Any(r => r.RoleId == _customerRoleID));
        }

        public User Find(string id)
        {
            return FindAll().FirstOrDefault(u => u.Id == id);
        }

        public IQueryable<User> Find(string keywords = null,
            Expression<Func<User, object>> orderExpr = null, bool orderAsc = true)
        {
            IQueryable<User> users = db.Users
                .Where(u => u.Roles.Any(r => r.RoleId == _customerRoleID))
                .AsQueryable();
            Expression<Func<User, bool>> whereExpr = PredicateBuilder.True<User>();

            #region Keywords filter

            if (!string.IsNullOrWhiteSpace(keywords))
            {
                Expression<Func<User, bool>> keywordsExpr = PredicateBuilder.False<User>();

                foreach (string keyword in keywords.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    // Check for noise
                    if (keyword.Length <= 1) continue;

                    //TODO: Check for noise words from list

                    string key = keyword;
                    keywordsExpr = keywordsExpr.Or(p => p.FirstName.Contains(key) ||
                        p.LastName.Contains(key) || p.Company.Contains(key) || p.Email.Contains(key));
                }

                whereExpr = whereExpr.And(keywordsExpr.Expand());
            }

            #endregion

            #region Other filters

            #endregion

            // Apply filters
            users = users.Where(whereExpr.Expand());

            // Apply sorting
            users = users.OrderByMember(orderExpr ?? (c => c.Id),
                orderAsc ? SortOrder.Ascending : SortOrder.Descending);

            return users;
        }

        public Address GetAddress(string userId, AddressType type)
        {
            var address = (from a in db.Addresses
                where a.UserId == userId && a.Type == type
                orderby a.IsPrimary descending, a.Id descending 
                select a).FirstOrDefault();

            // Return billing address if no shipping one is found
            if (address == null && type == AddressType.Shipping)
            {
                return GetAddress(userId, AddressType.Billing);
            }

            return address;
        }

        public Expression<Func<User, object>> CreateOrderExpr(string orderColumn)
        {
            if (orderColumn == null) return null;

            switch (orderColumn)
            {
                case "name":
                    return c => c.FirstName + c.LastName;
                case "email":
                    return c => c.Email;
                case "phone":
                    return c => c.PhoneNumber;
                case "dateRegistered":
                    return c => c.DateRegistered;
                case "ordersCount":
                    return c => c.Orders.Count;
                default:
                    return null;
            }
        }

        public User AddOrUpdate(CustomerViewModel model)
        {
            User user;
            if (!string.IsNullOrEmpty(model.Id))
            {
                user = Find(model.Id);
                user.Email = model.Email;
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    var passwordHash = userManager.PasswordHasher.HashPassword(model.NewPassword);
                    user.PasswordHash = passwordHash;
                }
            }
            else
            {
                user = Mapper.Map<User>(model);
            }

            if (model.BillingAddress != null)
            {
                Address billingAddress;
                if (model.BillingAddress.Id == 0)
                {
                    billingAddress = Mapper.Map<Address>(model.BillingAddress);
                    user.Addresses.Add(billingAddress);
                }
                else
                {
                    billingAddress = db.Addresses.Find(model.BillingAddress.Id);
                    billingAddress = Mapper.Map(model.BillingAddress, billingAddress);
                }
                billingAddress.IsPrimary = true;
                billingAddress.Type = AddressType.Billing;

                // Copy basic details from billing address to the user entity
                if (!string.IsNullOrEmpty(billingAddress.FirstName))
                    user.FirstName = billingAddress.FirstName;
                if (!string.IsNullOrEmpty(billingAddress.LastName))
                    user.LastName = billingAddress.LastName;
                if (!string.IsNullOrEmpty(billingAddress.Company))
                    user.Company = billingAddress.Company;
                if (!string.IsNullOrEmpty(billingAddress.Phone))
                    user.PhoneNumber = billingAddress.Phone;
            }

            if (model.ShippingAddress != null)
            {
                if (model.ShippingAddress == model.BillingAddress)
                    model.ShippingAddress.Id = 0;

                Address shippingAddress;
                if (model.ShippingAddress.Id == 0)
                {
                    shippingAddress = Mapper.Map<Address>(model.ShippingAddress);
                    user.Addresses.Add(shippingAddress);
                }
                else
                {
                    shippingAddress = db.Addresses.Find(model.ShippingAddress.Id);
                    shippingAddress = Mapper.Map(model.ShippingAddress, shippingAddress);
                }
                shippingAddress.IsPrimary = true;
                shippingAddress.Type = AddressType.Shipping;
            }

            if (string.IsNullOrEmpty(model.Id))
            {
                // Generate random password for new user if one is not specified
                if (string.IsNullOrEmpty(model.NewPassword))
                    model.NewPassword = Guid.NewGuid().ToString();

                IdentityResult result = userManager.Create(user, model.NewPassword);
                if (result.Succeeded)
                {
                    userManager.AddToRole(user.Id, User.CUSTOMER_ROLE);
                }

                foreach (string error in result.Errors)
                {
                    throw new ArgumentException(error);
                }
            }

            db.SaveChanges();

            return user;
        }

        public void LoginUser(HttpContextBase context, User user)
        {
            IAuthenticationManager authManager = context.GetOwinContext().Authentication;
            authManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            authManager.SignIn(new AuthenticationProperties { IsPersistent = true },
                userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie));
        }

        public void Delete(string id)
        {
            User customer = Find(id);
            var orders = customer.Orders.ToList();
            foreach (var order in orders)
            {
                deleteService.DeleteOrder(order.Id);
            }
            var addresses = customer.Addresses.ToList();
            foreach (var address in addresses)
            {
                db.Addresses.Remove(address);
            }
            var blogComments = customer.BlogPostComments.ToList();
            foreach (var blogComment in blogComments)
            {
                deleteService.DeleteBlogPostComment(blogComment.Id);
            }
            db.SaveChanges();

            userManager.RemoveFromRoles(id, userManager.GetRoles(id).ToArray());
            userManager.Delete(customer);
        }
    }
}