using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace MvcApplication1.Models
{
    public class DWGRecipient
    {
        [Required(ErrorMessage = "Customer # is required")]
        [Range(1000, 99999, ErrorMessage = "Please enter valid customer number")]
        public string customerNo { get; set; }

        [Required(ErrorMessage = "Recipient Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string emailAddress { get; set; }

        public string identifyString { get; set; }
    }

    public static class DWGEmail
    {
        public static Dictionary<string, List<DWGRecipient>> Recipients { get; set; }

        private static readonly string _recipientFilePath = @"\\10.0.0.8\EmailAPI\Documentations\list.txt";

        static DWGEmail()
        {
            LoadRecipients();
        }

        public static void AddRecipient(string customerNo, string emailAddress)
        {
            if (Recipients.ContainsKey(customerNo))
            {
                Recipients[customerNo].Add(new DWGRecipient()
                {
                    customerNo = customerNo,
                    emailAddress = emailAddress
                });
            }
            else
            {
                Recipients.Add(customerNo, new List<DWGRecipient>
                    { new DWGRecipient()
                        {
                            customerNo = customerNo,
                            emailAddress = emailAddress
                        }
                    }
                );
            }

            SaveRecipients();
        }

        public static void DeleteRecipient(string customerNo, int listIndex)
        {
            if (Recipients.ContainsKey(customerNo))
            {
                if (Recipients[customerNo].Count - 1 >= listIndex)
                {
                    Log.Append(string.Format("DWG Email Recipient removed {1}-'{0}'", Recipients[customerNo][listIndex].emailAddress, customerNo));

                    Recipients[customerNo].RemoveAt(listIndex);

                    // Remove key if recipient list is empty for customerNo
                    if (Recipients[customerNo].Count == 0)
                        Recipients.Remove(customerNo);
                }
            }

            SaveRecipients();
        }

        private static void LoadRecipients()
        {
            Recipients = new Dictionary<string, List<DWGRecipient>>();
            
            if (!File.Exists(_recipientFilePath)) return;

            var text = File.ReadAllText(_recipientFilePath);
            string[] lines = text.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

            // Load user information
            foreach (string line in lines)
            {
                if (line.Length > 10)
                {
                    string[] entry = line.Trim().Split(new[] {" "}, StringSplitOptions.None);
                    AddRecipient(entry[0], entry[1]);
                }
            }

            Console.Write("Complete");
        }

        private static void SaveRecipients()
        {
            StringBuilder str = new StringBuilder();

            foreach (KeyValuePair<string, List<DWGRecipient>> dwgRecipient in Recipients)
            {
                foreach (DWGRecipient recipient in dwgRecipient.Value)
                {
                    str.Append(String.Format("{0} {1}", recipient.customerNo, recipient.emailAddress) +
                               Environment.NewLine);
                }
            }

            Saver.Save(str.ToString(), _recipientFilePath);

        }
        
    }
}