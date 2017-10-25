using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using ExcoUtility;
using MvcApplication1.Financial_Reports.Income_Statement;
using MvcApplication1.Models;
using Process = System.Diagnostics.Process;

namespace MvcApplication1.Controllers
{
    public class FinancialsController : Controller
    {
        // GET: Financials
        [Route("Financials/IncomeStatement/{paramOne}")]
        public ActionResult IncomeStatement(string paramOne)
        {
            return View();
        }

        // GET: Financials
        [Route("Financials/SalesReport/{paramOne}")]
        public ActionResult SalesReport(string paramOne)
        {
            return View();
        }

        // GET: Financials
        [Route("Financials/TrialBalance/{paramOne}")]
        public ActionResult TrialBalance(string paramOne)
        {
            return View();
        }

        // GET: Financials
        [Route("Financials/YTD/{paramOne}")]
        public ActionResult YTD(string paramOne)
        {
            return View();
            
        }

        // GET: Financials
        [Route("Financials/ExchangeRates/{paramOne}")]
        public ActionResult ExchangeRates(string paramOne)
        {
            FinancialControls.CurrencyYearList = ExcoExRate.GetExchangeRatesList();
            return View();
            
        }

        // GET: Financials
        [Route("Financials/AddExchange/{paramOne}")]
        public ActionResult AddExchange(string paramOne)
        {
            string[] parameters = paramOne.Split(new[] { "," }, StringSplitOptions.None);
            if (parameters.Length == 2 && !FinancialControls.CurrencyYearList.Any(x => x.Year.ToString() == parameters[0] && ExcoExRate.GetCurrency(parameters[1]) == x.CurrencyType))
            {
                CurrencyYear newCY = new CurrencyYear(ExcoExRate.GetCurrency(parameters[1]),
                    Convert.ToInt32(parameters[0]));

                // Instantiate 0 values for all 12 periods
                for (int i = 0; i < 12; i++)
                {
                    newCY.ExchangeRates.Add(0);
                }

                FinancialControls.CurrencyYearList.Add(newCY);

                // Order by currency -> year
                FinancialControls.CurrencyYearList = FinancialControls.CurrencyYearList.OrderBy(x => x.CurrencyType).ThenBy(y => y.Year).ToList();

                ExcoExRate.SaveExchangeRates(FinancialControls.CurrencyYearList);
            }


            return RedirectToAction("ExchangeRates");
        }

        [Route("Financials/DeleteRate/{currYear}")]
        public ActionResult DeleteRate(string currYear)
        {
            FinancialControls.CurrencyYearList.RemoveAt(Convert.ToInt32(currYear));

            // Order by currency -> year
            FinancialControls.CurrencyYearList = FinancialControls.CurrencyYearList.OrderBy(x => x.CurrencyType).ThenBy(y => y.Year).ToList();

            ExcoExRate.SaveExchangeRates(FinancialControls.CurrencyYearList);
            
            return RedirectToAction("ExchangeRates", new {paramOne = "ER"});
        }


        [HttpPost]
        [Route("Financials/SaveExchange/{parameterStr}")]
        public ActionResult SaveExchange(string parameterStr)
        {
            parameterStr = parameterStr.Replace('x', '.');
            string[] parameters = parameterStr.Split(new[] {","}, StringSplitOptions.None);
            
            if (parameters.Length > 1)
            {
                try
                {
                    CurrencyYear refCY = FinancialControls.CurrencyYearList[Convert.ToInt32(parameters[0])];

                    double originalRate = refCY.ExchangeRates[Convert.ToInt32(parameters[1])];

                    // Update new rate
                    refCY.ExchangeRates[Convert.ToInt32(parameters[1])] = Convert.ToDouble(parameters[2]);

                    Log.Append(String.Format("Rate updated ({0} - {1}) : orig={2}, new={3}, by {4}", refCY.CurrencyType, refCY.Year, originalRate, parameters[2], HttpContext.Session["Email"]));

                    ExcoExRate.SaveExchangeRates(FinancialControls.CurrencyYearList);
                }
                catch (Exception ex)
                {
                    // Non-integer error
                }
            }

            return RedirectToAction("ExchangeRates", new { parameterStr = "ER" });
        }


        [HttpPost]
        [Route("Financials/GenerateIncomeStatement/{parameterStr}")]
        public ActionResult GenerateIncomeStatement(string parameterStr)
        {
            string[] parameters = parameterStr.Split(new[] {","}, StringSplitOptions.None);

            MvcApplication1.Financial_Reports.Income_Statement.Process process = new MvcApplication1.Financial_Reports.Income_Statement.Process(Convert.ToInt32(parameters[1]), Convert.ToInt32(parameters[2]));
            // create excel object

            Log.Append("    1");
            try
            {
            }
            catch
            {
                Log.Append("sdfsdf");
            }

            Log.Append("    1.51");
            ExcelWriter excelWriter = new ExcelWriter();

            try
            {
                Log.Append("    1.6");
                excelWriter = new ExcelWriter(process);
                Log.Append("    1.7");
            }
            catch (Exception ex)
            {
                Log.Append("sdfsdf2");
            }
            Log.Append("    2");
            excelWriter.FillSheets();
            Log.Append("    3");
            // write to file
            string path = @"\\10.0.0.8\EmailAPI\Financials\IS-Reports\Income Statement Report at " + process.fiscalMonth + "-" + process.fiscalYear + ".xlsx";
            //string path = "C:\\Sales Report\\Income Statement Report at " + process.fiscalMonth + "-" + process.fiscalYear + ".xlsx";
            Log.Append("    4");
            System.IO.File.Delete(path);
            Log.Append("    5");
            excelWriter.OutputToFile(path);
            Log.Append("    Opening Excel File (" + path + ")...");
            System.Diagnostics.Process.Start(path);

            return RedirectToAction("IncomeStatement");
            //return Redirect("/Financials/IncomeStatement/syncSettings");
        }
    }
}