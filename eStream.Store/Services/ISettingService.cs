using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Estream.Cart42.Web.Services
{
    public interface ISettingService
    {
        T Get<T>(SettingField key);
        void Set<T>(SettingField key, T value);
    }
}