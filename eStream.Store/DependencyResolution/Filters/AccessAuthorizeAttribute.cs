using System.Net;
using System.Web.Mvc;

namespace Estream.Cart42.Web.DependencyResolution.Filters
{
    public class AccessAuthorizeAttribute : AuthorizeAttribute
    {
        public AccessAuthorizeAttribute(string role)
        {
            Roles = Domain.User.ADMIN_ROLE + "," + role;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }
    }
}