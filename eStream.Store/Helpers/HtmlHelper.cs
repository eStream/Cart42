using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Elmah;
using Estream.Cart42.Web.Services;
using Forloop.HtmlHelpers;
using StructureMap;

namespace Estream.Cart42.Web.Helpers
{
    public static class HtmlHelper
    {
        public static MvcHtmlString RenderHtmlAttributes<TModel>(
            this HtmlHelper<TModel> htmlHelper, object htmlAttributes)
        {
            var attrbituesDictionary = new RouteValueDictionary(htmlAttributes);

            return MvcHtmlString.Create(String.Join(" ",
                attrbituesDictionary.Keys.Select(
                    key => String.Format("{0}=\"{1}\"", key,
                        htmlHelper.Encode(attrbituesDictionary[key])))));
        }

        public static void RenderActionAjax<TModel>(
            this HtmlHelper<TModel> htmlHelper, string actionName, string controllerName)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var writer = htmlHelper.ViewContext.Writer;

            writer.Write(@"
<div id=""{0}""></div>
<script type=""text/javascript"">
    $.ajax({{
          url: '{1}',
          dataType: 'html',
          success: function(data) {{
             $('#{0}').html(data);
          }}
    }});
</script>", Guid.NewGuid(), urlHelper.Action(actionName, controllerName));
        }

        public static MvcHtmlString LoadScript<TModel>(
            this HtmlHelper<TModel> htmlHelper, string name = null)
        {
            var controller = htmlHelper.ViewContext.RouteData.Values["Controller"].ToString();
            name = name ?? htmlHelper.ViewContext.RouteData.Values["Action"].ToString();

            return MvcHtmlString.Create(string.Format(
                "<script src=\"{2}?c={0}&n={1}\" type=\"text/javascript\"></script>",
                controller, name, CurrentUrlHelper().Action("Index", "ViewScript")));
        }

        public static UrlHelper CurrentUrlHelper()
        {
            return new UrlHelper(HttpContext.Current.Request.RequestContext);
        }

        public static string StripHTML(this string text)
        {
            return text == null ? null : Regex.Replace(text, @"<(.|\n)*?>", string.Empty);
        }

        public static string StripScriptTags(this string text)
        {
            return Regex.Replace(text, @"<script(.|\n)*?>(.|\n)*?</script(.|\n)*?>", string.Empty,
                RegexOptions.IgnoreCase);
        }

        public static string Shorten(this string text, int length)
        {
            if (text == null) return null;
            if (text.Length <= length) return text;
            return text.Remove(length) + "...";
        }

        public static string ToUpperFirst(this string text)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(text[0]) + text.Substring(1);
        }

        public static string BR2NL(this string text)
        {
            if (text == null) return null;
            return Regex.Replace(text, @"<br\s?/?>", "\n", RegexOptions.IgnoreCase);
        }

        public static string NL2BR(this string text)
        {
            if (text == null) return null;
            text = text.Replace("\r", "").Trim(new[] {' ', '\t', '\n'});
            if (text.IndexOf("\n\n \n\n", StringComparison.Ordinal) > 0) // Detect Outlook double spacing
                text = text.Replace("\n\n", "\n");
            return text.Replace("\n\n\n", "\n\n").Replace("\n", "<br />");
        }

        public static string Urls2Links(this string text)
        {
            try
            {
                const string regex =
                    @"(?<!<[^>]{0,100})((www\.|(http|https|ftp|news|file)+\:\/\/)[_.\w-]{1,200}\.([\w\/_:@=.+?,##%~-]|&amp;){0,100}[^.|\'|\# |!|\(|?|,| |>|<|;|\|&)])(?![^<]{0,100}>)";
                var r = new Regex(regex, RegexOptions.IgnoreCase);
                return r.Replace(text, "<a href=\"$1\" target=\"_blank\" rel=\"nofollow\">$1</a>").Replace("href=\"www",
                    "href=\"http://www");
            }
            catch (Exception err)
            {
                ErrorLog.GetDefault(HttpContext.Current).Log(new Error(err));
                return text;
            }
        }

        public static string TrimLongWords(this string text, int maxWordLength)
        {
            if (text == null) return null;
            return Regex.Replace(text, @"([\w\d]{" + (maxWordLength + 1) + @"})", m => m.Value + " ",
                RegexOptions.Compiled);
        }

        public static string AsNullIfEmpty(this string items)
        {
            if (String.IsNullOrEmpty(items))
            {
                return null;
            }
            return items;
        }

        public static string AsNullIfWhiteSpace(this string items)
        {
            if (String.IsNullOrWhiteSpace(items))
            {
                return null;
            }
            return items;
        }

        public static IEnumerable<T> AsNullIfEmpty<T>(this IEnumerable<T> items)
        {
            T[] asNullIfEmpty = items as T[] ?? items.ToArray();
            if (items == null || !asNullIfEmpty.Any())
            {
                return null;
            }
            return asNullIfEmpty;
        }

        #region ListModelBindingExtensions
        static Regex _stripIndexerRegex = new Regex(@"\[(?<index>\d+)\]", RegexOptions.Compiled);

        public static string GetIndexerFieldName(this TemplateInfo templateInfo) {
            string fieldName = templateInfo.GetFullHtmlFieldName("Index");
            fieldName = _stripIndexerRegex.Replace(fieldName, string.Empty);
            if (fieldName.StartsWith(".")) {
                fieldName = fieldName.Substring(1);
            }
            return fieldName;
        }

        public static int GetIndex(this TemplateInfo templateInfo)
        {
            string fieldName = templateInfo.GetFullHtmlFieldName("Index");
            var match = _stripIndexerRegex.Match(fieldName);
            if (match.Success) { 
                return int.Parse(match.Groups["index"].Value);
            }
            return 0;
        }

        public static MvcHtmlString HiddenIndexerInputForModel<TModel>(this HtmlHelper<TModel> html) {
            string name = html.ViewData.TemplateInfo.GetIndexerFieldName();
            object value = html.ViewData.TemplateInfo.GetIndex();
            string markup = String.Format(@"<input type=""hidden"" name=""{0}"" value=""{1}"" />", name, value);
            return MvcHtmlString.Create(markup);
        }
        #endregion
    }
}