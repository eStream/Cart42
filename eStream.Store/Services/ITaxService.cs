using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Estream.Cart42.Web.Services
{
    public interface ITaxService
    {
        decimal CalculateTax(string countryCode, int? regionId, int? taxClassId, decimal price);
    }
}