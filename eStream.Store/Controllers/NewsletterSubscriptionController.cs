using System.Web.Mvc;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Models;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Controllers
{
    public class NewsletterSubscriptionController : BaseController
    {
        private readonly INewsletterSubscriptionService newsletterSubscriptionService;

        public NewsletterSubscriptionController(INewsletterSubscriptionService newsletterSubscriptionService)
        {
            this.newsletterSubscriptionService = newsletterSubscriptionService;
        }

        public PartialViewResult AddSubscriptionPartial()
        {
            var model = new NewsletterSubscriptionViewModel();

            return PartialView("_AddSubscription", model);
        }

        [HttpPost]
        public ActionResult AddSubscription(NewsletterSubscriptionViewModel model)
        {
            newsletterSubscriptionService.Add(model);

            var action = RedirectToAction("Index", "Home");
            return action.WithSuccess(string.Format("You have subscribed successfully".TA()));
        }
    }
}