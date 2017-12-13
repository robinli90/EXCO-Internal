using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Ajax.Utilities;
using MvcApplication1.Models;
using MvcApplication1.Paperless_System;

namespace MvcApplication1
{
    public static class Global
    {
        private static readonly int MAX_EMAIL_COUNT = 5000;

        public static readonly string attachmentDirectoryPath = @"\\10.0.0.8\EmailAPI\Attachments\";
        public static readonly string messagesDirectoryPath = @"\\10.0.0.8\EmailAPI\Messages\";
        public static readonly string cacheDirectoryPath = @"\\10.0.0.8\EmailAPI\Cache\";
        public static readonly string settingsFilePath = @"\\10.0.0.8\EmailAPI\settings.cfg";

        public static bool isSyncing = false;

        public static List<Email> EmailList = new List<Email>();
        public static List<User> UserList = new List<User>();

        public static string ConnectionStr = "SERVER =10.0.0.6; Database =decade; UID =jamie; PWD =jamie;";


        internal static void LoadInitializers()
        {
            ArchivesChecker.GetEntireArchive();
            LoadSettings();
            ImportEmailFile();
            Readiness.DeleteBlockerFile();

            if (!Environment.MachineName.Contains("ROBIN"))
            {
                //Global.GetAllEmails();
            }

        }

        // ==========================================================================================
        // =====================================[ EMAILS ]===========================================
        // ==========================================================================================

        public static List<Email> GetAllEmails(bool forceUpdate = false)
        {

            #if DEBUG
            Readiness.DeleteBlockerFile();
            #endif

            Log.Append("Pulling emails from email servers...");

            TimeSpan startTime = DateTime.Now.TimeOfDay;

            List<Email> newEmails = new List<Email>();

            if (isSyncing || !Readiness.EmailSyncReady())
            {
                Log.Append("ERROR: Existing sync in progress");

                return new List<Email>();
            }
            
            Readiness.CreateBlockerFile();

            // If force update, set last update time to new DateTime (year = 1800)
            if (forceUpdate) UserList.ForEach(x => x.LastUpdateTime = new DateTime());

            foreach (User user in UserList.Where(x => x.SyncEmails))
            {
                newEmails.AddRange(user.GetEmails());

                if (Readiness.CheckTerminationStatus()) break;
            }

            // Remove emails from last update date (prevent redundancy)
            ClearDuplicateMailByID();

            Readiness.DeleteBlockerFile();

            Log.Append(String.Format("All inboxes completed. Total of {0} emails cached ({2} new emails). Process took {1}", EmailList.Count, 
                Arithmetic.GetStopWatchStr((DateTime.Now.TimeOfDay - startTime).TotalMilliseconds), newEmails.Count));

            ExportEmailFile();

            // Remove non-validated and unmapped messages in the background
            Task.Run(() => ValidateMessages(true));

            SaveSettings();
            
            return newEmails;
        }

        public static void AppendEmail(Email email, string prependLogInfo = "")
        {
            if (prependLogInfo.Length > 0)
                prependLogInfo = String.Format("({0}) - ", prependLogInfo);

            Log.Append(String.Format("    {2}email ({1}) added from {0}", email.From, email.MailDate, prependLogInfo));
            EmailList.Add(email);
        }

        /// <summary>
        /// Read all email messages; extremely slow process
        /// </summary>
        public static void ReadAllEmailMessages()
        {
            TimeSpan startTime = DateTime.Now.TimeOfDay;
            Log.Append("Reading all email messages...");
            try
            {
                EmailList.ForEach(x => x.RetrieveMsg());
            }
            catch
            {
                Log.Append("ERROR: Collection modified while reading email messages. Process restarting...");
                //  Restart process
                //Task.Run(() => ReadAllEmailMessages());
            }
            Log.Append(String.Format("All email messages processed. Process took {0}", 
                Arithmetic.GetStopWatchStr((DateTime.Now.TimeOfDay - startTime).TotalMilliseconds)));
        }

        // Remove today's email to prevent redundancy when syncying with email servers
        public static void ClearDuplicateMailByID()
        {
            return;

            int startCount = EmailList.Count;
            /*
            List<string> refEmailList = EmailList.Where(x => x.MailDate.Date == refDate.Date && x.To == toEmailAddress).ToList()
                .Select(x => x.ID).ToList();

            // Remove today's email
            foreach (string ID in refEmailList)
            {
                if (File.Exists(messagesDirectoryPath + ID + ".eml"))
                {
                    EmailList.RemoveAll(x => x.ID == ID);
                    File.Delete(messagesDirectoryPath + ID + ".eml");
                }
            }
            */
            // Distinct UID, To 
            List<Email> temp = EmailList.DistinctBy(x => new { x.UID, x.To }).ToList();

            foreach (Email email in EmailList)
            {
                if (!temp.Contains(email))
                {
                    File.Delete(messagesDirectoryPath + email.ID + ".eml");
                }
            }

            EmailList = temp;

            Log.Append(String.Format("{0} duplicates found and removed!", startCount - EmailList.Count));

        }

        /// <summary>
        /// Remove duplicate emails by date, from, to, subject
        /// </summary>
        public static int PurgeDuplicates()
        {
            TimeSpan startTime = DateTime.Now.TimeOfDay;

            Log.Append("Purging duplicate emails and files...");
            int startCount = EmailList.Count;

            // Distinct by date, subject, from, to (expensive task)
            List<Email> temp = EmailList.DistinctBy(x => new { x.MailDate.Date, x.Subject, x.From, x.To }).ToList();

            foreach (Email email in EmailList)
            {
                if (!temp.Contains(email))
                {
                    File.Delete(messagesDirectoryPath + email.ID + ".eml");
                }
            }

            EmailList = temp;

            Log.Append(String.Format("{1} duplicates purged. Process took {0}", 
                Arithmetic.GetStopWatchStr((DateTime.Now.TimeOfDay - startTime).TotalMilliseconds), startCount - EmailList.Count));

            return startCount - EmailList.Count;
        }

        public static void ImportEmailFile()
        {
            Log.Append("Application initialized");
            
            TimeSpan startTime = DateTime.Now.TimeOfDay;

            Log.Append("Caching email list...");

            try
            {
                string[] emailFiles = Directory.GetFiles(cacheDirectoryPath, "*.cfg*");

                if (emailFiles.Length <= 0)
                    RegenerateCacheFile();

                foreach (string filePath in emailFiles)
                {
                    var text = File.ReadAllText(filePath);
                    string[] lines = AESGCM.SimpleDecryptWithPassword(text, AESGCM.AES256Key)
                        .Split(new[] {Environment.NewLine}, StringSplitOptions.None);

                    // Load email information
                    foreach (string line in lines)
                    {
                        if (line.Contains("[EM_ID_]="))
                        {
                            Email email = new Email()
                            {
                                ID = Parser.Parse(line, "EM_ID_"),
                                UID = Parser.Parse(line, "EM_UI_"),
                                To = Parser.Parse(line, "EM_TO_"),
                                From = Parser.Parse(line, "EM_FR_"),
                                Subject = Parser.Parse(line, "EM_SU_"),
                                EmailMessage = "", // prevent nullification
                                MailDate = Convert.ToDateTime(Parser.Parse(line, "EM_MD_"))
                            };

                            // For testing, local does not read emails
                            //email.RetrieveMsg();

                            EmailList.Add(email);

                            email.MapNameFromEmail();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                RegenerateCacheFile();
            }

            // Retrieve all messages in background
            Task.Run(() => ReadAllEmailMessages());
            
            // Sort by date
            EmailList = EmailList.OrderByDescending(x => x.MailDate).ToList();

            Log.Append(String.Format("Cached {1} emails! Process took {0}",
            Arithmetic.GetStopWatchStr((DateTime.Now.TimeOfDay - startTime).TotalMilliseconds), EmailList.Count));

            GetEmailCounts();
        }

        public static void RegenerateCacheFile()
        {
            Log.Append("ERROR: Could not parse email file. Recovering cache file...");

            TimeSpan startTime = DateTime.Now.TimeOfDay;
            
            EmailList = new List<Email>();

            string[] messageFiles = Directory.GetFiles(messagesDirectoryPath, "*.eml*");

            int emailCount = 0;
            int totalEmailCount = messageFiles.Length;

            foreach (string filePath in messageFiles)
            {
                CDO.Message CDOMessage = Email.GetEmlFromFile(filePath);

                Email email = new Email()
                {
                    ID = Path.GetFileName(filePath).Substring(0, 9),
                    UID = "0",
                    To = CDOMessage.To + (CDOMessage.CC.Length > 0 ? "," + CDOMessage.CC : ""),
                    From = CDOMessage.From,
                    Subject = CDOMessage.Subject,
                    EmailMessage = "", // prevent nullification
                    MailDate = CDOMessage.ReceivedTime
                };

                emailCount++;

                email.ParseToEmail();
                AppendEmail(email, String.Format("{0}/{1}", emailCount, totalEmailCount));

                email.MapNameFromEmail();
            }

            // Save regenerated files
            ExportEmailFile();

            isSyncing = false;
            Readiness.DeleteBlockerFile();

            Log.Append(String.Format("Cache regeneration completed. Process took {0}",
            Arithmetic.GetStopWatchStr((DateTime.Now.TimeOfDay - startTime).TotalMilliseconds)));
        }   

        /// <summary>
        /// Get Attachment ID; if not exists, create a new one
        /// </summary>
        public static string GetAttachmentID()
        {
            string ID = new Random().Next(100000000, 999999999).ToString();

            // Check for duplication
            while (EmailList.Any(x => x.ID == ID))
            {
                ID = new Random().Next(100000000, 999999999).ToString();
            }

            return ID;
        }

        public static void ExportEmailFile()
        {
            int saveCount = 0;
            int fileCount = 0;

            Log.Append("Syncing email cache to server...");

            // Remove all original files
            foreach (string path in Directory.GetFiles(cacheDirectoryPath, "*.cfg*"))
            {
                if (File.Exists(cacheDirectoryPath + "CacheDump/" + Path.GetFileName(path)))
                {
                    File.Delete(cacheDirectoryPath + "CacheDump/" + Path.GetFileName(path));
                }
                File.Move(path, cacheDirectoryPath + "CacheDump/" + Path.GetFileName(path));
            }

            StringBuilder str = new StringBuilder();

            foreach (Email email in EmailList)
            {
                str.Append("[EM_ID_]=" + email.ID);
                str.Append("||[EM_UI_]=" + email.UID);
                str.Append("||[EM_TO_]=" + email.To);
                str.Append("||[EM_FR_]=" + email.From);
                str.Append("||[EM_SU_]=" + email.Subject);
                str.Append("||[EM_MD_]=" + email.MailDate + Environment.NewLine);

                saveCount++;

                if (saveCount % MAX_EMAIL_COUNT == 0)
                {
                    Saver.Save(AESGCM.SimpleEncryptWithPassword(str.ToString(), AESGCM.AES256Key), cacheDirectoryPath + "emailMaster_" + (++fileCount) + ".cfg");
                    str = new StringBuilder();
                }
            }

            // Get remainder
            if (saveCount % MAX_EMAIL_COUNT != 0 && str.Length > 0)
                Saver.Save(AESGCM.SimpleEncryptWithPassword(str.ToString(), AESGCM.AES256Key), cacheDirectoryPath + "emailMaster_" + (++fileCount) + ".cfg");

            GetEmailCounts();

            Log.Append("Sync complete!");
        }

        /// <summary>
        /// Verify that every message has an associated email object (if not, remove)
        /// </summary>
        public static void ValidateMessages(bool postSyncCheck=false)
        {
            if (isSyncing && !postSyncCheck)
            {
                Log.Append("ERROR: Sync in progress. Cannot validate information");
                return;
            }

            TimeSpan startTime = DateTime.Now.TimeOfDay;

            int startingCount = PurgeDuplicates();

            Log.Append("Validating email & data integrity...");

            int removedMessages = 0;
            startingCount += EmailList.Count;

            EmailList = EmailList.Where(x => File.Exists(messagesDirectoryPath + x.ID + ".eml") && UserList.Any(y => y.Email.ToLower() == x.To.ToLower())).ToList();

            string[] messageFiles = Directory.GetFiles(messagesDirectoryPath, "*.eml*");

            foreach (string filePath in messageFiles)
            {
                if (!EmailList.Any(x => x.ID == Path.GetFileName(filePath).Substring(0,9)))
                {
                    File.Delete(filePath);
                    removedMessages++;
                }
            }

            removedMessages += startingCount - EmailList.Count;


            Log.Append(String.Format("Validation completed. Found and removed {0} unverified messages. Process took {1}", 
                removedMessages, Arithmetic.GetStopWatchStr((DateTime.Now.TimeOfDay - startTime).TotalMilliseconds)));

            // Save again after verification if removed anything
            if (removedMessages > 0)
                ExportEmailFile();
            
            Readiness.DeleteBlockerFile();
        }

        // ==========================================================================================
        // ====================================[ SETTINGS ]==========================================
        // ==========================================================================================


        public static void AddUser(User user)
        {
            if (UserList.Contains(user)) return;

            Log.Append(String.Format("Added new user {0} - {1}", user.Name, user.Email));

            user.InternalID = Global.UserList.Count == 0 ? 0 : Global.UserList.Max(x => x.InternalID) + 1;
            UserList.Add(user);
            SaveSettings();
        }

        public static void SaveSettings()
        {

            StringBuilder str = new StringBuilder();

            // Save user data
            foreach (User user in UserList)
            {
                str.Append("[US_NA_]=" + user.Name);
                str.Append("||[US_DE_]=" + user.Department);
                str.Append("||[US_ID_]=" + user.UserID);
                str.Append("||[US_LP_]=" + user.LoginPassword);
                str.Append("||[US_PR_]=" + user.Privilege);
                str.Append("||[US_SE_]=" + (user.SyncEmails ? "1" : "0"));
                str.Append("||[US_LU_]=" + user.LastUpdateTime);
                str.Append("||[US_PT_]=" + user.ReceivingProtocol);
                str.Append("||[US_PO_]=" + user.ReceivingPort);
                str.Append("||[US_EM_]=" + user.Email);
                str.Append("||[US_PW_]=" + user.Password + Environment.NewLine);
            }

            str.Append(Settings.GetSaveSettingsStr());

            Saver.Save(AESGCM.SimpleEncryptWithPassword(str.ToString(), AESGCM.AES256Key), settingsFilePath);
        }

        public static void LoadSettings()
        {
            if (!File.Exists(settingsFilePath)) return;

            var text = File.ReadAllText(settingsFilePath);
            string[] lines = AESGCM.SimpleDecryptWithPassword(text, AESGCM.AES256Key).Split(new[] {Environment.NewLine}, StringSplitOptions.None);

            // Initialize settings dictionary
            Settings.SettingsDictionary = new Dictionary<string, string>();

            UserList = new List<User>();

            // Load user information
            foreach (string line in lines)
            {
                if (line.Contains("[US_NA_]="))
                {
                    User user = new User()
                    {
                        Name = Parser.Parse(line, "US_NA_"),
                        Department = Permissions.GetGroup(Parser.Parse(line, "US_DE_")),
                        UserID = Parser.Parse(line, "US_ID_"),
                        LoginPassword = Parser.Parse(line, "US_LP_"),
                        SyncEmails = Parser.Parse(line, "US_SE_") == "1",
                        Privilege = Parser.Parse(line, "US_PR_"),
                        LastUpdateTime = Convert.ToDateTime(Parser.Parse(line, "US_LU_")),
                        ReceivingProtocol = Parser.Parse(line, "US_PT_"),
                        ReceivingPort = Parser.Parse(line, "US_PO_"),
                        Email = Parser.Parse(line, "US_EM_"),
                        Password = Parser.Parse(line, "US_PW_"),
                        InternalID = UserList.Count == 0 ? 0 : UserList.Max(x => x.InternalID) + 1
                };
                    
                    UserList.Add(user);
                }
                else if (line.StartsWith("#SETVAL||[ST_NAME_]="))
                {
                    Settings.LoadSettingsStr(line);
                }
            }
            
            SaveSettings();
        }

        public static void GetEmailCounts()
        {
            UserList.ForEach(x => x.GetEmailCount());
        }


    }

}