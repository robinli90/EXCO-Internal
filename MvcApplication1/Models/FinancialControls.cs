using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExcoUtility;

namespace MvcApplication1.Models
{
    public static class FinancialControls
    {
        public static List<CurrencyYear> CurrencyYearList { get; set; }

        public static bool IsCurrentPeriod(int refYear, int refMonth)
        {
            int referenceMonth = DateTime.Now.Month + 1; // reference adds one month
            int referenceYear = DateTime.Now.Year;

            if (referenceMonth > 12)
            {
                referenceMonth = 1;
                referenceYear++;
            }

            if (refYear == referenceYear && referenceMonth == refMonth)
                return true;

            return false;
        }
    }
}