using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Estream.Cart42.Web.Services
{
    public interface IGeoService
    {
        string FindCountryCodeByIp(string ip);
    }
}