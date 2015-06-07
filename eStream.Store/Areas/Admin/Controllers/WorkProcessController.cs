using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class WorkProcessController : BaseController
    {
        private readonly IWorkProcessService workProcessService;

        public WorkProcessController(IWorkProcessService workProcessService)
        {
            this.workProcessService = workProcessService;
        }

        // GET: Admin/WorkProcess
        public PartialViewResult Index(WorkProcessType type)
        {
            return PartialView("_Index", type);
        }

        public JsonResult Progress(WorkProcessType type)
        {
            var jobs = workProcessService.Find(type).Where(p => p.IsRunning).ToList();

            return JsonSuccess(jobs);
        }
    }
}