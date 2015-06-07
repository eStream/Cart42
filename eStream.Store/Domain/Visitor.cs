using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Estream.Cart42.Web.Domain
{
    public class Visitor
    {
        [Key]
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public DateTime FirstVisitDate { get; set; }

        public DateTime LastVisitDate { get; set; }

        public int Visits { get; set; }

        public string IpAddress { get; set; }
    }
}