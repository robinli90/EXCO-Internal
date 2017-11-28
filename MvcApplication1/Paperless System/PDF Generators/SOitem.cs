using System.Collections.Generic;

namespace MvcApplication1.Paperless_System.PDF_Generators
{
    public class SOitem
    {
        public string dienumber = "";
        public string location = "";
        public string note = "";
        public string description = "";
        public int qty = 0;
        public double baseprice = 0.0;
        public double price = 0.0;
        public double steelcost = 0.0;
        public List<SOitemcharge> itemcharges = new List<SOitemcharge>();
    }
}