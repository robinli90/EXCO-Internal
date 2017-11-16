using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using MvcApplication1.Models;
using MvcApplication1.Paperless_System;

namespace MvcApplication1.Controllers
{
    public class PaperlessController : Controller
    {
        #region Invoicing
        // GET: Paperless
        [Route("Paperless/DailyInvoicing/{paramOne}")]
        public ActionResult DailyInvoicing()
        {
            return View();
        }

        // GET: Paperless
        [HttpGet]
        [Route("Paperless/DailyInvoicingRefresh/{paramOne}")]
        public ActionResult DailyInvoicingRefresh(string paramOne)
        {
            DateTime refDate = new DateTime();

            if (paramOne.Length > 5 && DateTime.TryParse(paramOne, out refDate))
                refDate = DateTime.ParseExact(paramOne, "MM-dd-yyyy",
                    CultureInfo.InvariantCulture);

            ArchivesChecker.ProcessEmailsForArchive();
            ArchivesChecker.CheckPendingOrders();
            ArchivesChecker.PopulateOrdersByInvoiceDate(refDate);
            return RedirectToAction("DailyInvoicing");
        }

        // GET: Paperless
        [HttpGet]
        [Route("Paperless/DailyInvoicingGetOrder/{paramOne}")]
        public ActionResult DailyInvoicingGetOrder(string paramOne)
        {
            if (paramOne.Length != 6 && !paramOne.All(char.IsDigit))
                return RedirectToAction("DailyInvoicing");

            ArchivesChecker.PopulateOrdersByOrderNo(paramOne);
            return RedirectToAction("DailyInvoicing");
        }
        
        [HttpPost]
        [Route("Paperless/InvoiceDragUpload/{paramOne}")]
        public ActionResult InvoiceDragUpload(string paramOne)
        {
            if (paramOne.Length != 6 && !paramOne.All(char.IsDigit))
                return RedirectToAction("DailyInvoicing");
            
            ArchivesChecker.CreateArchiveDirectory(paramOne);

            foreach (string file in Request.Files)
            {
                HttpPostedFileBase fileContent = Request.Files[file];

                Stream stream = fileContent.InputStream;

                int existingFileCount = Directory
                    .GetFiles(Path.Combine(ArchivesChecker._archivePath, paramOne), "*.msg").Length;

                string fileName = Path.GetFileName(paramOne + (existingFileCount > 0 ? "_" + ++existingFileCount : "") + ".msg");

                string path = Path.Combine(Path.Combine(ArchivesChecker._archivePath, paramOne), fileName);

                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                }
            }

            return RedirectToAction("DailyInvoicing");
        }

        #endregion

        #region Orders
        // GET: Paperless
        [Route("Paperless/DailyOrders/{paramOne}")]
        public ActionResult DailyOrders()
        {
            return View();
        }


        // GET: Paperless
        [HttpGet]
        [Route("Paperless/DailyOrderRefresh/{paramOne}")]
        public ActionResult DailyOrderRefresh(string paramOne)
        {
            DateTime refDate = new DateTime();

            if (paramOne.Length > 5)
                refDate = DateTime.ParseExact(paramOne, "MM-dd-yyyy",
                    CultureInfo.InvariantCulture);

            ArchivesChecker.PopulateOrdersByOrderDate(refDate);
            return RedirectToAction("DailyOrders");
        }

        // GET: Paperless
        [HttpGet]
        [Route("Paperless/DailyOrderGetOrder/{paramOne}")]
        public ActionResult DailyOrderGetOrder(string paramOne)
        {
            if (paramOne.Length != 6 && !paramOne.All(char.IsDigit))
                return RedirectToAction("DailyInvoicing");

            ArchivesChecker.PopulateDieOrdersByOrderNo(paramOne);
            return RedirectToAction("DailyOrders");
        }
        
        [HttpPost]
        [Route("Paperless/OrdersDragUpload/{paramOne}")]
        public ActionResult OrdersDragUpload(string paramOne)
        {
            if (paramOne.Length != 6 && !paramOne.All(char.IsDigit))
                return RedirectToAction("DailyOrders");


            ArchivesChecker.CreateArchiveDirectory(paramOne);

            foreach (string file in Request.Files)
            {
                HttpPostedFileBase fileContent = Request.Files[file];

                Stream stream = fileContent.InputStream;

                int existingFileCount = Directory
                    .GetFiles(Path.Combine(ArchivesChecker._archivePath, paramOne), "*_DIEFORM.msg").Length;

                string fileName = Path.GetFileName(paramOne + (existingFileCount > 0 ? "_" + ++existingFileCount : "") + "_DIEFORM.msg");

                string path = Path.Combine(Path.Combine(ArchivesChecker._archivePath, paramOne), fileName);

                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                }
            }


            ArchivesChecker.CurrentOrders.First(x => x._orderNo == paramOne)._hasFolder = true;

            return RedirectToAction("DailyOrders");
        }
        #endregion

        #region Archives
        // GET: Paperless
        [Route("Paperless/SearchArchive/{paramOne}")]
        public ActionResult SearchArchive()
        {
            return View();
        }

        [HttpGet]
        [Route("Paperless/RefreshArchives/{paramOne}")]
        public ActionResult RefreshArchives()
        {
            ArchivesChecker.GetEntireArchive();
            return RedirectToAction("SearchArchive");
        }
        
        [HttpGet]
        // GET: EmailRepository
        [Route("Paperless/GetFilesById/{id}")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetFilesById(string id)
        {
            return String.Join("<br />", ArchivesChecker.GetFilesForOrder(id));
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

        [HttpPost]
        [Route("Paperless/UploadToArchive/{paramOne}")]
        public ActionResult UploadToArchive(string paramOne)
        {
            foreach (string file in Request.Files)
            {
                HttpPostedFileBase fileContent = Request.Files[file];

                Stream stream = fileContent.InputStream;

                string fileName = Path.GetFileName(file);

                string path = Path.Combine(Path.Combine(ArchivesChecker._archivePath, paramOne), fileName);

                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                }
            }

            return RedirectToAction("SearchArchive");
        }
        #endregion
    }
}