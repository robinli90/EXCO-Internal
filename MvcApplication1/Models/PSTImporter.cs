using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Office.Interop.Outlook;
using Redemption;
using WebGrease.Css.Extensions;
using MAPIFolder = Microsoft.Office.Interop.Outlook.MAPIFolder;

namespace MvcApplication1.Models
{
    public static class PSTImporter
    {
        public static readonly string pstSyncDirectoryPath = @"\\10.0.0.8\EmailAPI\PST File Sync\";
        public static readonly string pstDumpDirectoryPath = @"\\10.0.0.8\EmailAPI\PST Dump\";
        public static readonly string syncedMessagesDirectoryPath = @"\\10.0.0.8\EmailAPI\Synced Messages\";

        public static void SyncPSTFiles()
        {
            if (Global.isSyncing)
            {
                Log.Append("ERROR: Cannot sync pst files because email synchronization is in progress");
                return;
            }

            TimeSpan startTime = DateTime.Now.TimeOfDay;

            Log.Append("Syncing all available .pst files...");
            
            string[] pstFiles = Directory.GetFiles(pstSyncDirectoryPath, "*.pst*");

            foreach (string filePath in pstFiles.Where(x => !x.Contains("tmp")))
            {
                string dumpPath = pstDumpDirectoryPath + Path.GetFileName(filePath);
                File.Move(filePath, dumpPath);

                Log.Append(String.Format("  Syncing emails from pst file '{0}'", Path.GetFileName(filePath)));

                RDOSession session = new RDOSession();
                session.LogonPstStore(dumpPath);
                RDOFolder folder = session.GetDefaultFolder(rdoDefaultFolders.olFolderInbox);

                // Log-purpose parameters
                int pstEmailCount = folder.Items.Count;
                int currentEmailCount = 0;

                foreach (RDOMail mail in folder.Items)
                {
                    currentEmailCount++;

                    Email email = new Email()
                    {
                        ID = Global.GetAttachmentID(),
                        UID = mail.EntryID,
                        To = mail.To,
                        From = mail.SenderEmailAddress,
                        Subject = mail.Subject,
                        MailDate = mail.SentOn
                    };

                    email.RetrieveMsg();
                    Global.AppendEmail(email, String.Format("{0}/{1}", currentEmailCount, pstEmailCount));
                    mail.SaveAs(Global.messagesDirectoryPath + email.ID + ".eml", rdoSaveAsType.olRFC822);
                }

                session.Logoff();
            }

            pstFiles.Where(x => !x.Contains("tmp")).ForEach(x => Log.Append(String.Format("   Moving pst. file \"{0}\"", x)));

            Log.Append(String.Format("Synced all available PST files. Process took {0}",
                Arithmetic.GetStopWatchStr((DateTime.Now.TimeOfDay - startTime).TotalMilliseconds)));
        }
    }
}

