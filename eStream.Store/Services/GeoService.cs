using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Glimpse.AspNet.Tab;
using MaxMind.Db;

namespace Estream.Cart42.Web.Services
{
    public class GeoService : IGeoService
    {
        readonly Reader reader;

        public GeoService()
        {
            // TODO: Make it centralized service
            reader = new Reader(HostingEnvironment.MapPath("~/Content/GeoIp/GeoLite2-Country.mmdb"));
        }

        public string FindCountryCodeByIp(string ip)
        {
            dynamic result = reader.Find(ip);
            if (result != null)
                return result.country.iso_code;
            return null;
        }
    }
}