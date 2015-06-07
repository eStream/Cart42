using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Estream.Cart42.Web.DependencyResolution.PaymentMethods;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;

namespace Estream.Cart42.Web.Integrations.Payments
{
    public class EPayBgButton : IPaymentMethod, IHostedPaymentMethod, IPaymentMethodIpn
    {
        private readonly PaymentHelper paymentHelper;
        private EPayBgButtonSettings settings = new EPayBgButtonSettings();

        public EPayBgButton(PaymentHelper paymentHelper)
        {
            this.paymentHelper = paymentHelper;
        }

        public string SuccessUrl { set; private get; }
        public string CancelUrl { set; private get; }
        public string IpnUrl { set; private get; }

        public IPaymentMethodSettings Settings
        {
            set { settings = (EPayBgButtonSettings) value; }
            get { return settings; }
        }

        public ActionResult Process(Order order)
        {
            string description = string.Format("Order {0}", order.Id);
            string expStr = DateTime.Now.AddDays(1).ToString("dd.MM.yyyy");

            string data = string.Format(
                @"MIN={0}
INVOICE={1}
AMOUNT={2}
EXP_TIME={3}
DESCR={4}
ENCODING=utf-8",
                settings.Min, order.Id, order.Total, expStr, description);

            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));

            string checksum = calculateHmac(encoded);

            string url = settings.TestMode ? "https://devep2.datamax.bg/ep2/epay2_demo/" : "https://www.epay.bg/";

            string form = string.Format(@"
<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">
<html>
<head>
<title>ePay.bg</title>
</head>
<body onload=""PayForm.submit();"">
<form action=""{0}"" method=post name=""PayForm"">
 <input type=hidden name=PAGE value=""{5}"">
 <input type=hidden name=ENCODED value=""{1}"">
 <input type=hidden name=CHECKSUM value=""{2}"">
 <input type=hidden name=URL_OK value=""{3}"">
 <input type=hidden name=URL_CANCEL value=""{4}"">
 <input type=submit>
 </form>",
                url, encoded, checksum, SuccessUrl, CancelUrl,
                settings.DirectPayment ? "credit_paydirect" : "paylogin");

            return new ContentResult {Content = form};
        }

        public IpnResult Ipn(HttpRequestBase request)
        {
            var result = new IpnResult();
            string encoded = request.Params["encoded"];
            string checksum = request.Params["checksum"];

            if (checksum == calculateHmac(encoded))
            {
                string response = "";

                string data = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                foreach (string line in data.Split('\n'))
                {
                    Match match = Regex.Match(line,
                        @"/^INVOICE=(\d+):STATUS=(PAID|DENIED|EXPIRED)(:PAY_TIME=(\d+):STAN=(\d+):BCODE=([0-9a-zA-Z]+))?$/");

                    if (match.Success)
                    {
                        string invoice = match.Groups[1].Value;
                        string status = match.Groups[2].Value;

                        paymentHelper.LogPayment(Convert.ToInt32(invoice), this.GetType().Name,
                            status == "PAID" ? PaymentStatus.Completed : PaymentStatus.Failed,
                            null, line);

                        response += "INVOICE=" + invoice + ":STATUS=OK\n";
                    }
                }

                result.Response = response;
            }
            else
            {
                result.Response = "ERR=Not valid CHECKSUM\n";
            }

            return result;
        }

        private string calculateHmac(string encoded)
        {
            var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(settings.Secret));
            hmac.Initialize();
            byte[] buffer = Encoding.ASCII.GetBytes(encoded);
            string checksum = BitConverter.ToString(hmac.ComputeHash(buffer)).Replace("-", "").ToLower();
            return checksum;
        }
    }

    public class EPayBgButtonSettings : IPaymentMethodSettings
    {
        private string min;
        private string secret;
        private bool testMode = true;

        public string Min
        {
            get { return min ?? "D345670335"; }
            set { min = value; }
        }

        public string Secret
        {
            get { return secret ?? "279GPILU04O5MABTRN4YFOWC18Z6PPUJJA2WMK26KTD3LDFFMHNIN1TRQ2XQZ7IH"; }
            set { secret = value; }
        }

        public bool TestMode
        {
            get { return testMode; }
            set { testMode = value; }
        }

        public bool DirectPayment { get; set; }
    }
}