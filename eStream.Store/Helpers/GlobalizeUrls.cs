using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Estream.Cart42.Web.Helpers
{
    public static class GlobalizeUrls
    {
        public static string Globalize { get { return "~/Scripts/globalize.js"; } }

        public static string GlobalizeCulture
        {
            get
            {
                var currentCulture = CultureInfo.CurrentCulture;
                const string filePattern = "~/scripts/globalize/globalize.culture.{0}.js";
                var regionalisedFileToUse = string.Format(filePattern, "en-US");

                if (File.Exists(HostingEnvironment.MapPath(string.Format(filePattern, currentCulture.Name))))
                    regionalisedFileToUse = string.Format(filePattern, currentCulture.Name);
                else if (File.Exists(HostingEnvironment.MapPath(string.Format(filePattern, currentCulture.TwoLetterISOLanguageName))))
                    regionalisedFileToUse = string.Format(filePattern, currentCulture.TwoLetterISOLanguageName);

                return regionalisedFileToUse;
            }
        }
    }
}