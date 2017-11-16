using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;

namespace MvcApplication1.Paperless_System.PDF_Generators
{
    public class PDFGenerator
    {
        public int page_width = 586;
        public int start_X = 22;
        public int start_Y = 20;
        public int row_height = 13;
        public int row_headerheight = 8;
        public int row_boxheight = 18;
        public int row_boxheight1 = 25;
        public int row_boxheight2 = 15;
        public int row_boxheight3 = 15;

        #region XFonts
        XFont font = new XFont("MS Reference Sans Serif", 10, XFontStyle.Bold);
        XFont font1 = new XFont("MS Reference Sans Serif", 7, XFontStyle.Bold);
        XFont font2 = new XFont("MS Reference Sans Serif", 7, XFontStyle.Regular);
        XFont font3 = new XFont("MS Reference Sans Serif", 11, XFontStyle.Bold);
        XFont font4 = new XFont("MS Reference Sans Serif", 11, XFontStyle.Regular);
        XFont font5 = new XFont("MS Reference Sans Serif", 20, XFontStyle.Bold);
        XFont font6 = new XFont("MS Reference Sans Serif", 10, XFontStyle.Regular);
        XFont font7 = new XFont("MS Reference Sans Serif", 10, XFontStyle.Bold);
        #endregion

        #region Invoice
        public void GenerateInvoice(string rootPath, string orderNumber)
        {
            Cust customer = new Cust(orderNumber);

            string fileName = Path.Combine(rootPath, orderNumber + "_INVOICE.pdf");

            // create secondary invoice file
            string invoiceName = Path.Combine(rootPath, customer.newSO.invoicenumber + ".invoice");

            if (File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                }
                catch { }
            }

            PdfDocument document = new PdfDocument();

            document.Info.Author = "EXCO";
            document.Info.Subject = "Invoice";
            document.Info.Title = "Exco Invoice";

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            DrawHeaderInvoice(gfx, page, customer.newSO);

            DrawAddressInvoice(gfx, page, customer);

            DrawDetailInvoice(gfx, page, customer);

            //save pdf file
            if (!File.Exists(fileName))
            {
                document.Save(fileName);
            }

            //save pdf file
            if (!File.Exists(invoiceName))
            {
                using (var tw = new StreamWriter(invoiceName, false))
                {
                    tw.WriteLine(customer.newSO.invoicedate);
                    tw.Close();
                }
                File.SetAttributes(invoiceName, File.GetAttributes(invoiceName) | FileAttributes.Hidden); // set as hidden
            }
        }
        
        private void DrawHeaderInvoice(XGraphics gfx, PdfPage page, SO curso)
        {
            
            XTextFormatter tf = new XTextFormatter(gfx);

            XImage image = XImage.FromGdiPlusImage(Image.FromFile(@"\\10.0.0.14\API\Images\Excologo.png"));
            gfx.DrawImage(image, start_X, start_Y - 10, 130, 40);

            XRect rect = new XRect(start_X, start_Y + 31, 100, row_headerheight);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("Exco Tooling Solutions", font1, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 105, start_Y + 31, 200, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("T. 905.477.1208 / 800.461.6298", font1, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X, start_Y + row_headerheight + 32, 100, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("130 Spy Court, 1st Floor", font1, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 105, start_Y + row_headerheight + 32, 200, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("F. 905.477.6304 / 877.336.3356", font1, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X, start_Y + row_headerheight * 2 + 33, 100, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("Markham, ON, L3R 5H6", font1, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 105, start_Y + row_headerheight * 2 + 33, 200, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("www.etsdies.com", font1, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 270, start_Y + 31, 38, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("Remit to:", font1, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 309, start_Y + 31, 120, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("Lockbox #15629, TD Bank Tower", font2, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 309, start_Y + row_headerheight + 32, 120, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("66 Wellington St. W., Suite 4500", font2, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 309, start_Y + row_headerheight * 2 + 33, 120, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("Toronto, Ontario M5K 1E7", font2, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 340, start_Y - 18, 120, 30);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("INVOICE", font5, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 470, start_Y - 18, 100, 30);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(curso.invoicenumber, font5, XBrushes.Black, rect, XStringFormats.TopLeft);

            XPen pen = new XPen(Color.Black, 0.5);
            int cur_Y = 35;
            gfx.DrawLine(pen, start_X + 470, cur_Y, start_X + 570, cur_Y);
            gfx.DrawLine(pen, start_X + 470, cur_Y + row_boxheight * 4, start_X + 570, cur_Y + row_boxheight * 4);
            gfx.DrawLine(pen, start_X + 470, cur_Y, start_X + 470, cur_Y + row_boxheight * 4);
            gfx.DrawLine(pen, start_X + 570, cur_Y, start_X + 570, cur_Y + row_boxheight * 4);
            gfx.DrawLine(pen, start_X + 470, cur_Y + row_boxheight, start_X + 570, cur_Y + row_boxheight);
            gfx.DrawLine(pen, start_X + 470, cur_Y + row_boxheight * 2, start_X + 570, cur_Y + row_boxheight * 2);
            gfx.DrawLine(pen, start_X + 470, cur_Y + row_boxheight * 3, start_X + 570, cur_Y + row_boxheight * 3);

            rect = new XRect(start_X + 471, cur_Y + 1, 98, row_boxheight - 2);
            gfx.DrawRectangle(XBrushes.LightGray, rect);

            rect = new XRect(start_X + 471, cur_Y + row_boxheight * 2 + 1, 98, row_boxheight - 2);
            gfx.DrawRectangle(XBrushes.LightGray, rect);

            rect = new XRect(start_X + 470, cur_Y + 2, 98, row_boxheight);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("INVOICE DATE", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 470, cur_Y + row_boxheight + 2, 98, row_boxheight);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(curso.invoicedate.ToShortDateString(), font4, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 470, cur_Y + row_boxheight * 2 + 2, 98, row_boxheight);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("SHIP DATE", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 470, cur_Y + row_boxheight * 3 + 2, 98, row_boxheight);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(curso.shipdate.ToShortDateString(), font4, XBrushes.Black, rect, XStringFormats.TopLeft);

        }

        private void DrawAddressInvoice(XGraphics gfx, PdfPage page, Cust curcust)
        {
            XTextFormatter tf = new XTextFormatter(gfx);

            int cur_Y = 110;

            XRect rect = new XRect(start_X, cur_Y, 50, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("Sold To:", font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 55, cur_Y, 175, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(curcust.custname, font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 55, cur_Y + row_height, 175, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(curcust.baddress1, font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 55, cur_Y + row_height * 2, 175, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(curcust.baddress2, font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 55, cur_Y + row_height * 3, 175, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(curcust.baddress3, font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 55, cur_Y + row_height * 4, 175, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(curcust.baddress4, font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 55, cur_Y + row_height * 5, 175, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(curcust.bpostalcode, font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 300, cur_Y, 50, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("Ship To:", font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 355, cur_Y, 175, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(curcust.custname, font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 355, cur_Y + row_height, 175, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(curcust.saddress1, font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 355, cur_Y + row_height * 2, 175, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(curcust.saddress2, font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 355, cur_Y + row_height * 3, 175, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(curcust.saddress3, font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 355, cur_Y + row_height * 4, 175, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(curcust.saddress4, font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 355, cur_Y + row_height * 5, 175, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(curcust.spostalcode, font6, XBrushes.Black, rect, XStringFormats.TopLeft);
        }

        private void DrawDetailInvoice(XGraphics gfx, PdfPage page, Cust curcust)
        {
            XTextFormatter tf = new XTextFormatter(gfx);

            int boxstart_Y = 190;
            int boxwidth = 565;
            int boxheight = 590;

            int itemgapheight = 10;

            int itemColwidth1 = 130;
            int itemColwidth2 = 235;
            int itemColwidth3 = 40;
            int itemColwidth4 = 80;
            int itemColwidth5 = 80;

            int cur_Y = 0;

            XRect rect;
            //outside bound
            XPen pen = new XPen(Color.Black, 0.5);
            gfx.DrawLine(pen, start_X, boxstart_Y, start_X + boxwidth, boxstart_Y);
            gfx.DrawLine(pen, start_X, boxstart_Y, start_X, boxstart_Y + boxheight);
            gfx.DrawLine(pen, start_X + boxwidth, boxstart_Y, start_X + boxwidth, boxstart_Y + boxheight);
            gfx.DrawLine(pen, start_X, boxstart_Y + boxheight, start_X + boxwidth, boxstart_Y + boxheight);
            gfx.DrawLine(pen, start_X, boxstart_Y + boxheight - row_boxheight, start_X + boxwidth, boxstart_Y + boxheight - row_boxheight);

            #region first line
            gfx.DrawLine(pen, start_X, boxstart_Y + row_boxheight, start_X + boxwidth, boxstart_Y + row_boxheight);
            rect = new XRect(start_X + 1, boxstart_Y + 1, boxwidth - 2, row_boxheight - 2);
            gfx.DrawRectangle(XBrushes.LightGray, rect);
            gfx.DrawLine(pen, start_X + 187, boxstart_Y, start_X + 187, boxstart_Y + row_boxheight + row_boxheight1);
            gfx.DrawLine(pen, start_X + 378, boxstart_Y, start_X + 378, boxstart_Y + row_boxheight + row_boxheight1);

            rect = new XRect(start_X + 1, boxstart_Y + 1, 187, row_boxheight - 2);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("PACKING SLIP", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 189, boxstart_Y + 1, 188, row_boxheight - 2);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("PURCHASE ORDER", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 378, boxstart_Y + 1, 186, row_boxheight - 2);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("TERMS", font3, XBrushes.Black, rect, XStringFormats.TopLeft);
            #endregion

            #region second line
            gfx.DrawLine(pen, start_X, boxstart_Y + row_boxheight + row_boxheight1, start_X + boxwidth, boxstart_Y + row_boxheight + row_boxheight1);

            rect = new XRect(start_X + 1, boxstart_Y + row_boxheight + 5, 187, 20);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(curcust.newSO.ordernumber, font4, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 189, boxstart_Y + row_boxheight + 5, 188, 20);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(curcust.newSO.customerpo, font4, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 378, boxstart_Y + row_boxheight + 5, 186, 20);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(curcust.terms, font4, XBrushes.Black, rect, XStringFormats.TopLeft);
            #endregion

            #region third line
            gfx.DrawLine(pen, start_X, boxstart_Y + row_boxheight * 2 + row_boxheight1, start_X + boxwidth, boxstart_Y + row_boxheight * 2 + row_boxheight1);
            rect = new XRect(start_X + 1, boxstart_Y + row_boxheight + row_boxheight1 + 1, boxwidth - 2, row_boxheight - 2);
            gfx.DrawRectangle(XBrushes.LightGray, rect);
            gfx.DrawLine(pen, start_X + itemColwidth1, boxstart_Y + row_boxheight + row_boxheight1, start_X + itemColwidth1, boxstart_Y + boxheight - row_boxheight);
            gfx.DrawLine(pen, start_X + itemColwidth1 + itemColwidth2, boxstart_Y + row_boxheight + row_boxheight1, start_X + itemColwidth1 + itemColwidth2, boxstart_Y + boxheight - row_boxheight);
            gfx.DrawLine(pen, start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, boxstart_Y + row_boxheight + row_boxheight1, start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, boxstart_Y + boxheight);
            gfx.DrawLine(pen, start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, boxstart_Y + row_boxheight + row_boxheight1, start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, boxstart_Y + boxheight);

            rect = new XRect(start_X + 1, boxstart_Y + row_boxheight + row_boxheight1 + 2, itemColwidth1 - 2, row_boxheight - 2);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("ITEM NUMBER", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + itemColwidth1, boxstart_Y + row_boxheight + row_boxheight1 + 2, itemColwidth2 - 2, row_boxheight - 2);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("DESCRIPTION", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + itemColwidth1 + itemColwidth2, boxstart_Y + row_boxheight + row_boxheight1 + 2, itemColwidth3 - 2, row_boxheight - 2);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("QTY.", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, boxstart_Y + row_boxheight + row_boxheight1 + 2, itemColwidth4 - 2, row_boxheight - 2);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("UNIT PRICE", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, boxstart_Y + row_boxheight + row_boxheight1 + 2, itemColwidth5 - 2, row_boxheight - 2);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("AMOUNT", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            #endregion

            #region order items
            cur_Y = boxstart_Y + row_boxheight * 2 + row_boxheight1 + itemgapheight;

            for (int i = 0; i < curcust.newSO.orderitems.Count; i++)
            {
                rect = new XRect(start_X, cur_Y, itemColwidth1, row_boxheight2);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString(curcust.newSO.orderitems[i].dienumber, font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(start_X + itemColwidth1 + 2, cur_Y, itemColwidth2, row_boxheight2);
                tf.Alignment = XParagraphAlignment.Left;
                tf.DrawString(curcust.newSO.orderitems[i].description, font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(start_X + itemColwidth1 + itemColwidth2, cur_Y, itemColwidth3, row_boxheight2);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString(curcust.newSO.orderitems[i].qty.ToString(), font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, cur_Y, itemColwidth4 - 5, row_boxheight2);
                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(curcust.newSO.orderitems[i].baseprice.ToString("C2"), font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, cur_Y, itemColwidth5 - 5, row_boxheight2);
                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString((curcust.newSO.orderitems[i].price * curcust.newSO.orderitems[i].qty).ToString("C2"), font6, XBrushes.Black, rect, XStringFormats.TopLeft);
                curcust.newSO.subtotal += curcust.newSO.orderitems[i].price * curcust.newSO.orderitems[i].qty;

                cur_Y += row_boxheight2;
                rect = new XRect(start_X + itemColwidth1 + 18, cur_Y, itemColwidth2 - 20, row_boxheight2);
                tf.Alignment = XParagraphAlignment.Left;
                tf.DrawString(String.Format("LOCATION: {0}", curcust.newSO.orderitems[i].location), font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                cur_Y += row_boxheight2;
                rect = new XRect(start_X + itemColwidth1 + 18, cur_Y, itemColwidth2 - 20, row_boxheight2);
                tf.Alignment = XParagraphAlignment.Left;
                tf.DrawString(String.Format("NOTE: {0}", curcust.newSO.orderitems[i].note), font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                if (curcust.newSO.orderitems[i].itemcharges.Count > 0)
                {
                    for (int j = 0; j < curcust.newSO.orderitems[i].itemcharges.Count; j++)
                    {
                        cur_Y += row_boxheight2;

                        rect = new XRect(start_X + itemColwidth1 + 30, cur_Y, itemColwidth2 - 32, row_boxheight2);
                        tf.Alignment = XParagraphAlignment.Left;
                        tf.DrawString(curcust.newSO.orderitems[i].itemcharges[j].chargename, font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                        if (!curcust.newSO.orderitems[i].itemcharges[j].chargename.Contains("NITRIDING"))
                        {
                            rect = new XRect(start_X + itemColwidth1 + itemColwidth2, cur_Y, itemColwidth3, row_boxheight2);
                            tf.Alignment = XParagraphAlignment.Center;
                            tf.DrawString(curcust.newSO.orderitems[i].itemcharges[j].qty.ToString(), font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                            rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, cur_Y, itemColwidth4 - 5, row_boxheight2);
                            tf.Alignment = XParagraphAlignment.Right;
                            tf.DrawString(curcust.newSO.orderitems[i].itemcharges[j].chargeprice.ToString("C2"), font6, XBrushes.Black, rect, XStringFormats.TopLeft);
                        }

                        rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, cur_Y, itemColwidth5 - 5, row_boxheight2);
                        tf.Alignment = XParagraphAlignment.Right;
                        tf.DrawString(curcust.newSO.orderitems[i].itemcharges[j].price.ToString("C2"), font6, XBrushes.Black, rect, XStringFormats.TopLeft);
                        curcust.newSO.subtotal += curcust.newSO.orderitems[i].itemcharges[j].price;
                    }
                }

                if (curcust.newSO.orderitems[i].steelcost != 0)
                {
                    cur_Y += row_boxheight2;
                    rect = new XRect(start_X + itemColwidth1 + 30, cur_Y, itemColwidth2 - 35, row_boxheight2);
                    tf.Alignment = XParagraphAlignment.Left;
                    tf.DrawString("STEEL SURCHARGE", font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, cur_Y, itemColwidth5 - 5, row_boxheight2);
                    tf.Alignment = XParagraphAlignment.Right;
                    tf.DrawString(curcust.newSO.orderitems[i].steelcost.ToString("C2"), font6, XBrushes.Black, rect, XStringFormats.TopLeft);
                    curcust.newSO.subtotal += curcust.newSO.orderitems[i].steelcost;
                }



                cur_Y += row_boxheight2 * 2;
            }


            #endregion

            #region tax info
            int reverse_Y = boxstart_Y + boxheight - row_boxheight;
            double gst = 0.0;
            foreach (var item in curcust.newSO.taxinfo.Reverse())
            {

                rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + 1, reverse_Y - row_boxheight, itemColwidth4 + itemColwidth5 - 2, row_boxheight - 1);
                gfx.DrawRectangle(XBrushes.White, rect);
                gfx.DrawLine(pen, start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, reverse_Y - row_boxheight, start_X + boxwidth, reverse_Y - row_boxheight);

                rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, reverse_Y - row_boxheight + 1, itemColwidth4 - 5, row_boxheight);
                tf.Alignment = XParagraphAlignment.Left;
                tf.DrawString(" " + item.Key.ToString(), font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, reverse_Y - row_boxheight + 1, itemColwidth5 - 5, row_boxheight);
                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(item.Value.ToString("C2"), font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                gst += item.Value;

                reverse_Y -= row_boxheight;
            }
            #endregion

            #region extra info

            //subtotal
            curcust.newSO.subtotal += curcust.newSO.fasttrackcharge - curcust.newSO.discountamount + curcust.newSO.freight;
            rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + 1, reverse_Y - row_boxheight, itemColwidth4 + itemColwidth5 - 2, row_boxheight - 1);
            gfx.DrawRectangle(XBrushes.White, rect);
            gfx.DrawLine(pen, start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, reverse_Y - row_boxheight, start_X + boxwidth, reverse_Y - row_boxheight);

            rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, reverse_Y - row_boxheight + 1, itemColwidth4 - 5, row_boxheight);
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(" SUB TOTAL", font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, reverse_Y - row_boxheight + 1, itemColwidth5 - 5, row_boxheight);
            tf.Alignment = XParagraphAlignment.Right;
            tf.DrawString(curcust.newSO.subtotal.ToString("C2"), font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            reverse_Y -= row_boxheight;

            //discount amount
            if (curcust.newSO.discountamount > 0)
            {
                rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + 1, reverse_Y - row_boxheight, itemColwidth4 + itemColwidth5 - 2, row_boxheight - 1);
                gfx.DrawRectangle(XBrushes.White, rect);
                gfx.DrawLine(pen, start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, reverse_Y - row_boxheight, start_X + boxwidth, reverse_Y - row_boxheight);

                rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, reverse_Y - row_boxheight + 1, itemColwidth4 - 5, row_boxheight);
                tf.Alignment = XParagraphAlignment.Left;
                tf.DrawString(" DISCOUNT", font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, reverse_Y - row_boxheight + 1, itemColwidth5 - 5, row_boxheight);
                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(String.Format("({0})", curcust.newSO.discountamount.ToString("C2")), font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                reverse_Y -= row_boxheight;
            }

            //fasttrack
            if (curcust.newSO.fasttrackcharge != 0)
            {
                rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + 1, reverse_Y - row_boxheight, itemColwidth4 + itemColwidth5 - 2, row_boxheight - 1);
                gfx.DrawRectangle(XBrushes.White, rect);
                gfx.DrawLine(pen, start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, reverse_Y - row_boxheight, start_X + boxwidth, reverse_Y - row_boxheight);

                rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, reverse_Y - row_boxheight + 1, itemColwidth4 - 5, row_boxheight);
                tf.Alignment = XParagraphAlignment.Left;
                tf.DrawString(" FASTTRACK", font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, reverse_Y - row_boxheight + 1, itemColwidth5 - 5, row_boxheight);
                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(curcust.newSO.fasttrackcharge.ToString("C2"), font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                reverse_Y -= row_boxheight;
            }


            //freight
            if (curcust.newSO.freight != 0)
            {
                rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + 1, reverse_Y - row_boxheight, itemColwidth4 + itemColwidth5 - 2, row_boxheight - 1);
                gfx.DrawRectangle(XBrushes.White, rect);
                gfx.DrawLine(pen, start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, reverse_Y - row_boxheight, start_X + boxwidth, reverse_Y - row_boxheight);

                rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, reverse_Y - row_boxheight + 1, itemColwidth4 - 5, row_boxheight);
                tf.Alignment = XParagraphAlignment.Left;
                tf.DrawString(" FREIGHT", font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, reverse_Y - row_boxheight + 1, itemColwidth5 - 5, row_boxheight);
                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(curcust.newSO.freight.ToString("C2"), font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                reverse_Y -= row_boxheight;
            }

            #endregion

            #region last line
            gfx.DrawLine(pen, start_X + 175, boxstart_Y + boxheight - row_boxheight, start_X + 175, boxstart_Y + boxheight);

            rect = new XRect(start_X + 1, boxstart_Y + boxheight - row_boxheight + 1, 173, row_boxheight);
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(String.Format(" CUSTOMER NO.     {0}", curcust.custcode), font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 177, boxstart_Y + boxheight - row_boxheight + 1, 220, row_boxheight);
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(" HST#: 101714533", font6, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + 1, boxstart_Y + boxheight - row_boxheight + 1, itemColwidth4 + itemColwidth5 - 2, row_boxheight - 1);
            gfx.DrawRectangle(XBrushes.White, rect);
            gfx.DrawLine(pen, start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, boxstart_Y + boxheight, start_X + boxwidth, boxstart_Y + boxheight);

            rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, boxstart_Y + boxheight - row_boxheight + 1, itemColwidth4 - 5, row_boxheight);
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(" " + curcust.accountset, font, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, boxstart_Y + boxheight - row_boxheight + 1, itemColwidth5 - 5, row_boxheight);
            tf.Alignment = XParagraphAlignment.Right;
            tf.DrawString(curcust.newSO.total.ToString("C2"), font, XBrushes.Black, rect, XStringFormats.TopLeft);

            #endregion
        }

        #endregion
        
        #region Order

        public void GenerateOrder(string rootPath, string orderNumber)
        {
            Cust customer = new Cust(orderNumber);

            string fileName = Path.Combine(rootPath, orderNumber + "_ORDER.pdf");

            if (File.Exists(fileName)) {
                try {
                    File.Delete(fileName);
                } catch { }
            }

            PdfDocument document = new PdfDocument();

            document.Info.Author = "EXCO";
            document.Info.Subject = "Invoice";
            document.Info.Title = "Exco Invoice";

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            DrawHeaderOrder(gfx, page, customer.newSO);

            DrawAddressOrder(gfx, page, customer);

            DrawDetailOrder(gfx, page, customer);

            //save pdf file
            if (!File.Exists(fileName))
            {
                document.Save(fileName);
            }
        }

        private void DrawHeaderOrder(XGraphics gfx, PdfPage page, SO curso)
        {
            
            XTextFormatter tf = new XTextFormatter(gfx);
            XRect rect = new XRect(start_X, start_Y + 31, 100, row_headerheight);
            /*
            XImage image = XImage.FromGdiPlusImage(Image.FromFile(@"\\10.0.0.14\API\Images\Excologo.png"));
            gfx.DrawImage(image, start_X, start_Y - 10, 130, 40);

            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("Exco Tooling Solutions", font1, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 105, start_Y + 31, 200, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("T. 905.477.1208 / 800.461.6298", font1, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X, start_Y + row_headerheight + 32, 100, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("130 Spy Court, 1st Floor", font1, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 105, start_Y + row_headerheight + 32, 200, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("F. 905.477.6304 / 877.336.3356", font1, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X, start_Y + row_headerheight * 2 + 33, 100, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("Markham, ON, L3R 5H6", font1, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 105, start_Y + row_headerheight * 2 + 33, 200, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("www.etsdies.com", font1, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 270, start_Y + 31, 38, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("Remit to:", font1, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 309, start_Y + 31, 120, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("Lockbox #15629, TD Bank Tower", font2, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 309, start_Y + row_headerheight + 32, 120, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("66 Wellington St. W., Suite 4500", font2, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 309, start_Y + row_headerheight * 2 + 33, 120, 8);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("Toronto, Ontario M5K 1E7", font2, XBrushes.Black, rect, XStringFormats.TopLeft);
            */
            rect = new XRect(start_X + 340, start_Y - 18, 120, 30);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString("ORDER", font5, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 470, start_Y - 18, 100, 30);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(curso.ordernumber, font5, XBrushes.Black, rect, XStringFormats.TopLeft);

            XPen pen = new XPen(Color.Black, 0.5);
            int cur_Y = 35;
            gfx.DrawLine(pen, start_X + 470, cur_Y, start_X + 570, cur_Y);
            gfx.DrawLine(pen, start_X + 470, cur_Y + row_boxheight * 2, start_X + 570, cur_Y + row_boxheight * 2);
            gfx.DrawLine(pen, start_X + 470, cur_Y, start_X + 470, cur_Y + row_boxheight * 2);
            gfx.DrawLine(pen, start_X + 570, cur_Y, start_X + 570, cur_Y + row_boxheight * 2);
            gfx.DrawLine(pen, start_X + 470, cur_Y + row_boxheight, start_X + 570, cur_Y + row_boxheight);

            rect = new XRect(start_X + 471, cur_Y + 1, 98, row_boxheight - 2);
            gfx.DrawRectangle(XBrushes.LightGray, rect);

            //rect = new XRect(start_X + 471, cur_Y + row_boxheight * 2 + 1, 98, row_boxheight - 2);
            //gfx.DrawRectangle(XBrushes.LightGray, rect);

            rect = new XRect(start_X + 470, cur_Y + 2, 98, row_boxheight);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("ORDER DATE", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 470, cur_Y + row_boxheight + 2, 98, row_boxheight);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(curso.orderdate.ToShortDateString(), font4, XBrushes.Black, rect, XStringFormats.TopLeft);
            
        }

        private void DrawAddressOrder(XGraphics gfx, PdfPage page, Cust curcust)
        {
            XTextFormatter tf = new XTextFormatter(gfx);

            int cur_Y = start_Y - 10;

            XRect rect = new XRect(start_X, cur_Y, 50, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString("CUSTOMER:", font7, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 70, cur_Y, 175, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(curcust.custname, font6, XBrushes.Black, rect, XStringFormats.TopLeft);
            
            rect = new XRect(start_X, cur_Y + row_height, 50, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString("DESIGN:", font7, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 70, cur_Y + row_height, 175, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(curcust.newSO.designtype, font6, XBrushes.Black, rect, XStringFormats.TopLeft);
            
            rect = new XRect(start_X, cur_Y + row_height * 2, 50, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString("NOTES:", font7, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 70, cur_Y + row_height * 2, 175, row_height);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(curcust.newSO.note, font6, XBrushes.Black, rect, XStringFormats.TopLeft);
            

        }

        private void DrawDetailOrder(XGraphics gfx, PdfPage page, Cust curcust)
        {
            XTextFormatter tf = new XTextFormatter(gfx);

            int boxstart_Y = 100;
            int boxwidth = 565;
            int boxheight = 590;

            int itemgapheight = 10;

            int itemColwidth1 = 130;
            int itemColwidth2 = 350;
            int itemColwidth3 = 0;
            int itemColwidth4 = 0;
            int itemColwidth5 = 80;

            int cur_Y = 0;

            XRect rect;
            //outside bound
            XPen pen = new XPen(Color.Black, 0.5);
            gfx.DrawLine(pen, start_X, boxstart_Y, start_X + boxwidth, boxstart_Y);
            gfx.DrawLine(pen, start_X, boxstart_Y, start_X, boxstart_Y + boxheight);

            gfx.DrawLine(pen, start_X + boxwidth, boxstart_Y, start_X + boxwidth, boxstart_Y + boxheight);
            gfx.DrawLine(pen, start_X, boxstart_Y + boxheight, start_X + boxwidth, boxstart_Y + boxheight);
            //gfx.DrawLine(pen, start_X, boxstart_Y + boxheight - row_boxheight, start_X + boxwidth, boxstart_Y + boxheight - row_boxheight);

            #region first line
            gfx.DrawLine(pen, start_X, boxstart_Y + row_boxheight, start_X + boxwidth, boxstart_Y + row_boxheight);
            rect = new XRect(start_X + 1, boxstart_Y + 1, boxwidth - 2, row_boxheight - 2);
            gfx.DrawRectangle(XBrushes.LightGray, rect);
            gfx.DrawLine(pen, start_X + 80, boxstart_Y, start_X + 80, boxstart_Y + row_boxheight + row_boxheight1);
            gfx.DrawLine(pen, start_X + 240, boxstart_Y, start_X + 240, boxstart_Y + row_boxheight + row_boxheight1);
            gfx.DrawLine(pen, start_X + 320, boxstart_Y, start_X + 320, boxstart_Y + row_boxheight + row_boxheight1);

            rect = new XRect(start_X + 1, boxstart_Y + 1, 80, row_boxheight - 2);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("CUST NO.", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 81, boxstart_Y + 1, 160, row_boxheight - 2);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("PURCHASE ORDER", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 241, boxstart_Y + 1, 80, row_boxheight - 2);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("SHOPDATE", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 321, boxstart_Y + 1, 220, row_boxheight - 2);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("SHIP VIA", font3, XBrushes.Black, rect, XStringFormats.TopLeft);
            #endregion

            #region second line
            gfx.DrawLine(pen, start_X, boxstart_Y + row_boxheight + row_boxheight1, start_X + boxwidth, boxstart_Y + row_boxheight + row_boxheight1);

            rect = new XRect(start_X + 1, boxstart_Y + row_boxheight + 5, 80, 20);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(curcust.newSO.ordernumber, font4, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 81, boxstart_Y + row_boxheight + 5, 160, 20);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(curcust.newSO.customerpo, font4, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 241, boxstart_Y + row_boxheight + 5, 80, 20);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(curcust.newSO.shopdate.ToShortDateString(), font4, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + 321, boxstart_Y + row_boxheight + 5, 220, 20);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(curcust.newSO.shipvia, font4, XBrushes.Black, rect, XStringFormats.TopLeft);
            #endregion

            #region third line
            gfx.DrawLine(pen, start_X, boxstart_Y + row_boxheight * 2 + row_boxheight1, start_X + boxwidth, boxstart_Y + row_boxheight * 2 + row_boxheight1);
            rect = new XRect(start_X + 1, boxstart_Y + row_boxheight + row_boxheight1 + 1, boxwidth - 2, row_boxheight - 2);
            gfx.DrawRectangle(XBrushes.LightGray, rect);
            gfx.DrawLine(pen, start_X + itemColwidth1, boxstart_Y + row_boxheight + row_boxheight1, start_X + itemColwidth1, boxstart_Y + boxheight );
            gfx.DrawLine(pen, start_X + itemColwidth1 + itemColwidth2, boxstart_Y + row_boxheight + row_boxheight1, start_X + itemColwidth1 + itemColwidth2, boxstart_Y + boxheight );
            gfx.DrawLine(pen, start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, boxstart_Y + row_boxheight + row_boxheight1, start_X + itemColwidth1 + itemColwidth2 + itemColwidth3, boxstart_Y + boxheight);
            gfx.DrawLine(pen, start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, boxstart_Y + row_boxheight + row_boxheight1, start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, boxstart_Y + boxheight);

            rect = new XRect(start_X + 1, boxstart_Y + row_boxheight + row_boxheight1 + 2, itemColwidth1 - 2, row_boxheight - 2);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("ITEM NUMBER", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + itemColwidth1, boxstart_Y + row_boxheight + row_boxheight1 + 2, itemColwidth2 - 2, row_boxheight - 2);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("DESCRIPTION", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, boxstart_Y + row_boxheight + row_boxheight1 + 2, itemColwidth5 - 2, row_boxheight - 2);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("QUANTITY", font3, XBrushes.Black, rect, XStringFormats.TopLeft);

            #endregion

            #region order items
            cur_Y = boxstart_Y + row_boxheight * 2 + row_boxheight1 + itemgapheight;

            for (int i = 0; i < curcust.newSO.orderitems.Count; i++)
            {
                rect = new XRect(start_X, cur_Y, itemColwidth1, row_boxheight2);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString(curcust.newSO.orderitems[i].dienumber, font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(start_X + itemColwidth1 + 2, cur_Y, itemColwidth2, row_boxheight2);
                tf.Alignment = XParagraphAlignment.Left;
                tf.DrawString(curcust.newSO.orderitems[i].description, font6, XBrushes.Black, rect, XStringFormats.TopLeft);
                
                rect = new XRect(start_X + itemColwidth1 + itemColwidth2 + itemColwidth3 + itemColwidth4, cur_Y, itemColwidth5 - 2, row_boxheight2);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString(curcust.newSO.orderitems[i].qty.ToString(), font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                cur_Y += row_boxheight2;
                rect = new XRect(start_X + itemColwidth1 + 18, cur_Y, itemColwidth2 - 20, row_boxheight2);
                tf.Alignment = XParagraphAlignment.Left;
                tf.DrawString(String.Format("LOCATION: {0}", curcust.newSO.orderitems[i].location), font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                cur_Y += row_boxheight2;
                rect = new XRect(start_X + itemColwidth1 + 18, cur_Y, itemColwidth2 - 20, row_boxheight2);
                tf.Alignment = XParagraphAlignment.Left;
                tf.DrawString(String.Format("NOTE: {0}", curcust.newSO.orderitems[i].note), font6, XBrushes.Black, rect, XStringFormats.TopLeft);

                if (curcust.newSO.orderitems[i].itemcharges.Count > 0)
                {
                    for (int j = 0; j < curcust.newSO.orderitems[i].itemcharges.Count; j++)
                    {
                        cur_Y += row_boxheight2;

                        rect = new XRect(start_X + itemColwidth1 + 30, cur_Y, itemColwidth2 - 32, row_boxheight2);
                        tf.Alignment = XParagraphAlignment.Left;
                        tf.DrawString(curcust.newSO.orderitems[i].itemcharges[j].chargename, font6, XBrushes.Black, rect, XStringFormats.TopLeft);
                    }
                }

                if (curcust.newSO.orderitems[i].steelcost != 0)
                {
                    cur_Y += row_boxheight2;
                    rect = new XRect(start_X + itemColwidth1 + 30, cur_Y, itemColwidth2 - 35, row_boxheight2);
                    tf.Alignment = XParagraphAlignment.Left;
                    tf.DrawString("STEEL SURCHARGE", font6, XBrushes.Black, rect, XStringFormats.TopLeft);
                }



                cur_Y += row_boxheight2 * 2;
            }


            #endregion
        }
        
        #endregion
    }
}

