using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using ExcoUtility;
using IncomeStatementReport;
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


        [Route("Financials/ModifyRate/{currYear}")]
        public ActionResult ModifyRate(string currYear)
        {
            // if UserManagement
            return RedirectToAction("ExchangeRates");
        }

        [Route("Financials/DeleteRate/{currYear}")]
        public ActionResult DeleteRate(string currYear)
        {
            // if UserManagement
            return RedirectToAction("ExchangeRates");
        }


        [HttpPost]
        [Route("Financials/GenerateIncomeStatement/{parameterStr}")]
        public ActionResult GenerateIncomeStatement(string parameterStr)
        {
            string[] parameters = parameterStr.Split(new[] {","}, StringSplitOptions.None);

            IncomeStatementReport.Process process = new IncomeStatementReport.Process(Convert.ToInt32(parameters[1]), Convert.ToInt32(parameters[2]));
            // create excel object
            ExcelWriter excelWriter = new ExcelWriter(process);
            excelWriter.FillSheets();
            // write to file
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Income Statement Report at " + process.fiscalMonth + "-" + process.fiscalYear + ".xlsx");
            //string path = "C:\\Sales Report\\Income Statement Report at " + process.fiscalMonth + "-" + process.fiscalYear + ".xlsx";
            System.IO.File.Delete(path);
            excelWriter.OutputToFile(path);
            System.Diagnostics.Process.Start(path);

            return RedirectToAction("IncomeStatement");
            //return Redirect("/Financials/IncomeStatement/syncSettings");
        }
    }
}