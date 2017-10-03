using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using MvcApplication1.Models;

namespace MvcApplication1.Services
{
    public class EmailRepository
    {
        private const string CacheKey = "EmailStore";

        public Email[] GetAllEmails()
        {
            var ctx = HttpContext.Current;

            if (ctx != null)
            {
                return (Email[])ctx.Cache[CacheKey];
            }

            return new Email[] { };
        }

        public List<Email> GetAllEmails(string department, string filterParam, bool useLINQ = false)
        {
            return SearchAlgorithm.GeneralSearch(department, filterParam, useLINQ);
        }

        public List<Email> SearchDieNumber(string department, string filterParam, bool useLINQ = false)
        {
            return SearchAlgorithm.SearchDieNumber(filterParam);
        }

        public EmailRepository()
        {
            var ctx = HttpContext.Current;

            if (ctx != null)
            {
                if (ctx.Cache[CacheKey] == null)
                {
                    ctx.Cache[CacheKey] = Global.EmailList.ToArray();
                }
            }
        }

        public static void CacheInfo(Email[] newEmails)
        {
            var ctx = HttpContext.Current;
            var currentData = ((Email[])ctx.Cache[CacheKey]).ToList();
            currentData.AddRange(newEmails);
            ctx.Cache[CacheKey] = currentData.ToArray();
        }

        public bool SaveEmail(Email contact)
        {
            var ctx = HttpContext.Current;

            if (ctx != null)
            {
                try
                {
                    var currentData = ((Email[])ctx.Cache[CacheKey]).ToList();
                    currentData.Add(contact);
                    ctx.Cache[CacheKey] = currentData.ToArray();

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }

            return false;
        }
    }
}