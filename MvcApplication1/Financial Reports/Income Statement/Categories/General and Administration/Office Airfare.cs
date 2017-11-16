using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Financial_Reports.Income_Statement.Categories.General_and_Administration
{
    public class Office_Airfare : Group
    {
        public Office_Airfare(int fiscalYear, int fiscalMonth)
        {
            name = "OFFICE AIRFARE";

            // certain
            //plant41.accountList.Add(new Account("4151", "552003"));
            //plant48.accountList.Add(new Account("4851", "552003"));
            //plant04.accountList.Add(new Account("451", "552003"));
            //plant49.accountList.Add(new Account("4951", "552003"));


            // office travel
            plant01.accountList.Add(new Account("100", "625000"));
            plant03.accountList.Add(new Account("300", "625000"));
            plant05.accountList.Add(new Account("500", "625000"));


            // office air fare
            plant04.accountList.Add(new Account("451", "551501"));
            plant04.accountList.Add(new Account("451", "551502"));
            plant04.accountList.Add(new Account("452", "551502"));
            plant41.accountList.Add(new Account("4151", "551501"));
            plant41.accountList.Add(new Account("4151", "551502"));
            plant41.accountList.Add(new Account("4152", "551502"));
            plant48.accountList.Add(new Account("4851", "551501"));
            plant48.accountList.Add(new Account("4851", "551502"));
            plant49.accountList.Add(new Account("4951", "551501"));


            // process accounts
            plant01.GetAccountsData(fiscalYear, fiscalMonth);
            plant03.GetAccountsData(fiscalYear, fiscalMonth);
            plant05.GetAccountsData(fiscalYear, fiscalMonth);
            plant04.GetAccountsData(fiscalYear, fiscalMonth);
            plant41.GetAccountsData(fiscalYear, fiscalMonth);
            plant48.GetAccountsData(fiscalYear, fiscalMonth);
            plant49.GetAccountsData(fiscalYear, fiscalMonth);
        }
    }
}