using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public interface ITemplateSettingService
    {
        TemplateSetting SetSetting(string templateName, string key, string value);

        string GetSetting(string templateName, string key);
        void ResetSettings(string templateName);
    }
}
