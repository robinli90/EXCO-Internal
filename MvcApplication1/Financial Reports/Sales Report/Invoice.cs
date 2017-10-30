using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExcoUtility;

namespace MvcApplication1.Financial_Reports.Sales_Report
{
    public class Invoice
    {
        // invoice number
        public int invoiceNum = 0;
        // order number
        public int orderNum = 0;
        // net sale
        public double sale = 0.0;
        // discount
        public double discount = 0.0;
        // fast track
        public double fastTrack = 0.0;
        // steel surcharge
        public double surcharge = 0.0;
        // customer
        public ExcoCustomer customer = new ExcoCustomer();
        // calendar
        public ExcoCalendar calendar = new ExcoCalendar();
        // currency
        public string currency = string.Empty;
        // territory
        public string territory = string.Empty;
        // plant id
        public int plant = 0;
        // credit note
        public char creditNote = ' ';

        // get invoice sales amount
        public ExcoMoney GetAmount(bool doesIncludeSurcharge)
        {
            double amount = 0.0;
            if (doesIncludeSurcharge)
            {
                amount = sale + discount + fastTrack + surcharge;
            }
            else
            {
                amount = sale + discount + fastTrack;
            }
            return new ExcoMoney(calendar, amount, currency);
        }
    }
}
