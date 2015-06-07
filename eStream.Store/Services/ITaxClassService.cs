using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface ITaxClassService
    {
        IQueryable<TaxClass> FindAll();
        TaxClass AddOrUpdate(TaxClassViewModel model);
        TaxClass Find(int id);
        void Delete(int id);
    }
}