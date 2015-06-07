using System.Linq;
using System.Web.Mvc;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Areas.Admin.Views
{
    public class BaseViewPage<TModel> : WebViewPage<TModel>
    {
        private readonly ISettingService _settingService;

        public ISettingService Settings
        {
            get { return _settingService; }
        }

        public BaseViewPage()
        {
            _settingService = DependencyResolver.Current.GetService<ISettingService>();
        }

        public BaseViewPage(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public override void Execute()
        {
            // Do nothing
        }

        public bool IsAny(params string[] controllers)
        {
            return controllers.Any(c => Is(c));
        }

        public bool Is(string controller, string action = null)
        {
            if (ViewContext.RouteData.Values["Controller"].ToString().ToLower() == controller.ToLower()
                && (action == null ||
                 ViewContext.RouteData.Values["Action"].ToString().ToLower() == action.ToLower()))
                return true;
            return false;
        }
    }
}