using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.DependencyResolution.PaymentMethods
{
    public interface IPaymentMethod
    {
        IPaymentMethodSettings Settings { set; }

        ActionResult Process(Order order);
    }
}