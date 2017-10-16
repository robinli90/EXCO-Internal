using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcApplication1.Models;

namespace MvcApplication1.Services
{
    public static class SearchAlgorithm
    {

        public static List<Email> GeneralSearch(string department, string filterParam, bool useLINQ = false)
        {
            filterParam = filterParam.ToLower();

            // Base copy
            List<Email> collectionWorking = Global.EmailList; //(Email[])ctx.Cache[CacheKey];


            // Validate what pool of emails the user can search from (increases performance)
            collectionWorking =
                Permissions.GetAvailableEmails(Permissions.GetGroup(department), collectionWorking, useLINQ);

            if (filterParam.Contains("date="))
            {
                DateTime refDate = new DateTime();
                string[] dateParam = filterParam.Split(new[] {","}, StringSplitOptions.None);
                foreach (string parameter in dateParam)
                {
                    string paramValue = parameter.Split(new[] {"="}, StringSplitOptions.None)[1].ToLower();
                    if (parameter.Contains("date="))
                    {
                        refDate = new DateTime(
                            Convert.ToInt32(paramValue.Substring(4)), //year
                            Convert.ToInt32(paramValue.Substring(0, 2)), //month
                            Convert.ToInt32(paramValue.Substring(2, 2)) //day
                        );
                    }

                    collectionWorking = collectionWorking.Where(x => x.MailDate >= refDate).ToList();
                }
            }

            string[] filterParameters = filterParam.Split(new[] {","}, StringSplitOptions.None);

            try
            {
                foreach (string parameter in filterParameters)
                {
                    string paramValue = parameter.Split(new[] {"="}, StringSplitOptions.None)[1].ToLower();
                    if (parameter.Contains("all="))
                    {
                        /* DEPRECIATED BECAUSE SLOW*/
                        if (useLINQ)
                            collectionWorking = collectionWorking.Where(x => x.From.ToLower().Contains(paramValue) ||
                                                                             x.To.ToLower().Contains(paramValue) ||
                                                                             x.Subject.ToLower().Contains(paramValue) ||
                                                                             x.EmailMessage.ToLower()
                                                                                 .Contains(paramValue))
                                .ToList();
                        else
                            collectionWorking = FilterEmailsByAll(collectionWorking, paramValue);
                    }
                    else
                    {
                        if (parameter.Contains("to="))
                        {
                            /* DEPRECIATED BECAUSE SLOW*/
                            if (useLINQ)
                                collectionWorking = collectionWorking.Where(x => x.To.ToLower().Contains(paramValue))
                                    .ToList();
                            else
                                collectionWorking = FilterEmailsByReceiver(collectionWorking, paramValue);
                        }
                        if (parameter.Contains("from="))
                        {
                            /* DEPRECIATED BECAUSE SLOW*/
                            if (useLINQ)
                                collectionWorking = collectionWorking.Where(x => x.From.ToLower().Contains(paramValue))
                                    .ToList();
                            else
                                collectionWorking = FilterEmailsBySender(collectionWorking, paramValue);
                        }
                        if (parameter.Contains("subject="))
                        {
                            /* DEPRECIATED BECAUSE SLOW*/
                            if (useLINQ)
                                collectionWorking = collectionWorking
                                    .Where(x => x.Subject.ToLower().Contains(paramValue))
                                    .ToList();
                            else
                                collectionWorking = FilterEmailsBySubject(collectionWorking, paramValue);
                        }
                        if (parameter.Contains("msg="))
                        {
                            /* DEPRECIATED BECAUSE SLOW*/
                            if (useLINQ)
                                collectionWorking = collectionWorking
                                    .Where(x => x.EmailMessage.ToLower().Contains(paramValue))
                                    .ToList();
                            else
                                collectionWorking = FilterEmailsByMessage(collectionWorking, paramValue);
                        }
                    }
                }
            }
            catch (Exception parsingError)
            {
                Log.Append(String.Format("ERROR: Parsing error for query={0} ({1})", filterParam, parsingError));
                return new List<Email> { };
            }

            if (collectionWorking.Count > 0)
            {
                Log.Append(String.Format("GET command completed. Returned {0} results", collectionWorking.Count));
                return collectionWorking;
            }

            Log.Append("GET command completed. Returned no results");
            return new List<Email>();
        }


        // Faster performance than LINQ.Where
        public static List<Email> FilterEmailsByReceiver(List<Email> refList, string filterWord)
        {
            StripSymbols(ref filterWord);
            List<Email> returnList = new List<Email>();
            foreach (var email in refList)
            {
                if (email.To.ToLower().Contains(filterWord))
                {
                    returnList.Add(email);
                }
            }
            return returnList;
        }

        public static List<Email> FilterEmailsBySender(List<Email> refList, string filterWord)
        {
            StripSymbols(ref filterWord);
            List<Email> returnList = new List<Email>();
            foreach (var email in refList)
            {
                if (email.From.ToLower().Contains(filterWord))
                {
                    returnList.Add(email);
                }
            }
            return returnList;
        }

        public static List<Email> FilterEmailsBySubject(List<Email> refList, string filterWord)
        {
            StripSymbols(ref filterWord);
            List<Email> returnList = new List<Email>();
            foreach (var email in refList)
            {
                if (email.Subject.ToLower().Contains(filterWord))
                {
                    returnList.Add(email);
                }
            }
            return returnList;
        }

        public static List<Email> FilterEmailsByMessage(List<Email> refList, string filterWord)
        {
            StripSymbols(ref filterWord);
            List<Email> returnList = new List<Email>();
            foreach (var email in refList)
            {
                if (email.EmailMessage.ToLower().Contains(filterWord))
                {
                    returnList.Add(email);
                }
            }
            return returnList;
        }

        public static List<Email> FilterEmailsByAll(List<Email> refList, string filterWord)
        {
            StripSymbols(ref filterWord);
            List<Email> returnList = new List<Email>();
            foreach (var email in refList)
            {
                if (email.To.ToLower().Contains(filterWord) ||
                    email.From.ToLower().Contains(filterWord) ||
                    email.Subject.ToLower().Contains(filterWord) ||
                    email.EmailMessage.ToLower().Contains(filterWord))
                {
                    returnList.Add(email);
                }
            }
            return returnList;
        }

        public static List<Email> SearchDieNumber(string filterWord)
        {
            List<Email> collectionWorking = Global.EmailList;

            if (filterWord.Contains("date="))
            {
                DateTime refDate = new DateTime();
                string dateParam = filterWord.Split(new[] { "," }, StringSplitOptions.None)[1];
                string paramValue = dateParam.Split(new[] { "=" }, StringSplitOptions.None)[1].ToLower();
                
                refDate = new DateTime(
                    Convert.ToInt32(paramValue.Substring(4)), //year
                    Convert.ToInt32(paramValue.Substring(0, 2)), //month
                    Convert.ToInt32(paramValue.Substring(2, 2)) //day
                );

                collectionWorking = collectionWorking.Where(x => x.MailDate >= refDate).ToList();

                filterWord = filterWord.Substring(0, filterWord.IndexOf(",date="));
            }

            StripSymbols(ref filterWord);
            filterWord = filterWord.ToLower();
            List<Email> returnList = new List<Email>();

            for (int i = 0; i < filterWord.Length; i++)
            {
                returnList.AddRange(FilterEmailsBySubject(collectionWorking, filterWord.Insert(i, " ")));
                returnList.AddRange(FilterEmailsBySubject(collectionWorking, filterWord.Insert(i, "-")));
                returnList.AddRange(FilterEmailsByMessage(collectionWorking, filterWord.Insert(i, " ")));
                returnList.AddRange(FilterEmailsByMessage(collectionWorking, filterWord.Insert(i, "-")));
            }

            Log.Append(String.Format("GET command completed. Returned {0} results", returnList.Count));
            return returnList.Distinct().ToList();
        }

        public static void StripSymbols(ref string refString)
        {
            string returnStr = "";
            foreach (var ch in refString)
            {
                if (char.IsSymbol(ch) && ch != '-')
                {
                }
                else
                    returnStr += ch;
            }
            refString = returnStr.Trim();
        }
    }
}