using System;
using System.Collections.Generic;
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
    public class OperatorService : IOperatorService
    {
        private readonly DataContext db;
        private static string _operatorRoleId;
        private static readonly object padlock = new object();
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public OperatorService(DataContext db, IUserStore<User> userStore, IRoleStore<IdentityRole, string> roleStore)
        {
            this.db = db;
            userManager = new UserManager<User>(userStore);
            roleManager = new RoleManager<IdentityRole>(roleStore);

            if (_operatorRoleId == null)
            {
                lock (padlock)
                {
                    if (_operatorRoleId == null)
                    {
                        // ReSharper disable PossibleMultipleWriteAccessInDoubleCheckLocking
                        _operatorRoleId = roleManager.FindByName(User.OPERATOR_ROLE).Id;
                        // ReSharper restore PossibleMultipleWriteAccessInDoubleCheckLocking
                    }
                }
            }
        }

        public IQueryable<User> FindAll()
        {
            return db.Users.Where(u => u.Roles.Any(r => r.RoleId == _operatorRoleId));
        }

        public User Find(string id)
        {
            return FindAll().FirstOrDefault(u => u.Id == id);
        }

        public User AddOrUpdate(OperatorViewModel model)
        {
            User user;

            if (!string.IsNullOrEmpty(model.Id))
            {
                user = Find(model.Id);
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
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

            if (string.IsNullOrEmpty(model.Id))
            {
                // Generate random password for new user if one is not specified
                if (string.IsNullOrEmpty(model.NewPassword))
                    model.NewPassword = Guid.NewGuid().ToString();

                IdentityResult result = userManager.Create(user, model.NewPassword);

                foreach (string error in result.Errors)
                {
                    throw new ArgumentException(error);
                }
            }

            if (model.Roles != null)
            {
                // Workaround bug in identity
                userManager.RemoveFromRoles(user.Id, userManager.GetRoles(user.Id).ToArray());

                // Add missing roles
                foreach (var role in model.Roles.Where(r => r.Contains("_")).ToList())
                {
                    var baseRole = role.Remove(role.IndexOf('_'));
                    if (!model.Roles.Contains(baseRole))
                        model.Roles.Add(baseRole);
                }

                userManager.AddToRole(user.Id, User.OPERATOR_ROLE);

                foreach (var role in model.Roles)
                {
                    if (!roleManager.RoleExists(role))
                        roleManager.Create(new IdentityRole(role));

                    if (!userManager.IsInRole(user.Id, role))
                        userManager.AddToRole(user.Id, role);
                }
            }

            db.SaveChanges();

            return user;
        }

        public List<string> GetRoles(string userId)
        {
            return (List<string>) userManager.GetRoles(userId);
        }

        public void Delete(string id)
        {
            var user = Find(id);
            userManager.RemoveFromRoles(user.Id, userManager.GetRoles(user.Id).ToArray());
            userManager.Delete(user);
        }
    }
}