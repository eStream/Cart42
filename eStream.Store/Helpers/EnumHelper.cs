using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Estream.Cart42.Web.Helpers
{
    public static class EnumHelper
    {
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            Type type = enumVal.GetType();
            MemberInfo[] memInfo = type.GetMember(enumVal.ToString());
            object[] attributes = memInfo[0].GetCustomAttributes(typeof (T), false);
            return (attributes.Length > 0) ? (T) attributes[0] : null;
        }

        public static string DisplayName(this Enum enumeration)
        {
            string name = enumeration.ToString();
            var descriptionAttributeArray = enumeration.GetType().GetField(name).GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
            if (descriptionAttributeArray == null || descriptionAttributeArray.Length == 0)
                return name;
            return descriptionAttributeArray[0].Name;
        }

    }
}