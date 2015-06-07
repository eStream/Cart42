using System.Web;
using System.Web.Mvc;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Integrations.Payments
{
    public interface IHostedPaymentMethod
    {
        string SuccessUrl { set; }

        string CancelUrl { set; }
    }

    public interface IPaymentMethodIpn
    {
        string IpnUrl { set; }

        IpnResult Ipn(HttpRequestBase request);
    }

    public class IpnResult
    {
        public string Response { get; set; }
    }

    public enum IpnResultType
    {
        Paid = 1,
        Failed = 2,
        ManualReview = 3,
        Refunded = 4
        // Chargeback...
    }
}