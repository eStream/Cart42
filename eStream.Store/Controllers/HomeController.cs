using System.Web.Mvc;

namespace Estream.Cart42.Web.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}