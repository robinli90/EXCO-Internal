using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Security.AntiXss;
using Microsoft.Office.Interop.Outlook;
using MimeKit;

namespace MvcApplication1.Models
{
    [DataContract]
    public class Email
    {

        // General settings
        public string UID { get; set; }

        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string To { get; set; }

        [DataMember]
        public string From { get; set; }

        [DataMember]
        public string Subject { get; set; }

        public string EmailMessage { get; set; }

        [DataMember]
        public DateTime MailDate { get; set; }

        public int AttachmentCount { get; set; }

        [DataMember]
        public string ToName { get; set; }

        public override string ToString()
        {
            return MailDate.ToShortDateString();
        }

        public void MapNameFromEmail()
        {
            ToName = Global.UserList.First(x => x.Email == To).Name;
        }

        public Email()
        {
            EmailMessage = ""; // prevent nullification
        }

        public void RetrieveMsg()
        {
            EmailMessage =
                WebUtility.HtmlDecode(AntiXssEncoder.HtmlEncode(ReadMessage(Global.messagesDirectoryPath + ID + ".eml").TextBody, true));

            // Prevent JSON overflow error
            if (EmailMessage.Length >= 2097152)
            {
                EmailMessage = "";
            }

            MapNameFromEmail();
        }

        public void CreateEmailMsgFile(MimeMessage mailMessage)
        {
            var message = mailMessage;

            using (var stream = File.Create(Global.messagesDirectoryPath + ID + ".eml"))
                message.WriteTo(stream);
        }

        public CDO.Message ReadMessage(String emlFileName)
        {
            CDO.Message msg = new CDO.Message();
            try
            {
                ADODB.Stream stream = new ADODB.Stream();
                stream.Open(Type.Missing, ADODB.ConnectModeEnum.adModeUnknown,
                    ADODB.StreamOpenOptionsEnum.adOpenStreamUnspecified, String.Empty, String.Empty);
                stream.LoadFromFile(emlFileName);
                stream.Flush();
                msg.DataSource.OpenObject(stream, "_Stream");
                msg.DataSource.Save();
                AttachmentCount = msg.Attachments.Count;
            }
            catch
            {
            }

            return msg;
        }
    }

    /* Depreciated 
     * 
    public static class MailMessageExt
    {
        public static void Save(System.Net.Mail.MailMessage Message, string FileName)
        {
            var message = MimeMessage.CreateFromMailMessage(Message);


            using (var stream = File.Create(FileName))
                message.WriteTo(stream);

        }
    }
         
    public void CreateEmailMsgFile(System.Net.Mail.MailMessage mailMessage)
    {
        // Create directory if it does not existID (this should always be the case)
        if (!File.Exists(Global.messagesDirectoryPath + ID + ".eml"))
        {
            MailMessageExt.Save(mailMessage, Global.messagesDirectoryPath + ID + ".eml");
        }
        else
        {
            Log.Append("ERROR: redundant ID detected");
        }
    }

    public void AddAttachment(Attachment attachment, string fileName)
    {
        // Create directory if it does not exist
        if (!Directory.Exists(Global.attachmentDirectoryPath + ID))
        {
            Directory.CreateDirectory(Global.attachmentDirectoryPath + ID);
        }

        SaveMailAttachment(attachment, Global.attachmentDirectoryPath + ID + @"\");

    }

    public void SaveMailAttachment(Attachment attachment, string attachmentDirectoryPath)
    {
        byte[] allBytes = new byte[attachment.ContentStream.Length];
        int bytesRead = attachment.ContentStream.Read(allBytes, 0, (int)attachment.ContentStream.Length);

        string destinationFile = attachmentDirectoryPath + attachment.Name;

        BinaryWriter writer = new BinaryWriter(new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None));
        writer.Write(allBytes);
        writer.Close();
    }*/
}