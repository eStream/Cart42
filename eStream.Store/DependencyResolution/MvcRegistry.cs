using System.Security.Principal;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using StructureMap.Configuration.DSL;

namespace Estream.Cart42.Web.DependencyResolution
{
    public class MvcRegistry : Registry
    {
        public MvcRegistry()
        {
            For<BundleCollection>().Use(BundleTable.Bundles);
            For<RouteCollection>().Use(RouteTable.Routes);
            For<HttpContextBase>().Use(() => new HttpContextWrapper(HttpContext.Current));
            For<HttpServerUtilityBase>().Use(() => new HttpServerUtilityWrapper(HttpContext.Current.Server));
            For<HttpRequestBase>().Use(() => new HttpRequestWrapper(HttpContext.Current.Request));
            For<IIdentity>().Use(() => HttpContext.Current.User.Identity);
            For<IUserStore<User>>().Use<UserStore<User>>();
            For<IRoleStore<IdentityRole, string>>().Use<RoleStore<IdentityRole, string, IdentityUserRole>>();
            For<DataContext>().Use(() => DataContext.Current);
            For<System.Data.Entity.DbContext>().Use(() => DataContext.Current);
        }
    }
}