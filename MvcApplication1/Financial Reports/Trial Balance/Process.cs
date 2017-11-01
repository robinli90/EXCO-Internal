
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using ExcoUtility;
using Excel2 = Microsoft.Office.Interop.Excel;

namespace MvcApplication1.Financial_Reports.Trial_Balance
{
    public class Process
    { 
        
        // @note This is the main function of the working
        // thread.
        public Process(string filePath, string plant, int year, int period)
        {
            ExcoExRate.GetExchangeRatesList();

            try
            {
                int plantID = 0;
                string plantName = string.Empty;
                if (plant == "Markham")
                {
                    plantID = 1;
                    plantName = "Markham";
                }
                else if (plant == "Michigan")
                {
                    plantID = 3;
                    plantName = "Michigan";
                }
                else if (plant == "Texas")
                {
                    plantID = 5;
                    plantName = "Texas";
                }
                else if (plant == "Colombia")
                {
                    plantID = 4;
                    plantName = "Colombia";
                }

                ExcoCalendar calendar = new ExcoCalendar(year, period, true, plantID);
                ExcoCalendar tempCalendar = new ExcoCalendar(year, period, false, 1);
                ExcoODBC database = ExcoODBC.Instance;
                database.Open(Database.CMSDAT);
                Dictionary<int, Account> accountMap = new Dictionary<int, Account>();
                string query = string.Empty;
                OdbcDataReader reader;
                if (plantID == 1)
                {
                    query = "select distinct a.aj4gl#1 as no1, a.aj4gl#2 as no2, b.aztitl as title, b.azatyp as account_type, (a.aj4tt" + calendar.GetFiscalMonth().ToString("D2") + "+aj4ob" + calendar.GetFiscalMonth().ToString("D2") + ") as balance from cmsdat.glmt as a, cmsdat.mast as b where a.aj4comp=1 and a.aj4ccyy=" + (calendar.GetFiscalYear()).ToString("D2") + " and a.aj4gl#1=b.azgl#1 and a.aj4gl#2=b.azgl#2 and b.azcomp=1 and a.aj4gl#1!=200";
                    reader = database.RunQuery(query);
                    while (reader.Read())
                    {
                        Account account = new Account();
                        account.glNum1 = Convert.ToInt32(reader["no1"]) % 100;
                        account.glNum2 = Convert.ToInt32(reader["no2"]);
                        account.title = reader["title"].ToString().Trim();
                        account.balanceAmount01 = Convert.ToDouble(reader["balance"]);
                        int key = account.glNum1 * 1000000 + account.glNum2;
                        if (99999999 == key)
                        {
                            continue;
                        }
                        if (accountMap.ContainsKey(key))
                        {
                            accountMap[key].balanceAmount01 = account.balanceAmount01;
                        }
                        else
                        {
                            accountMap.Add(key, account);
                        }
                    }
                    reader.Close();
                }
                else if (plantID == 3)
                {
                    query = "select a.aj4gl#1 as no1, a.aj4gl#2 as no2, b.aztitl as title, b.azatyp as account_type, (a.aj4tt" + calendar.GetFiscalMonth().ToString("D2") + "+aj4ob" + calendar.GetFiscalMonth().ToString("D2") + ") as balance from cmsdat.glmt as a, cmsdat.mast as b where a.aj4comp=3 and a.aj4ccyy=" + (calendar.GetFiscalYear()).ToString("D2") + " and a.aj4gl#1=b.azgl#1 and a.aj4gl#2=b.azgl#2 and b.azcomp=3";
                    reader = database.RunQuery(query);
                    while (reader.Read())
                    {
                        Account account = new Account();
                        account.glNum1 = Convert.ToInt32(reader["no1"]) % 100;
                        account.glNum2 = Convert.ToInt32(reader["no2"]);
                        account.title = reader["title"].ToString().Trim();
                        account.balanceAmount03 = Convert.ToDouble(reader["balance"]);
                        int key = account.glNum1 * 1000000 + account.glNum2;
                        if (99999999 == key)
                        {
                            continue;
                        }
                        if (accountMap.ContainsKey(key))
                        {
                            accountMap[key].balanceAmount03 = account.balanceAmount03;
                        }
                        else
                        {
                            accountMap.Add(key, account);
                        }
                    }
                    reader.Close();
                }
                else if (plantID == 5)
                {
                    query = "select a.aj4gl#1 as no1, a.aj4gl#2 as no2, b.aztitl as title, b.azatyp as account_type, (a.aj4tt" + calendar.GetFiscalMonth().ToString("D2") + "+aj4ob" + calendar.GetFiscalMonth().ToString("D2") + ") as balance from cmsdat.glmt as a, cmsdat.mast as b where a.aj4comp=5 and a.aj4ccyy=" + (calendar.GetFiscalYear()).ToString("D2") + " and a.aj4gl#1=b.azgl#1 and a.aj4gl#2=b.azgl#2 and b.azcomp=5";
                    reader = database.RunQuery(query);
                    while (reader.Read())
                    {
                        Account account = new Account();
                        account.glNum1 = Convert.ToInt32(reader["no1"]) % 100;
                        account.glNum2 = Convert.ToInt32(reader["no2"]);
                        account.title = reader["title"].ToString().Trim();
                        account.balanceAmount05 = Convert.ToDouble(reader["balance"]);
                        int key = account.glNum1 * 1000000 + account.glNum2;
                        if (99999999 == key)
                        {
                            continue;
                        }
                        if (accountMap.ContainsKey(key))
                        {
                            accountMap[key].balanceAmount05 = account.balanceAmount05;
                        }
                        else
                        {
                            accountMap.Add(key, account);
                        }
                    }
                    reader.Close();
                }
                else if (plantID == 4)
                {
                    // plant 04
                    query = "select a.aj4gl#1 as no1, a.aj4gl#2 as no2, b.aztitl as title, b.azatyp as account_type, (a.aj4tt" + calendar.GetFiscalMonth().ToString("D2") + "+aj4ob" + calendar.GetFiscalMonth().ToString("D2") + ") as balance from cmsdat.glmt as a, cmsdat.mast as b where a.aj4comp=4 and a.aj4ccyy=" + (calendar.GetFiscalYear()).ToString("D2") + " and a.aj4gl#1=b.azgl#1 and a.aj4gl#2=b.azgl#2 and b.azcomp=4";
                    reader = database.RunQuery(query);
                    while (reader.Read())
                    {
                        Account account = new Account();
                        account.glNum1 = Convert.ToInt32(reader["no1"]) % 100;
                        account.glNum2 = Convert.ToInt32(reader["no2"]);
                        account.title = reader["title"].ToString().Trim();
                        account.balanceAmount04 = Convert.ToDouble(reader["balance"]);
                        int key = account.glNum1 * 1000000 + account.glNum2;
                        if (99999999 == key)
                        {
                            continue;
                        }
                        if (accountMap.ContainsKey(key))
                        {
                            accountMap[key].balanceAmount04 = account.balanceAmount04;
                        }
                        else
                        {
                            accountMap.Add(key, account);
                        }
                    }
                    reader.Close();
                    // plant 48
                    query = "select a.aj4gl#1 as no1, a.aj4gl#2 as no2, b.aztitl as title, b.azatyp as account_type, (a.aj4tt" + calendar.GetFiscalMonth().ToString("D2") + "+aj4ob" + calendar.GetFiscalMonth().ToString("D2") + ") as balance from cmsdat.glmt as a, cmsdat.mast as b where a.aj4comp=48 and a.aj4ccyy=" + (calendar.GetFiscalYear() + 2000).ToString("D2") + " and a.aj4gl#1=b.azgl#1 and a.aj4gl#2=b.azgl#2 and b.azcomp=48";
                    reader = database.RunQuery(query);
                    while (reader.Read())
                    {
                        Account account = new Account();
                        account.glNum1 = Convert.ToInt32(reader["no1"]) % 100;
                        account.glNum2 = Convert.ToInt32(reader["no2"]);
                        account.title = reader["title"].ToString().Trim();
                        account.balanceAmount48 = Convert.ToDouble(reader["balance"]);
                        int key = account.glNum1 * 1000000 + account.glNum2;
                        if (99999999 == key)
                        {
                            continue;
                        }
                        if (accountMap.ContainsKey(key))
                        {
                            accountMap[key].balanceAmount48 = account.balanceAmount48;
                        }
                        else
                        {
                            accountMap.Add(key, account);
                        }
                    }
                    reader.Close();
                    // plant 41
                    query = "select a.aj4gl#1 as no1, a.aj4gl#2 as no2, b.aztitl as title, b.azatyp as account_type, (a.aj4tt" + tempCalendar.GetFiscalMonth().ToString("D2") + "+aj4ob" + tempCalendar.GetFiscalMonth().ToString("D2") + ") as balance from cmsdat.glmt as a, cmsdat.mast as b where a.aj4comp=41 and a.aj4ccyy=" + (tempCalendar.GetFiscalYear() + 2000).ToString("D2") + " and a.aj4gl#1=b.azgl#1 and a.aj4gl#2=b.azgl#2 and b.azcomp=41";
                    reader = database.RunQuery(query);
                    while (reader.Read())
                    {
                        Account account = new Account();
                        account.glNum1 = Convert.ToInt32(reader["no1"]) % 100;
                        account.glNum2 = Convert.ToInt32(reader["no2"]);
                        account.title = reader["title"].ToString().Trim();
                        account.balanceAmount41 = Convert.ToDouble(reader["balance"]);
                        int key = account.glNum1 * 1000000 + account.glNum2;
                        if (99999999 == key)
                        {
                            continue;
                        }
                        if (accountMap.ContainsKey(key))
                        {
                            accountMap[key].balanceAmount41 = account.balanceAmount41;
                        }
                        else
                        {
                            accountMap.Add(key, account);
                        }
                    }
                    reader.Close();

                    // plant 49

                    query = "select a.aj4gl#1 as no1, a.aj4gl#2 as no2, b.aztitl as title, b.azatyp as account_type, (a.aj4tt" + tempCalendar.GetFiscalMonth().ToString("D2") + "+aj4ob" + tempCalendar.GetFiscalMonth().ToString("D2") + ") as balance from cmsdat.glmt as a, cmsdat.mast as b where a.aj4comp=49 and a.aj4ccyy=" + (tempCalendar.GetFiscalYear() + 2000).ToString("D2") + " and a.aj4gl#1=b.azgl#1 and a.aj4gl#2=b.azgl#2 and b.azcomp=49";
                    reader = database.RunQuery(query);
                    while (reader.Read())
                    {
                        Account account = new Account();
                        account.glNum1 = Convert.ToInt32(reader["no1"]) % 100;
                        account.glNum2 = Convert.ToInt32(reader["no2"]);
                        account.title = reader["title"].ToString().Trim();
                        account.balanceAmount49 = Convert.ToDouble(reader["balance"]);
                        int key = account.glNum1 * 1000000 + account.glNum2;
                        if (99999999 == key)
                        {
                            continue;
                        }
                        if (accountMap.ContainsKey(key))
                        {
                            accountMap[key].balanceAmount49 = account.balanceAmount49;
                        }
                        else
                        {
                            accountMap.Add(key, account);
                        }
                    }
                    reader.Close();
                }
                else
                {
                    throw new Exception("None of plants has been selected!");
                }
                // write to excel
                Excel2.Application excel = new Excel2.Application();
                object misValue = System.Reflection.Missing.Value;
                Excel2.Workbook book = excel.Workbooks.Add(misValue);
                //Excel.Worksheet sheet = book.Worksheets[1];
                Excel2.Worksheet sheet = book.Worksheets.Add();//[1];
                //sheet.Delete();
                //sheet = book.Worksheets[1];
                //sheet.Delete();
                //sheet = book.Worksheets[1];
                sheet.Name = "Trial Balance for " + plantName;
                // write title
                sheet.Cells[1, 1] = plantName;
                Excel2.Range range = sheet.get_Range("A1");
                range.Font.Color = Color.Red;
                range.Font.Size = 14;
                sheet.Cells[2, 1] = "Balance Consolidations";
                range = sheet.get_Range("A2");
                range.Font.Size = 10;
                range.Font.Bold = true;
                sheet.Cells[3, 1] = "Generated at " + DateTime.Today.ToString("MMMM-dd-yyyy");
                range = sheet.get_Range("A3");
                range.Font.Size = 10;
                range.Font.Bold = true;
                sheet.Cells[6, 1] = "Ending Balance";
                range = sheet.get_Range("A6");
                range.Font.Size = 11;
                range.Font.Color = Color.Red;
                range.Font.Bold = true;
                sheet.Cells[7, 1] = "Account";
                range = sheet.get_Range("A7");
                range.Font.Size = 11;
                range.Cells.Borders[Excel2.XlBordersIndex.xlEdgeBottom].LineStyle = Excel2.XlLineStyle.xlContinuous;
                sheet.Cells[5, 2] = "Company Number";
                range = sheet.get_Range("B5");
                range.Font.Size = 11;
                range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                sheet.Cells[6, 2] = "Period";
                range = sheet.get_Range("B6");
                range.Font.Size = 11;
                range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                sheet.Cells[7, 2] = "Account Name / Company";
                range = sheet.get_Range("B7");
                range.Font.Size = 11;
                range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                if (4 != plantID)
                {
                    sheet.Cells[5, 3] = plantID.ToString("D2");
                    range = sheet.get_Range("C5");
                    range.Font.Size = 11;
                    range.Font.Color = Color.Red;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                    sheet.Cells[6, 3] = (calendar.GetFiscalYear() + 2000).ToString() + "-" + calendar.GetFiscalMonth().ToString("D2");
                    range = sheet.get_Range("C6");
                    range.Font.Size = 11;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                    sheet.Cells[7, 3] = plantName;
                    range = sheet.get_Range("C7");
                    range.Font.Size = 11;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                }
                else
                {
                    // 04
                    sheet.Cells[5, 3] = "04";
                    range = sheet.get_Range("C5");
                    range.Font.Size = 11;
                    range.Font.Color = Color.Red;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                    sheet.Cells[6, 3] = (calendar.GetFiscalYear() + 2000).ToString() + "-" + calendar.GetFiscalMonth().ToString("D2");
                    range = sheet.get_Range("C6");
                    range.Font.Size = 11;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                    sheet.Cells[7, 3] = "Exco GAAP";
                    range = sheet.get_Range("C7");
                    range.Font.Size = 11;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                    // 41
                    sheet.Cells[5, 4] = "41";
                    range = sheet.get_Range("D5");
                    range.Font.Size = 11;
                    range.Font.Color = Color.Red;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                    sheet.Cells[6, 4] = (tempCalendar.GetFiscalYear() + 2000).ToString() + "-" + tempCalendar.GetFiscalMonth().ToString("D2");
                    range = sheet.get_Range("D6");
                    range.Font.Size = 11;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                    sheet.Cells[7, 4] = "Exco IFRS";
                    range = sheet.get_Range("D7");
                    range.Font.Size = 11;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                    // 48
                    sheet.Cells[5, 5] = "48";
                    range = sheet.get_Range("E5");
                    range.Font.Size = 11;
                    range.Font.Color = Color.Red;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                    sheet.Cells[6, 5] = (calendar.GetFiscalYear() + 2000).ToString() + "-" + calendar.GetFiscalMonth().ToString("D2");
                    range = sheet.get_Range("E6");
                    range.Font.Size = 11;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                    sheet.Cells[7, 5] = "Coltooling GAAP";
                    range = sheet.get_Range("E7");
                    range.Font.Size = 11;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                    // 49
                    sheet.Cells[5, 6] = "49";
                    range = sheet.get_Range("F5");
                    range.Font.Size = 11;
                    range.Font.Color = Color.Red;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                    sheet.Cells[6, 6] = (tempCalendar.GetFiscalYear() + 2000).ToString() + "-" + tempCalendar.GetFiscalMonth().ToString("D2");
                    range = sheet.get_Range("F6");
                    range.Font.Size = 11;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                    sheet.Cells[7, 6] = "Coltooling IFRS";
                    range = sheet.get_Range("F7");
                    range.Font.Size = 11;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                    // cosolidated
                    sheet.Cells[7, 7] = "Consolidated";
                    range = sheet.get_Range("G7");
                    range.Font.Size = 11;
                    range.Cells.Borders.LineStyle = Excel2.XlLineStyle.xlContinuous;
                    range.Cells.Interior.Color = Color.PeachPuff;
                }

                // write content
                int row = 8;
                var accountList = from account in accountMap.Values orderby account.glNum1 * 1000000 + account.glNum2 select account;
                foreach (Account account in accountList)
                {
                    sheet.Cells[row, 1] = account.glNum1.ToString("D2") + "-" + (account.glNum2 / 100).ToString("D4") + "-" + (account.glNum2 % 100).ToString("D2");
                    sheet.Cells[row, 2] = account.title;
                    if (1 == plantID)
                    {
                        sheet.Cells[row++, 3] = account.balanceAmount01.ToString("C2");
                    }
                    else if (3 == plantID)
                    {
                        sheet.Cells[row++, 3] = account.balanceAmount03.ToString("C2");
                    }
                    else if (5 == plantID)
                    {
                        sheet.Cells[row++, 3] = account.balanceAmount05.ToString("C2");
                    }
                    else if (4 == plantID)
                    {
                        sheet.Cells[row, 3] = account.balanceAmount04.ToString("C2");
                        sheet.Cells[row, 4] = account.balanceAmount41.ToString("C2");
                        sheet.Cells[row, 5] = account.balanceAmount48.ToString("C2");
                        sheet.Cells[row, 6] = account.balanceAmount49.ToString("C2");
                        range = sheet.get_Range("G" + row.ToString());
                        range.Formula = "=sum(C" + row.ToString() + ":F" + row.ToString() + ")";
                        row++;
                    }
                }
                // write to file
                sheet.Cells.Columns.AutoFit();
                sheet.Cells.HorizontalAlignment = Excel2.XlHAlign.xlHAlignCenter;
                File.Delete(filePath);
                book.SaveAs(filePath, Excel2.XlFileFormat.xlOpenXMLWorkbook);
                excel.Quit();
            }
            catch (Exception e)
            {
            }
        }
    }
}
