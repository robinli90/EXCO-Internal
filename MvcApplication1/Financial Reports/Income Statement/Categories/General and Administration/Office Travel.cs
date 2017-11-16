using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Financial_Reports.Income_Statement.Categories.General_and_Administration
{
    public class Office_Travel : Group
    {
        public Office_Travel(int fiscalYear, int fiscalMonth)
        {
            name = "OFFICE TRAVEL";

            // office travel
            plant01.accountList.Add(new Account("100", "624000"));
            plant03.accountList.Add(new Account("300", "624000"));
            plant05.accountList.Add(new Account("500", "624000"));

            plant04.accountList.Add(new Account("451", "201002"));
            plant04.accountList.Add(new Account("451", "201002"));
            plant04.accountList.Add(new Account("451", "550501"));
            plant04.accountList.Add(new Account("451", "550502"));
            plant04.accountList.Add(new Account("451", "552001"));
            plant04.accountList.Add(new Account("451", "552002"));
            //plant04.accountList.Add(new Account("451", "552003")); auto 
            plant04.accountList.Add(new Account("451", "559550"));
            //plant04.accountList.Add(new Account("451", "551502"));

            plant41.accountList.Add(new Account("4151", "201002"));
            plant41.accountList.Add(new Account("4151", "550501"));
            plant41.accountList.Add(new Account("4151", "552001"));
            plant41.accountList.Add(new Account("4151", "550502"));
            plant41.accountList.Add(new Account("4151", "552002"));
            //plant41.accountList.Add(new Account("4151", "552003")); auto 
            plant41.accountList.Add(new Account("4151", "559550"));
            //plant41.accountList.Add(new Account("4151", "551502"));

            plant48.accountList.Add(new Account("4851", "201002"));
            plant48.accountList.Add(new Account("4851", "550501"));
            plant48.accountList.Add(new Account("4851", "550502"));
            plant48.accountList.Add(new Account("4851", "552001"));
            plant48.accountList.Add(new Account("4851", "552002"));
            //plant48.accountList.Add(new Account("4851", "552003")); auto 
            plant48.accountList.Add(new Account("4851", "559550"));

            plant49.accountList.Add(new Account("4951", "201002"));
            plant49.accountList.Add(new Account("4951", "550501"));
            plant49.accountList.Add(new Account("4951", "552001"));
            plant49.accountList.Add(new Account("4951", "550502"));
            plant49.accountList.Add(new Account("4951", "552002"));
            //plant49.accountList.Add(new Account("4951", "552003")); auto 
            plant49.accountList.Add(new Account("4951", "559550"));


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