using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Financial_Reports.Income_Statement.Categories.General_and_Administration
{
    public class Office_Meals_and_Entertainment : Group
    {
        public Office_Meals_and_Entertainment(int fiscalYear, int fiscalMonth)
        {
            name = "OFFICE MEALS AND ENTERTAINMENT";

            // office meals
            plant01.accountList.Add(new Account("100", "623000"));
            plant03.accountList.Add(new Account("300", "623000"));
            plant05.accountList.Add(new Account("500", "623000"));
            plant04.accountList.Add(new Account("451", "559501"));
            plant04.accountList.Add(new Account("451", "559502"));
            plant41.accountList.Add(new Account("4151", "559501"));
            plant41.accountList.Add(new Account("4151", "559502"));
            plant48.accountList.Add(new Account("4851", "559501"));
            plant49.accountList.Add(new Account("4951", "559501"));

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