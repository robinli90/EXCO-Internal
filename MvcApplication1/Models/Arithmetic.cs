using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models
{
    public static class Arithmetic
    {
        public static string GetStopWatchStr(double milliseconds)
        {
            double seconds = Math.Round((milliseconds) / 1000, 2);

            int minutes = Convert.ToInt32(seconds / 60);
            int hours = Convert.ToInt32(minutes / 60);

            seconds = Math.Round(seconds % 60, 2);

            if (hours >= 1)
            {
                return String.Format("{0} hour(s), {1} minutes, {2} seconds", hours, minutes % 60, seconds);
            }

            if (minutes >= 1)
            {
                return String.Format("{0} minute(s), {1} seconds", minutes % 60, seconds);
            }

            return String.Format("{0} seconds", seconds);

        }

        public static string ParseEmailBrackets(string input)
        {
            if (input.Contains("<"))
            {
                int firstCaratIndex = input.IndexOf("<");
                input = input.Substring(firstCaratIndex + 1, input.Length - 2 - firstCaratIndex).Trim();
            }

            return input;
        }


    }
}