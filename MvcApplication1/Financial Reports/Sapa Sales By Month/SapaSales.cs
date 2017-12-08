using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Web;
using ExcoUtility;

namespace MvcApplication1.Financial_Reports.Sapa_Sales_By_Month
{
    public class SapaSales
    {
        public static Dictionary<string, double> GetSapaSalesDictionary(int year, int month)
        {
            Dictionary<string, double> returnDict = new Dictionary<string, double>();

            string sCustomerName = "Sapa";
            // get data
            List<Invoice> invoiceList = new List<Invoice>();
            ExcoODBC database = ExcoODBC.Instance;
            database.Open(Database.CMSDAT);
            string query = "";
            if (month == 12)
            {
                query = "select distinct dhinv#, dhexrt, bvcity from cmsdat.oih, cmsdat.cust where (dhbnam like '%" + sCustomerName + "%' or dhbnam like '%" + sCustomerName.ToUpper() + "%') and (dhidat<='" + (Convert.ToInt32(year.ToString()) + 1).ToString() + "-01-01' and dhidat>='" + year + "-" + month + "-01') and bvcust=dhscs# order by bvcity";
            }
            else
            {
                query = "select distinct dhinv#, dhexrt, bvcity from cmsdat.oih, cmsdat.cust where (dhbnam like '%" + sCustomerName + "%' or dhbnam like '%" + sCustomerName.ToUpper() + "%') and (dhidat<'" + year + "-" + (month + 1).ToString("D2") + "-01' and dhidat>='" + year + "-" + month + "-01') and bvcust=dhscs# order by bvcity";
            }
            OdbcDataReader reader = database.RunQuery(query);
            while (reader.Read())
            {
                Invoice invoice = new Invoice();
                invoice.sInvNum = reader["dhinv#"].ToString().Trim();
                invoice.dConversionRate = 1;//Convert.ToDouble(reader["dhexrt"]);
                invoice.sCity = reader["bvcity"].ToString().Trim();
                invoiceList.Add(invoice);
            }
            reader.Close();
            // sale
            for (int i = 0; i < invoiceList.Count; i++)
            {
                query = "select coalesce(sum(dipric*diqtsp),0.0) from cmsdat.oid where diglcd='SAL' and diinv#=" + invoiceList[i].sInvNum;
                reader = database.RunQuery(query);
                if (reader.Read())
                {
                    Invoice invoice = invoiceList[i];
                    invoice.dSale = Convert.ToDouble(reader[0]);
                    invoiceList[i] = invoice;
                }
                reader.Close();
            }
            // discount
            for (int i = 0; i < invoiceList.Count; i++)
            {
                query = "select coalesce(sum(fldext),0.0) as value from cmsdat.ois where (fldisc like 'D%' or fldisc like 'M%') and flinv#=" + invoiceList[i].sInvNum;
                reader = database.RunQuery(query);
                if (reader.Read())
                {
                    Invoice invoice = invoiceList[i];
                    invoice.dDiscount = Convert.ToDouble(reader[0]);
                    invoiceList[i] = invoice;
                }
                reader.Close();
            }
            // fast track
            for (int i = 0; i < invoiceList.Count; i++)
            {
                query = "select coalesce(sum(fldext),0.0) as value from cmsdat.ois where fldisc like 'F%' and flinv#=" + invoiceList[i].sInvNum;
                reader = database.RunQuery(query);
                if (reader.Read())
                {
                    Invoice invoice = invoiceList[i];
                    invoice.dFastTrack = Convert.ToDouble(reader[0]);
                    invoiceList[i] = invoice;
                }
                reader.Close();
            }
            // surcharge
            for (int i = 0; i < invoiceList.Count; i++)
            {
                query = "select coalesce(sum(fldext),0.0) as value from cmsdat.ois where (fldisc like 'S%' or fldisc like 'P%') and flinv#=" + invoiceList[i].sInvNum;
                reader = database.RunQuery(query);
                if (reader.Read())
                {
                    Invoice invoice = invoiceList[i];
                    invoice.dSurcharge = Convert.ToDouble(reader[0]);
                    invoiceList[i] = invoice;
                }
                reader.Close();
            }
            // freight
            for (int i = 0; i < invoiceList.Count; i++)
            {
                query = "select DIEXT from cmsdat.oid where dipart like 'FREIGHT%' AND DIINV# = " + invoiceList[i].sInvNum;
                reader = database.RunQuery(query);
                if (reader.Read())
                {
                    Invoice invoice = invoiceList[i];
                    invoice.dFreight = Convert.ToDouble(reader[0]);
                    invoiceList[i] = invoice;
                }
                reader.Close();
            }

            // write to dictionary
            foreach (Invoice invoice in invoiceList)
            {
                if (!returnDict.ContainsKey(invoice.sCity))
                {
                    returnDict.Add(invoice.sCity, 0);
                }

                returnDict[invoice.sCity] += (invoice.dSale + invoice.dFastTrack + invoice.dSurcharge + invoice.dFreight + invoice.dDiscount) * invoice.dConversionRate;

            }

            return returnDict;
        }
    }
}