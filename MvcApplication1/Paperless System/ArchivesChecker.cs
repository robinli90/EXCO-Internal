using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using MvcApplication1.Models;
using MvcApplication1.Paperless_System.PDF_Generators;
using System.IO.Compression;
using WebGrease.Css.Extensions;

namespace MvcApplication1.Paperless_System
{
    public static class ArchivesChecker
    {
        public static readonly string _archivePath = @"\\10.0.0.8\EmailAPI\Paperless\Archive\";
        public static readonly string _errorPath = @"\\10.0.0.8\EmailAPI\Paperless\Archive_Email_Errors\";
        public static readonly string _pendingPath = @"\\10.0.0.8\EmailAPI\Paperless\Pending\";
        public static readonly string _packagePath = @"\\10.0.0.8\EmailAPI\Paperless\Packaged\";

        public static readonly string _drawingPath1 = @"\\10.0.0.8\sdrive\CADDRAWING\";
        public static readonly string _drawingPath2 = @"\\10.0.0.8\sdrive\CADDRAWINGBOL\";


        private static User archiveUser;

        public static List<ArchiveOrder> CurrentInvoiceOrders = new List<ArchiveOrder>();
        public static List<DieOrder> CurrentOrders = new List<DieOrder>();

        public static List<ArchiveOrder> Archives = new List<ArchiveOrder>();

        public static string currentInvoiceDateSearch = "";
        public static string currentOrderDateSearch = "";

        static ArchivesChecker()
        {
            PopulateOrdersByInvoiceDate();
            PopulateOrdersByOrderDate();
        }

        public static void InitializeArchive()
        {
            archiveUser = new User();
            archiveUser.Name = "Archive";
            archiveUser.ReceivingProtocol = "imap.pathcom.com";
            archiveUser.ReceivingPort = "993";
            archiveUser.Email = "archives@etsdies.com";
            archiveUser.Password = "5Zh2P8k4@2";
        }

        public static void ProcessEmailsForArchive()
        {
            if (archiveUser == null)
                InitializeArchive();

            archiveUser.GetArchiveEmails();
        }

        public static string CreateArchiveDirectory(string orderNumber)
        {
            Log.Append(String.Format("New order found (Order#={0}). Creating directory and pend file...", orderNumber));

            string directoryPath = _archivePath + orderNumber;

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(_pendingPath + orderNumber))
                File.CreateText(_pendingPath + orderNumber);

            return directoryPath;
        }

        public static void CheckPendingOrders()
        {
            foreach (string path in Directory.GetFiles(_pendingPath))
            {
                string orderNumber = Path.GetFileName(path).Substring(0, 6);

                if (IsOrderInvoiced(orderNumber))
                {
                    if (!Directory.Exists(_archivePath + orderNumber))
                    {
                        Directory.CreateDirectory(_archivePath + orderNumber);
                    }

                    Log.Append(String.Format("Order {0} has been invoiced. Creating order & invoice and copying drawing files to archive...", orderNumber));

                    PDFGenerator pdfObject = new PDFGenerator();
                    pdfObject.GenerateOrder(_archivePath + orderNumber, orderNumber);
                    pdfObject.GenerateInvoice(_archivePath + orderNumber, orderNumber);
                    CopyDrawings(orderNumber);

                    Log.Append(String.Format("Order {0} has been archived", orderNumber));

                    while (File.Exists(path))
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        public static void CopyDrawings(string orderNumber)
        {
            string path1 = Path.Combine(_drawingPath1, orderNumber + ".dwg");
            string path2 = Path.Combine(_drawingPath2, orderNumber + ".dwg");

            if (File.Exists(path1))
            {
                File.Copy(path1, Path.Combine(_archivePath + orderNumber, orderNumber + ".dwg"), true);
            }

            if (File.Exists(path2))
            {
                File.Copy(path2, Path.Combine(_archivePath + orderNumber, orderNumber + "_BOL.dwg"), true);
            }
        }

        public static bool IsOrderInvoiced(string orderNumber)
        {
            DateTime invoiceDate = new DateTime();
            
            SqlCommand objCmd = null;
            using (SqlConnection objConn =
                new SqlConnection("SERVER =10.0.0.6; Database =decade; UID =jamie; PWD =jamie;"))
            {
                objConn.Open();
                objCmd = objConn.CreateCommand();
                objCmd.CommandText = String.Format("select invoicedate from d_order where ordernumber = '{0}'",
                    orderNumber);

                using (SqlDataReader objReader = objCmd.ExecuteReader())
                {
                    if (objReader.HasRows)
                    {
                        while (objReader.Read())
                        {
                            return DateTime.TryParse(objReader["invoicedate"].ToString().Trim(), out invoiceDate);
                        }
                    }
                }
            }

            return invoiceDate.Year > 2000;
        }
        
        public static void PopulateOrdersByInvoiceDate(DateTime refDate = new DateTime())
        {
            if (refDate.Year < 2000)
                refDate = DateTime.Now.AddDays(-1);

            currentInvoiceDateSearch = refDate.ToShortDateString();

            Log.Append("Getting today's invoice list...");
            CurrentInvoiceOrders = new List<ArchiveOrder>();

            SqlCommand objCmd = null;

            using (SqlConnection objConn =
                new SqlConnection("SERVER =10.0.0.6; Database =decade; UID =jamie; PWD =jamie;"))
            {
                objConn.Open();
                objCmd = objConn.CreateCommand();
                objCmd.CommandText = String.Format("select ordernumber, invoicenumber from d_order where invoicedate = '{0}'",
                    refDate.ToShortDateString());

                using (SqlDataReader objReader = objCmd.ExecuteReader())
                {
                    if (objReader.HasRows)
                    {
                        while (objReader.Read())
                        {
                            CurrentInvoiceOrders.Add(
                                new ArchiveOrder(objReader["ordernumber"].ToString().Trim(),
                                    objReader["invoicenumber"].ToString().Trim()));
                        }
                    }
                }

            }
            Log.Append("Complete");
        }
        
        public static void PopulateOrdersByOrderNo(string orderNo)
        {
            CurrentInvoiceOrders = new List<ArchiveOrder>();

            SqlCommand objCmd = null;

            using (SqlConnection objConn =
                new SqlConnection("SERVER =10.0.0.6; Database =decade; UID =jamie; PWD =jamie;"))
            {
                objConn.Open();
                objCmd = objConn.CreateCommand();
                objCmd.CommandText = String.Format(
                    "select ordernumber, invoicenumber from d_order where ordernumber = '{0}'",
                    orderNo);

                using (SqlDataReader objReader = objCmd.ExecuteReader())
                {
                    if (objReader.HasRows)
                    {
                        while (objReader.Read())
                        {
                            CurrentInvoiceOrders.Add(
                                new ArchiveOrder(objReader["ordernumber"].ToString().Trim(),
                                    objReader["invoicenumber"].ToString().Trim()));
                        }
                    }
                }
            }

            Log.Append("Complete");
        }
        
        public static void PopulateOrdersByOrderDate(DateTime refDate = new DateTime())
        {
            if (refDate.Year < 2000)
                refDate = DateTime.Now;

            currentOrderDateSearch = refDate.ToShortDateString();

            Log.Append("Getting today's order list...");
            CurrentOrders = new List<DieOrder>();

            SqlCommand objCmd = null;

            using (SqlConnection objConn =
                new SqlConnection("SERVER =10.0.0.6; Database =decade; UID =jamie; PWD =jamie;"))
            {
                objConn.Open();
                objCmd = objConn.CreateCommand();
                objCmd.CommandText =
                    String.Format(
                        "select a.ordernumber, a.customercode, b.name, a.orderdate from d_order as a, d_customer as b where a.customercode = b.customercode and orderdate > '{0}' and orderdate < '{1}'",
                        refDate.ToShortDateString(), refDate.AddDays(1).ToShortDateString());

                using (SqlDataReader objReader = objCmd.ExecuteReader())
                {

                    if (objReader.HasRows)
                    {
                        while (objReader.Read())
                        {
                            CurrentOrders.Add(
                                new DieOrder(objReader["ordernumber"].ToString().Trim(),
                                    objReader["customercode"].ToString().Trim(),
                                    objReader["name"].ToString().Trim(),
                                    Convert.ToDateTime(objReader["orderdate"].ToString().Trim())
                                ));
                        }
                    }
                }
            }

            Log.Append("Complete");
        }
        
        public static void PopulateDieOrdersByOrderNo(string orderNo)
        {
            Log.Append("Getting today's order list...");
            CurrentOrders = new List<DieOrder>();
            
            SqlCommand objCmd = null;
            
            using (SqlConnection objConn =
                new SqlConnection("SERVER =10.0.0.6; Database =decade; UID =jamie; PWD =jamie;"))
            {
                objConn.Open();
                objCmd = objConn.CreateCommand();
                objCmd.CommandText =
                    String.Format(
                        "select a.ordernumber, a.customercode, b.name, a.orderdate from d_order as a, d_customer as b where a.customercode = b.customercode and a.ordernumber = '{0}'",
                        orderNo);

                using (SqlDataReader objReader = objCmd.ExecuteReader())
                {
                    if (objReader.HasRows)
                    {
                        while (objReader.Read())
                        {
                            CurrentOrders.Add(
                                new DieOrder(objReader["ordernumber"].ToString().Trim(),
                                    objReader["customercode"].ToString().Trim(),
                                    objReader["name"].ToString().Trim(),
                                    Convert.ToDateTime(objReader["orderdate"].ToString().Trim())
                                ));
                        }
                    }
                }
            }

            Log.Append("Complete");
        }

        /// <summary>
        /// Download entire archive from archive server and store in list
        /// </summary>
        public static void GetEntireArchive()
        {
            List<string> orderNumbers = Directory.GetDirectories(_archivePath).Select(x => x.Substring(x.Length - 6)).ToList();

            Archives = new List<ArchiveOrder>();

            foreach (string orderNumber in orderNumbers)
            {
                Archives.Add(new ArchiveOrder(orderNumber));
            }
        }

        public static List<string> GetFilesForOrder(string archiveOrderNo)
        {
            if (Directory.Exists(Path.Combine(_archivePath, archiveOrderNo)))
            {
                string archivePath = Path.Combine(_archivePath, archiveOrderNo);

                return Directory.GetFiles(archivePath)
                    .Select(x => Path.GetFileName(x))
                    .Where(y => !y.Contains(".invoice"))
                    .ToList();
            }
            return new List<string>();
        }

        public static string CreatePackage(string archiveOrderNo)
        {
            // Check if archive exists for ordernumber
            if (Directory.Exists(Path.Combine(_archivePath, archiveOrderNo)))
            {
                string archivePath = Path.Combine(_archivePath, archiveOrderNo);
                string packagePath = Path.Combine(_packagePath, archiveOrderNo);

                // Delete if package path exists
                if (Directory.Exists(packagePath))
                {
                    Directory.GetFiles(packagePath).ForEach(x => File.Delete(x)); // delete internal files
                    Directory.Delete(packagePath);
                }

                Directory.CreateDirectory(packagePath);

                int packageCount = 0;

                foreach (var filePath in Directory.GetFiles(archivePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    // Copy non-hidden files
                    if (!fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        File.Copy(filePath, Path.Combine(packagePath, Path.GetFileName(filePath)));
                        packageCount++;
                    }
                }

                string zipPath = Path.Combine(_packagePath, archiveOrderNo + ".zip");

                // Delete existing zip packaged file
                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                // Zip the files if more than 0 files
                if (packageCount > 0)
                    ZipFile.CreateFromDirectory(packagePath, zipPath, CompressionLevel.Optimal, true);


                // Delete if package after zip
                while (Directory.Exists(packagePath))
                {
                    try
                    {
                        Directory.GetFiles(packagePath).ForEach(x => File.Delete(x)); // delete internal files
                        Directory.Delete(packagePath);
                    }
                    catch
                    {
                    }
                }

                Log.Append(String.Format("Archive {0} downloaded by {1}", archiveOrderNo, HttpContext.Current.Session["Email"]));

                return zipPath;
            }

            return "";
        }
    }

    public class ArchiveOrder
    {
        public string _orderNo { get; }
        public string _invoiceNo { get; }
        public bool hasDieForm { get; }
        public bool hasOrderForm { get; }
        public bool hasInvoice { get; }

        public bool hasDrawings;
        public int miscScanItems;

        public DateTime InvoiceDate;

        public ArchiveOrder(string orderNo, string invoiceNo="")
        {
            _orderNo = orderNo;

            // Set invoice to given invoice number; if no invoice no provided, try to locate using invoice file
            _invoiceNo = invoiceNo;
            
            if (_invoiceNo == "" && Directory.Exists(Path.Combine(ArchivesChecker._archivePath, orderNo)))
            {
                string[] fileName = Directory.GetFiles(Path.Combine(ArchivesChecker._archivePath, orderNo),
                    "*.invoice");

                if (fileName.Length > 0)
                {
                    _invoiceNo = Path.GetFileNameWithoutExtension(fileName[0]);
                    string[] fileLines = File.ReadAllLines(fileName[0]);

                    if (fileLines.Length > 0)
                        DateTime.TryParse(fileLines[0], out InvoiceDate);
                }
            }

            // Validate files and toggle bool as per
            hasDieForm = File.Exists(Path.Combine(ArchivesChecker._archivePath + _orderNo, _orderNo + "_DIEFORM.eml")) ||
                            File.Exists(Path.Combine(ArchivesChecker._archivePath + _orderNo, _orderNo + "_DIEFORM.msg"));
            hasOrderForm = File.Exists(Path.Combine(ArchivesChecker._archivePath + _orderNo, _orderNo + "_ORDER.pdf"));
            hasInvoice = File.Exists(Path.Combine(ArchivesChecker._archivePath + _orderNo, _orderNo + "_INVOICE.pdf"));

            if (Directory.Exists(Path.Combine(ArchivesChecker._archivePath, orderNo)))
            {
                hasDrawings = Directory.GetFiles(Path.Combine(ArchivesChecker._archivePath, orderNo), "*.dwg").Length > 0;
                miscScanItems = Directory.GetFiles(Path.Combine(ArchivesChecker._archivePath, orderNo))
                                    .Where(name => !name.EndsWith(".invoice") && !name.EndsWith(".dwg")).ToList().Count -
                                (hasDieForm ? 1 : 0) -
                                (hasOrderForm ? 1 : 0) -
                                (hasInvoice ? 1 : 0);
            }
        }
    }

    public class DieOrder
    {
        public string _orderNo { get; }
        public string _custNo { get; }
        public string _custName { get; }
        public DateTime _orderDate { get; }
        public bool _hasFolder { get; set; }
        public List<string> _dieNumbers { get; set; }

        public DieOrder(string orderNo, string custNo, string custName, DateTime orderDate)
        {

            _orderNo = orderNo;
            _custNo = custNo;
            _custName = custName;
            _orderDate = orderDate;

            _hasFolder = File.Exists(Path.Combine(ArchivesChecker._archivePath + orderNo, orderNo+ "_DIEFORM.msg"));

            _dieNumbers = new List<string>();

            // Query die numbers
            SqlCommand objCmd = null;
            using (SqlConnection objConn =
                new SqlConnection("SERVER =10.0.0.6; Database =decade; UID =jamie; PWD =jamie;"))
            {
                objConn.Open();
                objCmd = objConn.CreateCommand();
                objCmd.CommandText = String.Format("select dienumber from d_orderitem where ordernumber = '{0}'",
                    orderNo);

                using (SqlDataReader objReader = objCmd.ExecuteReader())
                {
                    if (objReader.HasRows)
                    {
                        while (objReader.Read())
                        {
                            _dieNumbers.Add(objReader["dienumber"].ToString().Trim());
                        }
                    }
                }
            }

            // Remove duplicates
            _dieNumbers = _dieNumbers.Distinct().ToList();
        }

        public void SetHasFolder(bool hasFolder = false)
        {
            _hasFolder = hasFolder;
        }
    }
}