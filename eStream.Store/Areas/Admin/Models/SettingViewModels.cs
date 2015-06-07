using System.Collections.Generic;
using System.Web.Mvc;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class SettingsViewModel
    {
        public string Category { get; set; }

        public List<SettingViewModel> Settings { get; set; }
    }

    public class SettingViewModel
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [AllowHtml]
        public string Value { get; set; }
        public string EditorType { get; set; }
    }
}