using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Models;

namespace Estream.Cart42.Web.Services
{
    public class NewsletterSubscriptionService : INewsletterSubscriptionService
    {
        private readonly DataContext db;

        public NewsletterSubscriptionService(DataContext db)
        {
            this.db = db;
        }

        public IQueryable<NewsletterSubscription> FindAll()
        {
            return db.NewsletterSubscriptions;
        }

        public NewsletterSubscription Add(NewsletterSubscriptionViewModel model)
        {
            NewsletterSubscription subscription = db.NewsletterSubscriptions.FirstOrDefault(a => a.Email == model.Email);
            if (subscription == null)
            {
                subscription = Mapper.Map<NewsletterSubscription>(model);
                subscription.DateSubscribed = DateTime.Now;
                db.NewsletterSubscriptions.Add(subscription);                 
            }            
            db.SaveChanges();
            return subscription;
        }
    }
}