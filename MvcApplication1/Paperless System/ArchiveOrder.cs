using System;
using System.IO;
using System.Linq;

namespace MvcApplication1.Paperless_System
{
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
}