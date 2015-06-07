using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Estream.Cart42.Web.Domain
{
    public class NewsletterSubscription
    {
        [Key]
        public int Id { get; set; }

        public string Email { get; set; }

        public bool IsActive { get; set; }

        public DateTime DateSubscribed { get; set; }
    }
}