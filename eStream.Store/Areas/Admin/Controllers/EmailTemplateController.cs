using System.Linq;
using System.Net;
using System.Web.Mvc;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class EmailTemplateController : BaseController
    {
        // GET: /Admin/EmailTemplate/
        public EmailTemplateController(DataContext db) : base(db)
        {
        }

        [AccessAuthorize(OperatorRoles.TEMPLATES)]
        public ActionResult Index()
        {
            return View(db.EmailTemplates.ToList());
        }

        // GET: /Admin/EmailTemplate/Edit/5
        [AccessAuthorize(OperatorRoles.TEMPLATES + OperatorRoles.WRITE)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmailTemplate emailtemplate = db.EmailTemplates.Find(id);
            if (emailtemplate == null)
            {
                return HttpNotFound();
            }
            return View(emailtemplate);
        }

        // POST: /Admin/EmailTemplate/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.TEMPLATES + OperatorRoles.WRITE)]
        public ActionResult Edit([Bind(Include = "Id,Subject,Body")] EmailTemplate model)
        {
            if (ModelState.IsValid)
            {
                EmailTemplate emailtemp = db.EmailTemplates.Single(e => e.Id == model.Id);
                emailtemp.Subject = model.Subject;
                emailtemp.Body = model.Body;
                db.SaveChanges();
                return RedirectToAction("Index")
                    .WithSuccess(string.Format("Template \"{0}\" has been updated".TA(), emailtemp.Type.DisplayName()));
            }
            return View(model);
        }
    }
}