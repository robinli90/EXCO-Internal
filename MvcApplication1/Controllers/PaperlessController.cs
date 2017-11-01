using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.Paperless_System;

namespace MvcApplication1.Controllers
{
    public class PaperlessController : Controller
    {
        // GET: Paperless
        [Route("Paperless/DailyInvoicing/{paramOne}")]
        public ActionResult DailyInvoicing()
        {
            return View();
        }

        // GET: Paperless
        [Route("Paperless/SearchArchive/{paramOne}")]
        public ActionResult SearchArchive()
        {
            return View();
        }

        // GET: Paperless
        [HttpGet]
        [Route("Paperless/DailyInvoicingRefresh/{paramOne}")]
        public ActionResult DailyInvoicingRefresh(string paramOne)
        {
            DateTime refDate = new DateTime();

            if (paramOne.Length > 5)
                refDate = DateTime.ParseExact(paramOne, "MM-dd-yyyy",
                    CultureInfo.InvariantCulture);

            //DateTime.TryParse(paramOne, out refDate);

            ArchivesChecker.ProcessEmailsForArchive();
            ArchivesChecker.CheckPendingOrders();
            ArchivesChecker.PopulateOrders(refDate);
            return RedirectToAction("DailyInvoicing");
        }

        // GET: Paperless
        [HttpGet]
        [Route("Paperless/DownloadArchive/{paramOne}")]
        public ActionResult DownloadArchive(string paramOne)
        {
            string archivePackagePath = ArchivesChecker.CreatePackage(paramOne);

            if (archivePackagePath.Length == 0)
                return RedirectToAction("SearchArchive");

            byte[] fileBytes = System.IO.File.ReadAllBytes(archivePackagePath);
            string fn = paramOne + ".zip";
            return File(fileBytes, "application/octet-stream", fn);
            
        }
    }
}