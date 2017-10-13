using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MvcApplication1
{
    public static class Log
    {
        public static readonly string logFilePath = @"\\10.0.0.8\EmailAPI\log.txt";

        public static void AddTick()
        {
            AppendLine(".");
        }

        public static void Append(string logText)
        {
            logText = String.Format("[{3}-{0}:{1}:{2}] - {5} - {4}", DateTime.Now.Hour.ToString("D2"),
                DateTime.Now.Minute.ToString("D2"),
                DateTime.Now.Second.ToString("D2"), DateTime.Now.Date.ToShortDateString(), logText,
                //GetSessionEmail());
                Environment.MachineName);
            
            while (true)
            {
                try
                {
                    AppendLine(logText + Environment.NewLine);
                    return;
                }
                catch
                {
                    // File in use, try again
                }
            }


        }

        public static string GetSessionEmail()
        {
            try
            {
                return HttpContext.Current.Session["Email"].ToString();
            }
            catch (Exception e)
            {
                return Environment.MachineName;
                //return "Unauthenticated session";
            }
        }

        public static void AppendBlankLine()
        {
            AppendLine("=======================================================================================================================" + Environment.NewLine);
        }

        public static void DeleteLogFile()
        {
            try
            {
                File.Delete(logFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void AppendLine(string line)
        {
            while (true)
            {
                try
                {
                    File.AppendAllText(logFilePath, line);
                    return;
                }
                catch
                {
                    // File in use, try again
                }
            }
        }
    }
}