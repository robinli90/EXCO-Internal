using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MvcApplication1.Paperless_System.PDF_Generators
{
    public class Cust
    {
        public string custname = "";
        public string ordernumber = "";
        public string custcode = "";
        public string baddress1 = "";
        public string baddress2 = "";
        public string baddress3 = "";
        public string baddress4 = "";
        public string bpostalcode = "";
        public string saddress1 = "";
        public string saddress2 = "";
        public string saddress3 = "";
        public string saddress4 = "";
        public string spostalcode = "";
        public string terms = "";
        public string accountset = "";
        public bool custselected = false;
        public string taxgroup = "";
        public string taxcode = "";

        public SO newSO;

        public Cust(string orderNo)
        {
            ordernumber = orderNo;
            
            SqlCommand objCmd = null;

            using (SqlConnection objConn =
                new SqlConnection(Global.ConnectionStr))
            {
                objConn.Open();
                objCmd = objConn.CreateCommand();
                objCmd.CommandText = String.Format("select * from d_order where ordernumber = '{0}'", orderNo);

                using (SqlDataReader objReader = objCmd.ExecuteReader())
                {

                    if (objReader.HasRows)
                    {
                        while (objReader.Read())
                        {
                            custcode = objReader["customercode"].ToString().Trim();
                        }
                    }
                }
            }

            if (custcode.Length > 3)
            {
                GetCustInfo();
            }
        }


        private void GetCustInfo()
        {
            SqlCommand objCmd = null;

            using (SqlConnection objConn = new SqlConnection(Global.ConnectionStr))
            {
                objConn.Open();
                objCmd = objConn.CreateCommand();
                objCmd.CommandText = String.Format(
                    "select a.customercode,a.name,b.name as aname,b.address1,b.address2,b.address3,b.address4,b.postalcode "
                    + " from d_customer as a, d_customeraddress as b where a.customercode=b.customercode and a.customercode='{0}'",
                    custcode);

                using (SqlDataReader objReader = objCmd.ExecuteReader())
                {
                    if (objReader.HasRows)
                    {
                        while (objReader.Read())
                        {
                            if (objReader["aname"].ToString().Trim() == "SHIPPING")
                            {
                                saddress1 = objReader["address1"].ToString().Trim();
                                saddress2 = objReader["address2"].ToString().Trim();
                                saddress3 = objReader["address3"].ToString().Trim();
                                saddress4 = objReader["address4"].ToString().Trim();
                                spostalcode = objReader["postalcode"].ToString().Trim();
                            }
                            else if (objReader["aname"].ToString().Trim() == "BILLING")
                            {
                                baddress1 = objReader["address1"].ToString().Trim();
                                baddress2 = objReader["address2"].ToString().Trim();
                                baddress3 = objReader["address3"].ToString().Trim();
                                baddress4 = objReader["address4"].ToString().Trim();
                                bpostalcode = objReader["postalcode"].ToString().Trim();
                            }
                        }
                    }
                }
            }

            using (SqlConnection objConn =
                new SqlConnection(Global.ConnectionStr))
            {
                objConn.Open();
                objCmd = objConn.CreateCommand();
                objCmd.CommandText = String.Format(
                    "select a.ordernumber,a.customercode,b.name,b.terms,a.invoicenumber,a.invoicedate,a.shipdate,a.sales,"
                    + " a.fasttrackcharge,a.discountamount,a.orderdate,a.note,a.designtype,a.freight,a.freightweight,a.freightcharge,a.customerpo,a.shopdate,b.accountset,a.total,a.steelrate,a.shipvia,b.taxgroup,b.taxcode "
                    + " from d_order as a, d_customer as b "
                    + " where a.customercode = b.customercode "
                    + " and a.ordernumber = '{0}' "
                    //+ " and (a.customerpo not like '%CANCEL%' or a.customerpo is null) "
                    + " order by b.name, a.ordernumber", ordernumber);

                using (SqlDataReader objReader = objCmd.ExecuteReader())
                {
                    if (objReader.HasRows)
                    {
                        while (objReader.Read())
                        {
                            custcode = objReader["customercode"].ToString().Trim();
                            custname = objReader["name"].ToString().Trim();
                            terms = objReader["terms"].ToString().Trim();
                            accountset = objReader["accountset"].ToString().Trim();
                            taxgroup = objReader["taxgroup"].ToString().Trim();
                            taxcode = objReader["taxcode"].ToString().Trim();
                            newSO = new SO();
                            newSO.ordernumber = objReader["ordernumber"].ToString().Trim();
                            newSO.shipvia = objReader["shipvia"].ToString().Trim();
                            newSO.note = objReader["note"].ToString().Trim();
                            newSO.designtype = GetDesignType(objReader["designtype"].ToString().Trim());
                            newSO.invoicenumber = objReader["invoicenumber"].ToString().Trim();
                            newSO.invoicedate = Convert.ToDateTime(objReader["invoicedate"].ToString());
                            newSO.shipdate = Convert.ToDateTime(objReader["shipdate"].ToString());
                            newSO.orderdate = Convert.ToDateTime(objReader["orderdate"].ToString());
                            newSO.shopdate = Convert.ToDateTime(objReader["shopdate"].ToString());
                            newSO.customerpo = objReader["customerpo"].ToString().Trim();
                            newSO.total = Convert.ToDouble(objReader["total"]);
                            newSO.sales = Convert.ToDouble(objReader["sales"]);
                            newSO.steelrate = Convert.ToDouble(objReader["steelrate"]);
                            newSO.freight = Convert.ToDouble(objReader["freight"]);
                            newSO.freightweight = Convert.ToDouble(objReader["freightweight"]);
                            newSO.freightcharge = Convert.ToDouble(objReader["freightcharge"]);
                            newSO.discountamount = Convert.ToDouble(objReader["discountamount"]);
                            newSO.fasttrackcharge = Convert.ToDouble(objReader["fasttrackcharge"]);
                            GetSOInfo();
                            GetTaxInfo();
                        }
                    }
                }
            }
        }

        private string GetDesignType(string numericValue)
        {
            switch (numericValue)
            {
                case "1":
                    return "NEW";
                case "2":
                    return "REPEAT";
                case "3":
                    return "REDESIGN";
                default:
                    return "REDESIGN";
            }
        }

        private void GetSOInfo()
        {
            SqlCommand objCmd = null;

            using (SqlConnection objConn =
                new SqlConnection(Global.ConnectionStr))
            {
                objConn.Open();
                objCmd = objConn.CreateCommand();
                objCmd.CommandText = String.Format(
                    "select a.ordernumber,a.line,a.qty,a.description,a.steelcost,a.location,a.dienumber,a.note,a.price,a.baseprice,b.line as line2,b.chargename,b.chargeprice,b.qty " +
                    "as qty2,b.price as price2 from d_orderitem as a  "
                    + " left join d_orderitemcharges as b on a.ordernumber=b.ordernumber and a.line=b.line where a.ordernumber='{0}' order by a.line, b.chargeline",
                    newSO.ordernumber);


                using (SqlDataReader objReader = objCmd.ExecuteReader())
                {
                    int curline = 0;
                    int i = 0;

                    if (objReader.HasRows)
                    {
                        while (objReader.Read())
                        {
                            if (i == 0)
                            {
                                SOitem tempItem = new SOitem();
                                tempItem.description = objReader["description"].ToString().Trim();
                                tempItem.dienumber = objReader["dienumber"].ToString().Trim();
                                tempItem.note = objReader["note"].ToString().Trim();
                                tempItem.location = objReader["location"].ToString().Trim();
                                tempItem.qty = Convert.ToInt32(objReader["qty"]);
                                tempItem.baseprice = Convert.ToDouble(objReader["baseprice"]);
                                tempItem.price = Convert.ToDouble(objReader["price"]);
                                tempItem.steelcost = objReader["steelcost"] != DBNull.Value
                                    ? Convert.ToDouble(objReader["steelcost"])
                                    : 0.0;
                                if (objReader["line2"] != DBNull.Value)
                                {
                                    SOitemcharge tempItemcharge = new SOitemcharge();
                                    tempItemcharge.chargename = objReader["chargename"].ToString().Trim();
                                    tempItemcharge.qty = Convert.ToInt32(objReader["qty2"]);
                                    tempItemcharge.price = Convert.ToDouble(objReader["price2"]);
                                    tempItemcharge.chargeprice = Convert.ToDouble(objReader["chargeprice"]);
                                    tempItem.itemcharges.Add(tempItemcharge);
                                }
                                newSO.orderitems.Add(tempItem);
                                curline = Convert.ToInt32(objReader["line"]);

                            }
                            else
                            {
                                if (curline == Convert.ToInt32(objReader["line"]))
                                {
                                    SOitemcharge tempItemcharge = new SOitemcharge();
                                    tempItemcharge.chargename = objReader["chargename"].ToString().Trim();
                                    tempItemcharge.qty = Convert.ToInt32(objReader["qty2"]);
                                    tempItemcharge.price = Convert.ToDouble(objReader["price2"]);
                                    tempItemcharge.chargeprice = Convert.ToDouble(objReader["chargeprice"]);
                                    newSO.orderitems[newSO.orderitems.Count - 1].itemcharges.Add(tempItemcharge);
                                }
                                else
                                {
                                    SOitem tempItem = new SOitem();
                                    tempItem.description = objReader["description"].ToString().Trim();
                                    tempItem.dienumber = objReader["dienumber"].ToString().Trim();
                                    tempItem.note = objReader["note"].ToString().Trim();
                                    tempItem.location = objReader["location"].ToString().Trim();
                                    tempItem.qty = Convert.ToInt32(objReader["qty"]);
                                    tempItem.baseprice = Convert.ToDouble(objReader["baseprice"]);
                                    tempItem.price = Convert.ToDouble(objReader["price"]);
                                    tempItem.steelcost = objReader["steelcost"] != DBNull.Value
                                        ? Convert.ToDouble(objReader["steelcost"])
                                        : 0.0;
                                    if (objReader["line2"] != DBNull.Value)
                                    {
                                        SOitemcharge tempItemcharge = new SOitemcharge();
                                        tempItemcharge.chargename = objReader["chargename"].ToString().Trim();
                                        tempItemcharge.qty = Convert.ToInt32(objReader["qty2"]);
                                        tempItemcharge.price = Convert.ToDouble(objReader["price2"]);
                                        tempItemcharge.chargeprice = Convert.ToDouble(objReader["chargeprice"]);
                                        tempItem.itemcharges.Add(tempItemcharge);
                                    }
                                    newSO.orderitems.Add(tempItem);
                                    curline = Convert.ToInt32(objReader["line"]);
                                }
                            }
                            i++;
                        }
                    }
                }
            }
        }

        private void GetTaxInfo()
        {
            SqlCommand objCmd = null;

            using (SqlConnection objConn =
                new SqlConnection(Global.ConnectionStr))
            {
                objConn.Open();
                objCmd = objConn.CreateCommand();
                objCmd.CommandText = String.Format(
                    "select a.ordernumber,a.taxline,b.description,a.taxamount from d_ordertaxes as a,d_taxinfo as b where a.ordernumber='{0}' "
                    + " and a.taxtype=b.taxtype and b.taxgroup='{1}' and b.taxratecode='{2}' order by a.taxline",
                    ordernumber, taxgroup, taxcode);

                using (SqlDataReader objReader = objCmd.ExecuteReader())
                {
                    if (objReader.HasRows)
                    {
                        while (objReader.Read())
                        {
                            newSO.taxinfo.Add(objReader["description"].ToString().Trim(),
                                Convert.ToDouble(objReader["taxamount"]));
                        }
                    }
                }
            }
        }
    }
}