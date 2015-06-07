using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace Estream.Cart42.Web.Helpers
{
    public class RazorTemplateBase<TT> : TemplateBase<TT>
    {
        public IEncodedString T(string text)
        {
            return Raw(text.T());
        }

        public UrlHelper Url
        {
            get
            {
                return new UrlHelper(HttpContext.Current.Request.RequestContext);
            }
        }
    }
}