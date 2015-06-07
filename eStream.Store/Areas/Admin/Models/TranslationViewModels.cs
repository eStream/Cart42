using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class LanguageSelectorViewModel
    {
        public LanguageSelectorViewModel()
        {
            Languages = new List<LanguageViewModel>();
        }

        public string CurrentLanguageCode { get; set; }
        public List<LanguageViewModel> Languages { get; set; }
    }

    public class LanguageViewModel
    {
        public string Code { get; set; }
        public string DisplayName { get; set; }
    }
}