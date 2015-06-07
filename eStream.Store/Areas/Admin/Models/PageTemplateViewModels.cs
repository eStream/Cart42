using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class PageTemplatesIndexViewModel
    {
        public PageTemplatesIndexViewModel()
        {
            Templates = new List<PageTemplateIndexViewModel>();
        }

        public string SelectedTemplate { get; set; }
        public List<PageTemplateIndexViewModel> Templates { get; set; }
    }

    public class PageTemplateIndexViewModel
    {
        public string Name { get; set; }
        public string Thumbnail { get; set; }
    }

    public class PageTemplateEditViewModel
    {
        public PageTemplateEditViewModel()
        {
            CssFiles = new List<CssFileViewModel>();
            ViewFiles = new List<ViewFileViewModel>();
        }

        public string Name { get; set; }
        public List<CssFileViewModel> CssFiles { get; set; }
        public List<ViewFileViewModel> ViewFiles { get; set; }
    }

    public class CssFileViewModel
    {
        public string Name { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ViewFileViewModel
    {
        public string Name { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ThemeSettingsEditViewModel
    {
        public ThemeSettingsEditViewModel()
        {
            Sections = new List<ThemeSettingSectionEditViewModel>();
        }

        public string Name { get; set; }
        public List<ThemeSettingSectionEditViewModel> Sections { get; set; }
    }

    public class ThemeSettingSectionEditViewModel
    {
        public ThemeSettingSectionEditViewModel()
        {
            Items = new List<ThemeSettingItemEditViewModel>();
        }

        public string Name { get; set; }
        public List<ThemeSettingItemEditViewModel> Items { get; set; }
    }

    public class ThemeSettingItemEditViewModel
    {
        private string value;
        public string Key { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> DescriptionT { get; set; }
        public string Default { get; set; }
        public Dictionary<string, string> DefaultT { get; set; }
        public ThemeSettingItemType Type { get; set; }
        public List<ThemeSettingItemOptionEditViewModel> Options { get; set; }
        public string Hint { get; set; }
        public Dictionary<string, string> HintT { get; set; }

        public string Value
        {
            get { return value ?? Default; }
            set { this.value = value; }
        }

        public bool ValueBool
        {
            get { return Convert.ToBoolean(value ?? Default); }
            set { this.value = value.ToString(); }
        }

        public HttpPostedFileBase ValueFile { get; set; }
    }

    public enum ThemeSettingItemType
    {
        Textbox,
        Checkbox,
        Upload,
        Dropdown,
        Color,
        Multiline,
        Html
    }

    public class ThemeSettingItemOptionEditViewModel
    {
        public string Value { get; set; }
        public string Description { get; set; }
    }
}