using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Models
{
    public class NewsletterSubscriptionViewModel : IHaveCustomMappings
    {
        public string Email { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<NewsletterSubscription, NewsletterSubscriptionViewModel>();
            Mapper.CreateMap<NewsletterSubscriptionViewModel, NewsletterSubscription>();
        }
    }
}