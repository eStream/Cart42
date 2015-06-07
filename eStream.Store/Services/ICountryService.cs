using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface ICountryService
    {
        IQueryable<Country> FindAll();
        Country Find(string code);
        Country AddOrUpdate(CountryViewModel model);
        void Delete(string code);
        void SetActive(string code, bool active);
    }
}