using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Models;

namespace Estream.Cart42.Web.Services
{
    public interface INewsletterSubscriptionService
    {
        IQueryable<NewsletterSubscription> FindAll();
        NewsletterSubscription Add(NewsletterSubscriptionViewModel model);
    }
}