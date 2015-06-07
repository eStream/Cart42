using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Estream.Cart42.Web.Helpers
{
    public class StandardJsonResult : JsonResult
    {
        public StandardJsonResult()
        {
            ErrorMessages = new List<string>();
        }

        public IList<string> ErrorMessages { get; private set; }

        public void AddError(string errorMessage)
        {
            ErrorMessages.Add(errorMessage);
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (JsonRequestBehavior == JsonRequestBehavior.DenyGet &&
                string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "GET access is not allowed.  Change the JsonRequestBehavior if you need GET access.");
            }

            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = string.IsNullOrEmpty(ContentType) ? "application/json" : ContentType;

            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }

            SerializeData(response);
        }

        protected virtual void SerializeData(HttpResponseBase response)
        {
            if (ErrorMessages.Any())
            {
                object originalData = Data;
                Data = new
                       {
                           Success = false,
                           OriginalData = originalData,
                           ErrorMessage = string.Join("\n", ErrorMessages),
                           ErrorMessages = ErrorMessages.ToArray()
                       };

                response.StatusCode = 400;
                response.TrySkipIisCustomErrors = true;
            }

            var settings = new JsonSerializerSettings
                           {
                               ContractResolver = new CamelCasePropertyNamesContractResolver(),
                               Converters = new JsonConverter[]
                                            {
                                                new StringEnumConverter()
                                            },
                           };

            response.Write(JsonConvert.SerializeObject(Data, settings));
        }
    }

    public class StandardJsonResult<T> : StandardJsonResult
    {
        public new T Data
        {
            get { return (T) base.Data; }
            set { base.Data = value; }
        }
    }

    public static class JavaScriptHelper
    {
        public static IHtmlString Json(this System.Web.Mvc.HtmlHelper helper, object obj)
        {
            var settings = new JsonSerializerSettings
                           {
                               ContractResolver = new CamelCasePropertyNamesContractResolver(),
                               Converters = new JsonConverter[]
                                            {
                                                new StringEnumConverter(),
                                                new JavaScriptDateTimeConverter(), 
                                            },
                               StringEscapeHandling = StringEscapeHandling.EscapeHtml
                           };

            return MvcHtmlString.Create(JsonConvert.SerializeObject(obj, settings));
        }

        //Adapted from JSON.NET.
        public static string CamelCaseIdForModel(this System.Web.Mvc.HtmlHelper helper)
        {
            string input = helper.IdForModel().ToString();

            if (string.IsNullOrEmpty(input) || !char.IsUpper(input[0]))
            {
                return input;
            }

            var sb = new StringBuilder();

            for (int i = 0; i < input.Length; ++i)
            {
                bool flag = i + 1 < input.Length;
                if (i == 0 || !flag || char.IsUpper(input[i + 1]))
                {
                    char ch = char.ToLower(input[i], CultureInfo.InvariantCulture);
                    sb.Append(ch);
                }
                else
                {
                    sb.Append(input.Substring(i));
                    break;
                }
            }

            return sb.ToString();
        }
    }
}