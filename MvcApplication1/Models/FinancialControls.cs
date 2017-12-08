using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExcoUtility;

namespace MvcApplication1.Models
{
    public enum FinancialPlant
    {
        All,
        Consolidated,
        Markham,
        Michigan,
        Texas,
        Colombia
    }

    public static class FinancialPlantControls
    {
        public static FinancialPlant GetFinancialPlant(string financialPlantName)
        {
            switch (financialPlantName)
            {
                case "All":
                {
                    return FinancialPlant.All;
                }
                case "Consolidated":
                {
                    return FinancialPlant.Consolidated;
                }
                case "Markham":
                {
                    return FinancialPlant.Markham;
                }
                case "Michigan":
                {
                    return FinancialPlant.Michigan;
                }
                case "Texas":
                {
                    return FinancialPlant.Texas;
                }
                case "Colombia":
                {
                    return FinancialPlant.Colombia;
                }
                default:
                {
                    return FinancialPlant.Colombia;
                }
            }
        }
    }

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