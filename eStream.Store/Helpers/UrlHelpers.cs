using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Glimpse.AspNet.Tab;

namespace Estream.Cart42.Web.Helpers
{
    public static class UrlHelpers
    {
        public static string AddParam(this UrlHelper urlHelper, string name, string value)
        {
            string url = urlHelper.RequestContext.HttpContext.Request.RawUrl;
            url += url.Contains("?") ? "&" : "?";
            url += name + "=" + HttpUtility.UrlEncode(value);

            return url;
        }

        public static string RemoveParam(this UrlHelper urlHelper, string name, string value)
        {
            string url = urlHelper.RequestContext.HttpContext.Request.RawUrl;

            url = Regex.Replace(url, string.Format(@"(\?|\&){0}={1}(&|$)", Regex.Escape(name), Regex.Escape(value)),
                "$1").TrimEnd('&', '?');

            return url;
        }

        public static string ToSlug(this string title)
        {
            if (title == null) return "";

            const int maxlen = 80;
            int len = title.Length;
            bool prevdash = false;
            var sb = new StringBuilder(len);
            char c;

            for (int i = 0; i < len; i++)
            {
                c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lowercase
                    sb.Append((char) (c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
                         c == '\\' || c == '-' || c == '_' || c == '=')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else if (c >= 128)
                {
                    int prevlen = sb.Length;
                    sb.Append(RemapInternationalCharToAscii(c));
                    if (prevlen != sb.Length) prevdash = false;
                }
                if (i == maxlen) break;
            }

            if (prevdash)
                return sb.ToString().Substring(0, sb.Length - 1);
            return sb.ToString();
        }

        public static string RemapInternationalCharToAscii(char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåąаъ".Contains(s))
            {
                return "a";
            }
            if ("б".Contains(s))
            {
                return "b";
            }
            if ("в".Contains(s))
            {
                return "v";
            }
            if ("д".Contains(s))
            {
                return "d";
            }
            if ("èéêëęе".Contains(s))
            {
                return "e";
            }
            if ("ж".Contains(s))
            {
                return "j";
            }
            if ("з".Contains(s))
            {
                return "z";
            }
            if ("ìíîïıий".Contains(s))
            {
                return "i";
            }
            if ("к".Contains(s))
            {
                return "k";
            }
            if ("л".Contains(s))
            {
                return "l";
            }
            if ("м".Contains(s))
            {
                return "m";
            }
            if ("òóôõöøőðо".Contains(s))
            {
                return "o";
            }
            if ("п".Contains(s))
            {
                return "p";
            }
            if ("р".Contains(s))
            {
                return "r";
            }
            if ("ùúûüŭů".Contains(s))
            {
                return "u";
            }
            if ("çćčĉ".Contains(s))
            {
                return "c";
            }
            if ("żźž".Contains(s))
            {
                return "z";
            }
            if ("śşšŝс".Contains(s))
            {
                return "s";
            }
            if ("т".Contains(s))
            {
                return "t";
            }
            if ("ф".Contains(s))
            {
                return "f";
            }
            if ("ñńн".Contains(s))
            {
                return "n";
            }
            if ("ýÿу".Contains(s))
            {
                return "y";
            }
            if ("х".Contains(s))
            {
                return "h";
            }
            if ("ч".Contains(s))
            {
                return "ch";
            }
            if ("ц".Contains(s))
            {
                return "ts";
            }
            if ("ч".Contains(s))
            {
                return "ch";
            }
            if ("ш".Contains(s))
            {
                return "sh";
            }
            if ("щ".Contains(s))
            {
                return "sht";
            }
            if ("ю".Contains(s))
            {
                return "iu";
            }
            if ("я".Contains(s))
            {
                return "iа";
            }
            if ("ğĝг".Contains(s))
            {
                return "g";
            }
            if (c == 'ř')
            {
                return "r";
            }
            if (c == 'ł')
            {
                return "l";
            }
            if (c == 'đ')
            {
                return "d";
            }
            if (c == 'ß')
            {
                return "ss";
            }
            if (c == 'Þ')
            {
                return "th";
            }
            if (c == 'ĥ')
            {
                return "h";
            }
            if (c == 'ĵ')
            {
                return "j";
            }
            return "";
        }
    }
}