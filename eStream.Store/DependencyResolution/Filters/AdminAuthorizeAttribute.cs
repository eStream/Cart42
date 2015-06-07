using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Estream.Cart42.Web.DependencyResolution.Filters
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        public AdminAuthorizeAttribute()
        {
            Roles = Domain.User.ADMIN_ROLE + "," + Domain.User.OPERATOR_ROLE;
        }
    }
}