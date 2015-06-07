using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Estream.Cart42.Web.DependencyResolution.Filters;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminAlertController : Controller
    {
        // GET: Admin/AdminAlert
        public PartialViewResult Index()
        {
            return PartialView();
        }
    }
}