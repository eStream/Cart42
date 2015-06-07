using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public class VisitorService : IVisitorService
    {
        private readonly DataContext db;

        public VisitorService(DataContext db)
        {
            this.db = db;
        }

        public Guid TrackVisitor(Guid? id, string userId, string ipAddress)
        {
            Visitor visitor = null;

            // Find visit by id from cookie
            if (id.HasValue)
            {
                visitor = db.Visitors.FirstOrDefault(v => v.Id == id.Value);
            }

            // Find recent visit by ip
            if (visitor == null)
            {
                var cutoffDate = DateTime.Now.AddHours(-2);
                visitor = (from v in db.Visitors
                    where v.IpAddress == ipAddress
                          && v.LastVisitDate >= cutoffDate
                    orderby v.LastVisitDate descending
                    select v).FirstOrDefault();
            }

            if (visitor == null)
            {
                visitor = new Visitor
                          {
                              Id = Guid.NewGuid(),
                              FirstVisitDate = DateTime.Now,
                              LastVisitDate = DateTime.Now,
                              Visits = 1
                          };
                db.Visitors.Add(visitor);
            }

            visitor.LastVisitDate = DateTime.Now;
            visitor.IpAddress = ipAddress;
            visitor.UserId = userId;

            if (visitor.LastVisitDate < DateTime.Now.AddHours(1))
                visitor.Visits = visitor.Visits + 1;

            db.SaveChanges();

            return visitor.Id;
        }
    }
}