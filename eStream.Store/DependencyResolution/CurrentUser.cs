using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Microsoft.AspNet.Identity;

namespace Estream.Cart42.Web.DependencyResolution
{
    public interface ICurrentUser
    {
        User User { get; }
    }

    public class CurrentUser : ICurrentUser
    {
        private readonly IIdentity identity;
        private readonly DataContext db;
        private User user;

        public CurrentUser(IIdentity identity, DataContext db)
        {
            this.identity = identity;
            this.db = db;
        }

        public User User
        {
            get { return user ?? (user = db.Users.Find(identity.GetUserId())); }
        }
    }
}