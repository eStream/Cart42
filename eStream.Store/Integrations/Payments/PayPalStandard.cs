using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Elmah;
using Estream.Cart42.Web.DependencyResolution.PaymentMethods;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Integrations.Payments
{
    public class PayPalStandard : IPaymentMethod, IHostedPaymentMethod, IPaymentMethodIpn
    {
        private readonly ISettingService settingService;
        private readonly PaymentHelper paymentHelper;
        private PayPalStandardSettings settings = new PayPalStandardSettings();

        public string SuccessUrl { set; private get; }

        public string CancelUrl { set; private get; }

        public IPaymentMethodSettings Settings
        {
            set { settings = (PayPalStandardSettings) value; }
            get { return settings; }
        }

        public PayPalStandard(ISettingService settingService, PaymentHelper paymentHelper)
        {
            this.settingService = settingService;
            this.paymentHelper = paymentHelper;
        }

        public ActionResult Process(Order order)
        {
            string postUrl = settings.TestMode
                ? "https://www.sandbox.paypal.com/cgi-bin/webscr"
                : "https://www.paypal.com/cgi-bin/webscr";
            /*
            var form = string.Format(@"
<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">
<html>
<head>
<title>Payment</title>
</head>
<body onload=""PayForm.submit();"">
<form action=""{0}"" method=post name=""PayForm"">
<input type=""hidden"" name=""cmd"" value=""_xclick"">
<input type=""hidden"" name=""business"" value=""{1}"">
<input type=""hidden"" name=""currency_code"" value=""{2}"">
<input type=""hidden"" name=""custom"" value=""{3}"">
<input type=""hidden"" name=""item_name"" value=""{4}"">
<input type=""hidden"" name=""item_number"" value=""{5}"">
<input type=""hidden"" name=""amount"" value=""{6}"">
<input type=""hidden"" name=""first_name"" value=""{7}"">
<input type=""hidden"" name=""last_name"" value=""{8}"">
<input type=""hidden"" name=""address1"" value=""{9}"">
<input type=""hidden"" name=""address2"" value=""{10}"">
<input type=""hidden"" name=""city"" value=""{11}"">
<input type=""hidden"" name=""state"" value=""{12}"">
<input type=""hidden"" name=""zip"" value=""{13}"">
<input type=""hidden"" name=""country"" value=""{14}"">
<input type=""hidden"" name=""charset"" value=""utf-8"">
<input type=""hidden"" name=""callback_url"" value=""{15}"">
<input type=""image"" name=""submit"" border=""0""
src=""https://www.paypalobjects.com/en_US/i/btn/btn_buynow_LG.gif""
alt=""PayPal - The safer, easier way to pay online"">
</form>", postUrl, settings.Email, SettingsHelper.GetSetting<string>(Helpers.Settings.CurrencyCode),
        order.Id, string.Format("Order {0}".T(), order.Id), order.Id, order.Total,
        order.BillingAddress.FirstName, order.BillingAddress.LastName, order.BillingAddress.Address1,
        order.BillingAddress.Address2, order.BillingAddress.City, 
        order.BillingAddress.RegionId.HasValue ? order.BillingAddress.Region.Name : order.BillingAddress.RegionOther,
        order.BillingAddress.ZipPostal, order.BillingAddress.CountryCode, IpnUrl);
             */

            string itemsHtml = "";
            int itemNo = 1;
            foreach (OrderItem item in order.Items)
            {
                itemsHtml += string.Format(@"
<input type=""hidden"" name=""quantity_{0}"" value=""{1}"">
<input type=""hidden"" name=""item_name_{0}"" value=""{2}"">
<input type=""hidden"" name=""item_number_{0}"" value=""{3}"">
<input type=""hidden"" name=""amount_{0}"" value=""{4}"">", itemNo++, item.Quantity, item.Product.Name,
                    item.Product.Sku, item.Quantity*item.ItemPrice);
            }

            string taxHtml = "";
            if (!settingService.Get<bool>(global::SettingField.TaxIncludedInPrices))
                taxHtml += string.Format(@"<input type=""hidden"" name=""tax_cart"" value=""{0}"">", order.TaxAmount);

            var currencyCode = settingService.Get<string>(SettingField.CurrencyCode);
            string form = string.Format(@"
<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">
<html>
<head>
<title>Payment</title>
</head>
<body onload=""PayForm.submit();"">
<form action=""{0}"" method=post name=""PayForm"">
<input type=""hidden"" name=""cmd"" value=""_cart"">
<input type=""hidden"" name=""upload"" value=""1"">
<input type=""hidden"" name=""business"" value=""{1}"">
<input type=""hidden"" name=""currency_code"" value=""{2}"">
<input type=""hidden"" name=""custom"" value=""{3}"">
{4}
{5}
<input type=""hidden"" name=""handling_cart"" value=""{6}"">
<input type=""hidden"" name=""first_name"" value=""{7}"">
<input type=""hidden"" name=""last_name"" value=""{8}"">
<input type=""hidden"" name=""address1"" value=""{9}"">
<input type=""hidden"" name=""address2"" value=""{10}"">
<input type=""hidden"" name=""city"" value=""{11}"">
<input type=""hidden"" name=""state"" value=""{12}"">
<input type=""hidden"" name=""zip"" value=""{13}"">
<input type=""hidden"" name=""country"" value=""{14}"">
<input type=""hidden"" name=""charset"" value=""utf-8"">
<input type=""hidden"" name=""notify_url"" value=""{15}"">
<input type=""image"" name=""submit"" border=""0"" style=""display: none""
src=""https://www.paypalobjects.com/en_US/i/btn/btn_buynow_LG.gif""
alt=""PayPal - The safer, easier way to pay online"">
Redirecting to PayPal...
</form>", postUrl, settings.Email, currencyCode,
                order.Id, itemsHtml, taxHtml, order.ShippingAmount,
                order.BillingAddress.FirstName, order.BillingAddress.LastName, order.BillingAddress.Address1,
                order.BillingAddress.Address2, order.BillingAddress.City,
                order.BillingAddress.RegionId.HasValue
                    ? order.BillingAddress.Region.Name
                    : order.BillingAddress.RegionOther,
                order.BillingAddress.ZipPostal, order.BillingAddress.CountryCode, IpnUrl);

            return new ContentResult {Content = form};
        }

        public string IpnUrl { set; private get; }

        public IpnResult Ipn(HttpRequestBase request)
        {
            var result = new IpnResult();

            string postUrl = settings.TestMode
                ? "https://www.sandbox.paypal.com/cgi-bin/webscr"
                : "https://www.paypal.com/cgi-bin/webscr";

            //Post back to either sandbox or live
            var req = (HttpWebRequest) WebRequest.Create(postUrl);

            //Set values for the request back
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            byte[] param = request.BinaryRead(request.ContentLength);
            string strRequest = Encoding.ASCII.GetString(param);
            string strResponse_copy = strRequest; //Save a copy of the initial info sent by PayPal
            strRequest += "&cmd=_notify-validate";
            req.ContentLength = strRequest.Length;

            //for proxy
            //WebProxy proxy = new WebProxy(new Uri("http://url:port#"));
            //req.Proxy = proxy;
            //Send the request to PayPal and get the response
            var streamOut = new StreamWriter(req.GetRequestStream(), Encoding.ASCII);
            streamOut.Write(strRequest);
            streamOut.Close();
            var streamIn = new StreamReader(req.GetResponse().GetResponseStream());
            string strResponse = streamIn.ReadToEnd();
            streamIn.Close();

            if (strResponse == "VERIFIED")
            {
                //check the payment_status is Completed
                //check that txn_id has not been previously processed
                //check that receiver_email is your Primary PayPal email
                //check that payment_amount/payment_currency are correct
                //process payment

                // pull the values passed on the initial message from PayPal

                NameValueCollection args = HttpUtility.ParseQueryString(strResponse_copy);
                var orderId = Convert.ToInt32(args["custom"]);
                var amount = Convert.ToDecimal(args["mc_gross"]);
                PaymentStatus status;
                string paymentNotes;
                if (args["receiver_email"] != settings.Email)
                {
                    paymentNotes = string.Format("PayPal recipient expected was {0} but received {1}",
                        settings.Email, args["receiver_email"]);
                    status = PaymentStatus.ManualReview;
                }
                else if (args["test_ipn"] == "1" && !settings.TestMode)
                {
                    paymentNotes = string.Format("PayPal IPN was in test mode but site is not");
                    status = PaymentStatus.Failed;
                }
                else if (args["mc_currency"] != settingService.Get<string>(SettingField.CurrencyCode))
                {
                    paymentNotes = string.Format("Expected currency {0} but received {1}",
                        settingService.Get<string>(SettingField.CurrencyCode), args["mc_currency"]);
                    status = PaymentStatus.Failed;
                }
                else
                {
                    paymentNotes = strResponse_copy;
                    switch (args["payment_status"])
                    {
                        case "Canceled_Reversal":
                        case "Completed":
                        case "Processed":
                            status = PaymentStatus.Completed;
                            break;
                        case "Denied":
                        case "Voided":
                        case "Expired":
                        case "Failed":
                            status = PaymentStatus.Failed;
                            break;
                        case "Refunded":
                        case "Reversed":
                            status = PaymentStatus.Refunded;
                            break;
                        case "Pending":
                            status = PaymentStatus.ManualReview;
                            break;
                        default:
                            status = PaymentStatus.Failed;
                            break;
                    }
                }

                paymentHelper.LogPayment(orderId, GetType().Name, status, amount, paymentNotes);
            }
            else if (strResponse == "INVALID")
            {
                //log for manual investigation
                ErrorLog.GetDefault(HttpContext.Current).Log(
                    new Error(new Exception("PayPal INVALID: " + strResponse_copy)));
            }
            else
            {
                //log response/ipn data for manual investigation
                ErrorLog.GetDefault(HttpContext.Current).Log(
                    new Error(new Exception("PayPal " + strResponse + ": " + strResponse_copy)));
            }

            return result;
        }
    }

    public class PayPalStandardSettings : IPaymentMethodSettings
    {
        private string email = "emil-facilitator@estreambg.com";
        private bool testMode = true;

        public bool TestMode
        {
            get { return testMode; }
            set { testMode = value; }
        }

        public string Email
        {
            get { return email; }
            set { email = value; }
        }
    }
}