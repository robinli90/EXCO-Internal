using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using MailKit;
using MailKit.Search;
using MimeKit;
//using ImapClient = AE.Net.Mail.ImapClient;

namespace MvcApplication1.Models
{

    public class User
    {
        // General settings
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        
        public int SelectedId { get; set; }
        public IList<SelectListItem> AllItems { get; set; }

        public Permissions.Group Department { get; set; }
        public string UserID { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string LoginPassword { get; set; }

        public string Privilege { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public bool SyncEmails { get; set; }

        // Email settings
        public string ReceivingProtocol { get; set; }

        public string ReceivingPort { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [Remote("doesEmailExist", "User", HttpMethod = "POST", ErrorMessage = "Email already exists. Please enter a different user name.")]
        public string Email { get; set; }

        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public int InternalID { get; set; }

        public int InternalEmailCount { get; set; }

        public User()
        {
            AllItems = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "0", Text = "Administrator"},
                new SelectListItem() {Value = "1", Text = "Manager"},
                new SelectListItem() {Value = "2", Text = "Sales"},
                new SelectListItem() {Value = "3", Text = "General"},
            };
        }

        public int GetDropDownId()
        {
            if (Name == "")
                return 0;

            return AllItems.IndexOf(AllItems.First(x => x.Text == Department.ToString()));
        }

        public List<Email> GetEmails(bool saveAttachments = false)
        {
            if (Global.isSyncing)
            {
                Log.Append("Error: Existing sync in progress");
                return new List<Email>();
            }

            Global.isSyncing = true;

            int errorLevel = 0;

            Log.Append(String.Format("Getting new emails (since {1}) from '{0}'...", Email, LastUpdateTime.ToShortDateString()));

            // Generate the list of uids from current emails stored for current user (used to check if email already exists)
            List<string> ExistingUID = Global.EmailList.Where(x => x.To == Email && x.MailDate >= LastUpdateTime.AddDays(-5)).Select(y => y.UID).ToList();

            List<Email> newEmails = new List<Email>();
           
            int emailSyncCount = 0;

            using (var client = new MailKit.Net.Imap.ImapClient())
            {
                try
                {

                    //For demo-purposes, accept all SSL certificates
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    #region Connect using receiving parameters

                    try
                    {
                        client.Connect(ReceivingProtocol, Convert.ToInt32(ReceivingPort), true);
                    }
                    catch
                    {
                        Log.Append(String.Format("ERROR: Failed to connect to {0} using {1}:{2}", Email,
                            ReceivingProtocol, ReceivingPort));
                        return new List<Email>();
                    }

                    #endregion

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    #region Login using credentials

                    try
                    {
                        client.Authenticate(Email, Password);
                    }
                    catch
                    {
                        Log.Append(String.Format("ERROR: Failed to login using user credentials for '{0}'", Email));
                        return new List<Email>();
                    }

                    #endregion

                    #region Inbox

                    // The Inbox folder is always available on all IMAP servers...
                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadOnly);

                    Log.Append("  Checking 'Inbox'");

                    var query = SearchQuery.DeliveredAfter(LastUpdateTime);

                    foreach (var uid in inbox.Search(query))
                    {
                        try
                        {
                            if (Readiness.CheckTerminationStatus(true))
                                break;

                            string workingID = Global.GetAttachmentID();

                            try
                            { 
                                // Verify that this email does not exist.
                                if (!ExistingUID.Contains(uid.ToString()))
                                {
                                    MimeMessage message = inbox.GetMessage(uid);

                                    var date = message.Date.ToString();

                                    Email email = new Email()
                                    {
                                        UID = uid.ToString(),
                                        ID = workingID,
                                        MailDate = Convert.ToDateTime(date),
                                        From = message.From.ToString(),
                                        To = Email,
                                        Subject = message.Subject,
                                    };
                                    emailSyncCount++;
                                    email.CreateEmailMsgFile(message);
                                    email.RetrieveMsg();

                                    newEmails.Add(email);
                                    Global.AppendEmail(email);
                                }
                            }
                            catch
                            {
                                Log.Append(String.Format("ERROR: Email can't be processed with ID={0}", workingID));
                            }
                        }
                        catch (Exception ex)
                        {
                            errorLevel++;
                            Log.Append(String.Format("ERROR [Inbox]: {0}", ex));
                            // Undetermined error from automated system 
                        }
                    }

                    #endregion

                    #region Subfolders

                    var personal = client.GetFolder(client.PersonalNamespaces[0]);
                    foreach (var folder in personal.GetSubfolders(false))
                    {
                        Log.Append(String.Format("  Checking folder '{0}'", folder.Name));

                        if (folder.Name.ToLower() != "sent")
                        {
                            if (Readiness.CheckTerminationStatus(true))
                                break;

                            try
                            {
                                folder.Open(FolderAccess.ReadOnly);

                                query = SearchQuery.DeliveredAfter(LastUpdateTime);

                                foreach (var uid in folder.Search(query))
                                {
                                    if (Readiness.CheckTerminationStatus())
                                        return newEmails;

                                    // Verify that this email does not exist.
                                    if (!ExistingUID.Contains(uid.ToString()))
                                    {
                                        MimeMessage message = folder.GetMessage(uid);

                                        var date = message.Date.ToString();

                                        Email email = new Email()
                                        {
                                            UID = uid.ToString(),
                                            ID = Global.GetAttachmentID(),
                                            MailDate = Convert.ToDateTime(date),
                                            From = message.From.ToString(),
                                            To = Email,
                                            Subject = message.Subject,
                                        };

                                        emailSyncCount++;
                                        email.CreateEmailMsgFile(message);
                                        email.RetrieveMsg();

                                        newEmails.Add(email);
                                        Global.AppendEmail(email);
                                    }
                                }
                            }
                            catch
                            {
                                Log.Append(String.Format("  Sub folder IMAP retrieval error for folder=\"{0}\"",
                                    folder.Name));
                            }
                        }
                    }

                    #endregion

                    client.Disconnect(true);
                }
                catch (Exception ex)
                {
                    errorLevel++;
                    Log.Append(String.Format("ERROR [Overall]: {0}", ex));
                }
            }
            

            Log.Append(String.Format("{0} emails synced.", emailSyncCount));

            if (errorLevel <= 0)
            {
                LastUpdateTime = DateTime.Now;
                Log.Append("Complete!");
            }
            GetEmailCount();
            Global.SaveSettings();
            Global.ExportEmailFile();

            Global.isSyncing = false;

            // Sort by date
            Global.EmailList = Global.EmailList.OrderByDescending(x => x.MailDate).ToList();

            return newEmails;
        }


        public void GetEmailCount()
        {
            try
            {
                InternalEmailCount = Global.EmailList.Count(y => y.To.ToLower() == Email.ToLower());
            }
            catch (Exception e)
            {
                Log.Append(String.Format("ERROR: Cannot get email count - {0}", e));
            }
        }

        public void DeleteEmailsForUser()
        {
            Log.Append(String.Format("Deleting files for '{0}'...", Email));
            List<Email> userEmails = Global.EmailList.Where(x => x.To.ToLower() == Email.ToLower()).ToList();

            foreach (Email email in userEmails)
            {
                Global.EmailList.Remove(email);

                if (File.Exists(Global.messagesDirectoryPath + email.ID + ".eml"))
                {
                    File.Delete(Global.messagesDirectoryPath + email.ID + ".eml");
                }
            }

            InternalEmailCount = 0;

            Global.ExportEmailFile();

            Log.Append(String.Format("Deletion complete. Deleted {0} emails...", userEmails.Count));
            
        }
    }
}


/* S22 Mail 
using (S22.Imap.ImapClient incoming = new S22.Imap.ImapClient(ReceivingProtocol, Convert.ToInt32(ReceivingPort), Email, Password, 
    AuthMethod.Login))
{
    // This returns all messages sent since August 23rd 2012
    IEnumerable<uint> messageIDs = incoming.Search(
        SearchCondition.SentSince(LastUpdateTime)
    );

    foreach (System.Net.Mail.MailMessage mailMessage in incoming.GetMessages(messageIDs))
    {
        Email email = new Email()
        {
            ID = Global.GetAttachmentID(),
            MailDate = Convert.ToDateTime(mailMessage.Date()),
            From = mailMessage.From.ToString(),
            To = Email,
            Subject = mailMessage.Subject,
        };

        email.CreateEmailMsgFile(mailMessage);
        email.RetrieveMsg();

        if (saveAttachments)
        {
            foreach (System.Net.Mail.Attachment attachment in mailMessage.Attachments)
            {
                email.AddAttachment(attachment, mailMessage.Subject);
            }
        }

        Global.AppendEmail(email);

    }
    Log.Append(String.Format("{0} emails synced.", messageIDs.Count()));

    LastUpdateTime = DateTime.Now;
                
    return emailList;
}
}*/