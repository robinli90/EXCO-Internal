using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using ExcoUtility;
using MvcApplication1.Financial_Reports.Income_Statement;
using MvcApplication1.Financial_Reports.YTD;
using MvcApplication1.Models;

namespace MvcApplication1.Controllers
{
    public class FinancialsController : Controller
    {


        #region Exchange Rates
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
        #endregion

        #region Income Statement

        // GET: Financials
        [Route("Financials/IncomeStatement/{paramOne}")]
        public ActionResult IncomeStatement(string paramOne)
        {
            return View();
        }

        private static int runCountIS = 0;

        [HttpGet]
        [Route("Financials/GenerateIncomeStatement/{parameterStr}")]
        public ActionResult GenerateIncomeStatement(string parameterStr)
        {
            string[] parameters = parameterStr.Split(new[] { "," }, StringSplitOptions.None);
            
            // First time 
            string fileName = "Income Statement Report at " + parameters[1] + "-" + parameters[2] + ".xlsx";
            string path = @"\\10.0.0.8\EmailAPI\Financials\IS-Reports\" + fileName;
            
            if (runCountIS % 2 == 0)
            {

                MvcApplication1.Financial_Reports.Income_Statement.Process process = new MvcApplication1.Financial_Reports.Income_Statement.Process(Convert.ToInt32(parameters[1]), Convert.ToInt32(parameters[2]));
                // create excel object
                //string path = @"\\10.0.0.8\EmailAPI\Financials\IS-Reports\Income Statement Report at " + process.fiscalMonth + "-" + process.fiscalYear + ".xlsx";
    
                ExcelWriter excelWriter = new ExcelWriter(process);

                excelWriter.FillSheets();
                System.IO.File.Delete(path);
                excelWriter.OutputToFile(path);
    
                // Let file settle
                Thread.Sleep(1000);
            }

            runCountIS++;

            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fn = fileName;
            return File(fileBytes, "application/octet-stream", fn);
        }

        #endregion

        #region YTD Income Statement

        // GET: Financials
        [Route("Financials/YTD/{paramOne}")]
        public ActionResult YTD(string paramOne)
        {
            return View();

        }
        
        private static int runCountYTD;

        [HttpGet]
        [Route("Financials/GenerateYTDIS/{parameterStr}")]
        public ActionResult GenerateYTDIS(string parameterStr)
        {
            Log.Append("Creating YTD Income Statement Excel spreadsheet...");

            string[] parameters = parameterStr.Split(new[] {","}, StringSplitOptions.None);
            
            Updater upd = new Updater(parameters[1], parameters[2]);

            if (parameters.Length > 3 && parameters[3] == "true")
            {
                upd._SET_CURRENCY(true);
            }

            // First time 
            string fileName = "YTD at " + DateTime.Now.Year + "-" + DateTime.Now.Month + " (" + parameters[0] +
                              ").xlsx";
            string path = @"\\10.0.0.8\EmailAPI\Financials\YTD-IS-Reports\" + fileName;

            if (runCountYTD % 2 == 0)
            {
                upd.Generate(path,
                    parameters[0],
                    parameters[1],
                    parameters[2]);

                // Let file settle
                Thread.Sleep(1000);
            }

            runCountYTD++;

            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fn = fileName;
            return File(fileBytes, "application/octet-stream", fn);
        }

        [HttpGet]
        [Route("Financials/RegenerateYTD/{parameterStr}")]
        public ActionResult RegenerateYTD(string parameterStr)
        {
            Log.Append("Refreshing Data for YTD");

            string[] parameters = parameterStr.Split(new[] {","}, StringSplitOptions.None);

            // First time 
            string fileName = "YTD at " + DateTime.Now.Year + "-" + DateTime.Now.Month + " (" + parameters[0] +
                                ").xlsx";
            string path = @"\\10.0.0.8\EmailAPI\Financials\YTD-IS-Reports\" + fileName;

            Updater upd = new Updater(parameters[1], parameters[2]);

            upd.RefreshData();

            // Let file settle
            Thread.Sleep(1000);
            return RedirectToAction("YTD");
        }

        #endregion

        #region Sales Report
        // GET: Financials
        [Route("Financials/SalesReport/{paramOne}")]
        public ActionResult SalesReport(string paramOne)
        {
            return View();
        }

        private static int runCountSR = 0;

        [HttpGet]
        [Route("Financials/GenerateSalesReport/{parameterStr}")]
        public ActionResult GenerateSalesReport(string parameterStr)
        {

            // First time 
            string fileName = "Sales Report at " + DateTime.Now.Year + "-" + DateTime.Now.Month + ".xlsx";
            string path = @"\\10.0.0.8\EmailAPI\Financials\Sales-Reports\" + fileName;
            
            if (runCountSR % 2 == 0)
            {
                Financial_Reports.Sales_Report.Process process = new Financial_Reports.Sales_Report.Process(path);
                // create excel object
    
                process.Run(parameterStr == "true");
                
                // Let file settle
                Thread.Sleep(1000);
            }

            runCountSR++;

            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fn = fileName;
            return File(fileBytes, "application/octet-stream", fn);
        }
        #endregion

        #region Trial Balance

        // GET: Financials
        [Route("Financials/TrialBalance/{paramOne}")]
        public ActionResult TrialBalance(string paramOne)
        {
            return View();
        }

        private static int runCountTB = 0;

        [HttpGet]
        [Route("Financials/GenerateTrialBalance/{parameterStr}")]
        public ActionResult GenerateTrialBalance(string parameterStr)
        {
            string[] parameters = parameterStr.Split(new[] { "," }, StringSplitOptions.None);

            // First time 
            string fileName = "Trial Balance at " + DateTime.Now.Year + "-" + DateTime.Now.Month + " (" + parameters[0] + ").xlsx";
            string path = @"\\10.0.0.8\EmailAPI\Financials\Trial-Balance\" + fileName;

            if (runCountTB % 2 == 0)
            {
                Financial_Reports.Trial_Balance.Process process = new Financial_Reports.Trial_Balance.Process(path, parameters[0], 
                    Convert.ToInt32(parameters[1]), 
                    Convert.ToInt32(parameters[2]));

                // Let file settle
                Thread.Sleep(1000);
            }

            runCountTB++;

            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fn = fileName;
            return File(fileBytes, "application/octet-stream", fn);
        }

        #endregion
    }
}