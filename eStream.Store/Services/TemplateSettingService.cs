using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Services
{
    public class TemplateSettingService : ITemplateSettingService
    {
        private readonly DataContext db;

        private static readonly ConcurrentDictionary<string, ThemeSettingsEditViewModel> _themeSettings =
            new ConcurrentDictionary<string, ThemeSettingsEditViewModel>();
        private static readonly ConcurrentDictionary<string, string> _settingsCache =
            new ConcurrentDictionary<string, string>();

        public TemplateSettingService(DataContext db)
        {
            this.db = db;
        }

        public TemplateSetting SetSetting(string templateName, string key, string value)
        {
            var setting = db.TemplateSettings.FirstOrDefault(s => s.TemplateName == templateName && s.Key == key);
            if (setting != null)
            {
                setting.Value = value;
            }
            else
            {
                setting = new TemplateSetting { TemplateName = templateName, Key = key, Value = value };
                db.TemplateSettings.Add(setting);
            }

            db.SaveChanges();

            // Update local cache
            _settingsCache.AddOrUpdate(templateName + key, value, (oldKey, oldvalue) => value);

            return setting;
        }

        public string GetSetting(string templateName, string key)
        {
            string value;
            if (_settingsCache.TryGetValue(templateName + key, out value))
                return value;

            var setting = db.TemplateSettings.FirstOrDefault(s => s.TemplateName == templateName && s.Key == key);

            if (setting == null)
            {
                setting = SetSetting(templateName, key, getDefault(templateName, key));
            }

            // Update local cache
            _settingsCache.AddOrUpdate(templateName + key, setting.Value, (oldKey, oldvalue) => setting.Value);

            return setting.Value;
        }

        public void ResetSettings(string templateName)
        {
            foreach (var setting in db.TemplateSettings.ToList())
            {
                db.TemplateSettings.Remove(setting);
                string dummy;
                _settingsCache.TryRemove(templateName + setting.Key, out dummy);
            }
            db.SaveChanges();
        }

        private string getDefault(string templateName, string key)
        {
            if (key == null || templateName == null) return null;
            ThemeSettingsEditViewModel model;
            if (!_themeSettings.TryGetValue(templateName, out model))
            {
                var viewDirectory = Path.Combine(HostingEnvironment.MapPath("~/Views"), templateName);
                if (!File.Exists(Path.Combine(viewDirectory, "settings.json")))
                    return null;
                var settingsJson = File.ReadAllText(Path.Combine(viewDirectory, "settings.json"));
                model = JsonConvert.DeserializeObject<ThemeSettingsEditViewModel>(settingsJson);
                if (!_themeSettings.TryAdd(templateName, model))
                {
                    // Already added by another thread
                    model = _themeSettings[templateName];
                }
            }
            foreach (var section in model.Sections)
            {
                var item = section.Items.FirstOrDefault(i => i.Key == key);
                if (item != null)
                {
                    if (item.DefaultT != null &&
                        item.DefaultT.ContainsKey(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName))
                        return item.DefaultT[CultureInfo.CurrentUICulture.TwoLetterISOLanguageName];
                    return item.Default;
                }
            }
            return null;
        }
    }
}