using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BechdelTonight.Extensions
{
    public static class DateTimeExtensions
    {
        public static string FormatForGuide(this DateTime datetime)
        {
            if (datetime == DateTime.Today)
            {
                return "Today";
            }
            else if (datetime == DateTime.Today.AddDays(1))
            {
                return "Tomorrow";
            }
            else
            {
                return datetime.ToString("ddd, MMMM dd");
            }
        }
    }
}