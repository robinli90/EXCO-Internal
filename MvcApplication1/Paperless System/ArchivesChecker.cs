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
using System.Text;
using System.Threading.Tasks;
using WebGrease.Css.Extensions;

namespace MvcApplication1.Paperless_System
{
    public static class ArchivesChecker
    {
        public static readonly string _archivePath = @"\\10.0.0.8\EmailAPI\Paperless\Archive\";
        public static readonly string _cachePath = @"\\10.0.0.8\Cache\";
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

            CreatePendingFile(orderNumber);

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
                new SqlConnection(Global.ConnectionStr))
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

        public static string GetOrderNumber(string invoiceNumber)
        {
            SqlCommand objCmd = null;
            using (SqlConnection objConn =
                new SqlConnection(Global.ConnectionStr))
            {
                objConn.Open();
                objCmd = objConn.CreateCommand();
                objCmd.CommandText = String.Format("select ordernumber from d_order where invoicenumber = '{0}'",
                    invoiceNumber);

                using (SqlDataReader objReader = objCmd.ExecuteReader())
                {
                    if (objReader.HasRows)
                    {
                        while (objReader.Read())
                        {
                            return objReader["ordernumber"].ToString().Trim();
                        }
                    }
                }
            }
            return "";
        }

        public static void ProcessCacheFiles()
        {
            foreach (string directoryPath in Directory.GetDirectories(_cachePath))
            {

                // Path should not be null
                if (directoryPath == null) continue;

                string orderNumber = Path.GetFileName(directoryPath);
                string archivePath = Path.Combine(_archivePath, orderNumber);

                Log.Append(String.Format("Transferring cache folder {0} to archive...", orderNumber));

                if (!Directory.Exists(archivePath))
                    Directory.CreateDirectory(archivePath);

                foreach (var filePath in Directory.GetFiles(directoryPath))
                {
                    File.Copy(filePath, Path.Combine(archivePath, Path.GetFileName(filePath)), true);
                }

                try
                {
                    // Delete all cache files
                    Directory.GetFiles(directoryPath).ForEach(x => File.Delete(x));

                    // Delete cache directory
                    Directory.Delete(directoryPath);

                    Log.Append(String.Format("Complete transfer to archive for {0}...", orderNumber));
                }
                catch (Exception e)
                {
                    Log.Append("Error: Cannot delete cache folder");
                }
            }
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
                new SqlConnection(Global.ConnectionStr))
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
                            string orderNumber = objReader["ordernumber"].ToString().Trim();

                            CurrentInvoiceOrders.Add(
                                new ArchiveOrder(orderNumber,
                                    objReader["invoicenumber"].ToString().Trim()));

                            CreatePendingFile(orderNumber);
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
                new SqlConnection(Global.ConnectionStr))
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

        public static void CreatePendingFile(string orderNumber)
        {
            string directoryPath = Path.Combine(_archivePath, orderNumber);
            if (!Directory.Exists(directoryPath) || !File.Exists(
                    Path.Combine(directoryPath,
                        String.Format("{0}_INVOICE.pdf", orderNumber)))
            )
            {
                Log.Append(String.Format("    Invoice pending file created for {0}", orderNumber));
                if (!File.Exists(_pendingPath + orderNumber))

                    File.CreateText(_pendingPath + orderNumber).Close();
            }
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
                new SqlConnection(Global.ConnectionStr))
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
                new SqlConnection(Global.ConnectionStr))
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
            // Threaded archival pull
            Task.Run(() =>
            {
                List<string> orderNumbers = Directory.GetDirectories(_archivePath)
                    .Select(x => x.Substring(x.Length - 6)).ToList();

                Archives = new List<ArchiveOrder>();

                foreach (string orderNumber in orderNumbers)
                {
                    Archives.Add(new ArchiveOrder(orderNumber));
                }
            });
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

        public static string CreateAuditPackage(string archiveInformation)
        {
            List<string> info = archiveInformation.Split(new string[] {"_"}, StringSplitOptions.None).ToList();

            bool needsEmails = info[0].Equals("true");
            bool needsInvoices = info[1].Equals("true");
            bool needsOrders = info[2].Equals("true");

            if (info.Count > 3)
            {
                int packageCount = 0;

                string folderName = String.Format("AuditPackage_{0}_{1}_{2}-{3}_{4}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Year, DateTime.Now.Hour,
                                    DateTime.Now.Minute);
                string rootPath = Path.Combine(_packagePath, folderName);

                // Create root directory
                Directory.CreateDirectory(rootPath);

                List<string> OrdersWithoutFiles = new List<string>() {"Invoices that are not found in the archive are listed below:"};

                // Get all the orders accordingly less the first 3 in list (because the first 3 is info)
                foreach (string invoiceNo in info.GetRange(3, info.Count - 3))
                {
                    // Append to list for time being (if not removed later, has no file)
                    if (invoiceNo.Length != 6) continue;

                    // Retrieve order number from invoice number
                    string order = GetOrderNumber(invoiceNo);

                    string archivePath = Path.Combine(_archivePath, order);

                    if (!Directory.Exists(archivePath) || order.Length != 6)
                    {
                        OrdersWithoutFiles.Add(invoiceNo);
                        continue;
                    }

                    string packagePath = Path.Combine(Path.Combine(_packagePath, folderName), order);
                    
                    // Create sub directories in rootPath
                    Directory.CreateDirectory(packagePath);

                    List<string> files = Directory.GetFiles(archivePath)
                        .Where(file => new string[] {".msg", ".pdf", ".eml"}
                            .Contains(Path.GetExtension(file)))
                        .ToList();

                    foreach (var filePath in files)
                    {
                        if ((filePath.Contains(".msg") || filePath.Contains(".eml")) && !needsEmails) continue;
                        if (filePath.Contains("_INVOICE.pdf") && !needsInvoices) continue;
                        if (filePath.Contains("_ORDER.pdf") && !needsOrders) continue;

                        var fileInfo = new FileInfo(filePath);
                        // Copy non-hidden files
                        if (!fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
                        {
                            File.Copy(filePath, Path.Combine(packagePath, Path.GetFileName(filePath)));
                            packageCount++;
                        }
                    }
                }

                // create file to show missing files
                if (OrdersWithoutFiles.Count > 1)
                {
                    StringBuilder str = new StringBuilder();
                    foreach (string orders in OrdersWithoutFiles)
                    {
                        str.Append(String.Format("{0}", orders +
                                                        Environment.NewLine));  
                    }
                    Saver.Save(str.ToString(), Path.Combine(rootPath, "missingFiles.txt"));
                }

                string zipPath = Path.Combine(_packagePath,
                    String.Format("AuditPackage_{0}_{1}_{2}-{3}_{4}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Year, DateTime.Now.Hour,
                        DateTime.Now.Minute) + ".zip");

                // Delete existing zip packaged file
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
                
                // Zip the files if more than 0 files
                if (packageCount > 0)
                {
                    ZipFile.CreateFromDirectory(rootPath, zipPath, CompressionLevel.Optimal, true);
                    Log.Append(String.Format("Audit Package downloaded by {0}",
                        HttpContext.Current.Session["Email"]));
                }

                return packageCount > 0 ? zipPath : "";
            }

            return "";
        }
    }
}