using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface IPaymentMethodService
    {
        IQueryable<PaymentMethod> FindAll();
        PaymentMethod Find(int id);
        PaymentMethod Update(PaymentMethodViewModel model);
    }
}