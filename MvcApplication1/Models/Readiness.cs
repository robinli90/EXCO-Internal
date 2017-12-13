using System;
using System.IO;
using MvcApplication1.Paperless_System;

namespace MvcApplication1.Models
{
    public static class Readiness
    {
        private static readonly string SyncBlockerFilePath = @"\\10.0.0.8\EmailAPI\Documentations\check.chk"; 
        public static readonly string TerminationBlockerFilePath = @"\\10.0.0.8\EmailAPI\Documentations\termination.chk";

        public static bool Terminate = false;

        public static bool EmailSyncReady()
        {
            if (File.Exists(SyncBlockerFilePath))
            {
                return false;
            }

            return true;
        }

        public static void CreateBlockerFile()
        {
            File.WriteAllText(SyncBlockerFilePath, "");
            File.SetAttributes(SyncBlockerFilePath, FileAttributes.Hidden);
        }

        public static void DeleteBlockerFile()
        {
            File.Delete(SyncBlockerFilePath);
        }

        public static bool CheckTerminationStatus(bool doNotLog=false)
        {
            if (Terminate)
            {
                if (!doNotLog)
                {
                    Log.Append("Email update has been terminated");
                    Terminate = false;
                }
                return true;
            }

            return false;
        }


        public static void TerminationCheck()
        {
            if (File.Exists(TerminationBlockerFilePath))
            {
                Log.Append("Termination request detected...");
                Terminate = true;
            }
            else if (Terminate)
            {
                Terminate = false;
            }
        }
        

        private static bool _SyncReady = true;
        public static int _SyncHour = 1; // Sync at 1am?
        public static int _SyncBuffer = 5; // How much time post-sync to toggle _syncReady? (_SyncHour + _SyncBuffer must be <= 23 -- 24 hours)

        public static void MinutelyTaskChecker()
        {
            if (_SyncReady && DateTime.Now.Hour == Convert.ToInt32(Settings.GetSettingsValue("EmailSyncHour", "1")) && Settings.GetSettingsValue("EmailSyncOn", "0") == "1")
            {
                Log.Append("***Scheduled synchronizations and updates started***");
                _SyncReady = false;
                PSTImporter.SyncPSTFiles();
                Global.GetAllEmails();
            }
            else if (!_SyncReady && DateTime.Now.Hour == Convert.ToInt32(Settings.GetSettingsValue("EmailSyncHour", "1")) + _SyncBuffer)
            {
                _SyncReady = true;
            }
            
            // Archive check every 59 minutes
            if (Environment.MachineName.Contains("EXCOTRACK3") && DateTime.Now.Minute % 30 == 0)
            {
                ArchivesChecker.ProcessEmailsForArchive();
                ArchivesChecker.CheckPendingOrders();
            }
            
            // Archive check every 5 minutes
            if (DateTime.Now.Minute % 5 == 0)
            {
                ArchivesChecker.ProcessCacheFiles();
            }
        }
    }
}