using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Estream.Cart42.Web.Services;
using Microsoft.AspNet.Identity;

namespace Estream.Cart42.Web.Controllers
{
    public class VisitorController : BaseController
    {
        private readonly IVisitorService visitorService;

        public VisitorController(IVisitorService visitorService)
        {
            this.visitorService = visitorService;
        }

        public EmptyResult Track()
        {
            string userId = null;
            if (User.Identity.IsAuthenticated)
                userId = User.Identity.GetUserId();
            var ip = Request.UserHostAddress;
            
            Guid? id = null;

            if (Request.Cookies["VisitorId"] != null)
            {
                Guid cookieVal;
                if (Guid.TryParse(Request.Cookies["VisitorId"].Value, out cookieVal))
                {
                    id = cookieVal;
                }
            }

            id = visitorService.TrackVisitor(id, userId, ip);

            Response.SetCookie(new HttpCookie("VisitorId", id.ToString())
                               {
                                   Expires = DateTime.Now.AddDays(30)
                               });

            return new EmptyResult();
        }
    }
}