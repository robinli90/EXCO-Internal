using System;
using System.Collections.Generic;

namespace MvcApplication1.Paperless_System.PDF_Generators
{
    public class SO
    {
        public string ordernumber = "";
        public string invoicenumber = "";
        public string customerpo = "";
        public string designtype = "";
        public string shipvia = "";
        public string note = "";
        public DateTime invoicedate;
        public DateTime shopdate;
        public DateTime shipdate;
        public DateTime orderdate;
        public bool isSelected = false;
        public double total = 0.0;
        public double sales = 0.0;
        public double steelrate = 0.0;
        public double freight = 0.0;
        public double freightcharge = 0.0;
        public double freightweight = 0.0;
        public double discountamount = 0.0;
        public double subtotal = 0.0;
        public double fasttrackcharge = 0.0;
        public List<SOitem> orderitems = new List<SOitem>();
        public Dictionary<string, double> taxinfo = new Dictionary<string, double>();
    }
}