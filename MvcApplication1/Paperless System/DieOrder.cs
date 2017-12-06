using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace MvcApplication1.Paperless_System
{
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
                new SqlConnection(Global.ConnectionStr))
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
    }
}