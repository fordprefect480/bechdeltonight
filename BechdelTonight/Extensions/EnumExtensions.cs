using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace BechdelTonight.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription<T>(this T enumeration)
        {
            var fi = enumeration.GetType().GetField(enumeration.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return enumeration.ToString();
        }
    }
}