using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace MvcApplication1.Models
{
    public static class Permissions
    {
        public enum Group
        {
            Administrator,
            Manager,
            Sales,
            General
        }

        /// <summary>
        /// Hierarchy for permissions. Important to note that the order matters
        /// </summary>
        private static readonly List<Group> Hierarchy = new List<Group>
        {
            Group.Administrator, 
            Group.Manager, 
            Group.Sales, 
            Group.General
        };

        public static List<string> ValidAPIKeys = new List<string>() {"gluo"};

        public static Group GetGroup(string groupName)
        {
            switch (groupName.ToLower())
            {
                case "administrator":
                    return Group.Administrator;
                case "manager":
                    return Group.Manager;
                case "sales":
                    return Group.Sales;
                case "general":
                    return Group.General;
                default:
                    return Group.General;
            }
        }

        public static string GenerateAPI(Group group)
        {
            string APIHash = new Random().Next(100000000, 999999999).ToString();
            APIHash += new Random().Next(100000000, 999999999).ToString();
            APIHash = group.ToString().Substring(3).ToUpper() + APIHash;

            return AESGCM.SimpleEncryptWithPassword(APIHash, AESGCM.AES256KeyAPI).Substring(20);
        }

        public static List<Email> GetAvailableEmails(Group group, List<Email> emailList, bool useLINQ = false)
        {
            // Admin gets master list
            if (group == Group.Administrator) return Global.EmailList;

            int hierarchyValue = Hierarchy.IndexOf(group);

            // Get all user emails that are in the hierarchial group
            List<string> validEmails = Global.UserList.Where(x => Hierarchy.GetRange(hierarchyValue, Hierarchy.Count - hierarchyValue).Contains(x.Department))
                .Select(x => x.Email).ToList();

            List<Email> returnEmailList = new List<Email>();
            
            if (useLINQ) return emailList.Where(x => validEmails.Contains(x.To)).ToList();

            foreach (Email email in emailList)
            {
                if (validEmails.Contains(email.To))
                {
                    returnEmailList.Add(email);
                }
            }

            return returnEmailList;
            
        }

        public static bool ValidAPIKey(string APIKey)
        {
            bool validAPIKey = ValidAPIKeys.Contains(APIKey);

            if (!validAPIKey)
                Log.Append(string.Format("ERROR: Invalid API Key '{0}'", APIKey));

            return validAPIKey;
        }

        /// <summary>
        /// intendedGroupName is the intended group to see items. Ex: if you want only admin to see something, intendedGroupName = Administrator
        /// onlyThisGroup ignores the hierarchy and enabled for administrators and members of intendedGroup
        /// </summary>
        /// <param name="userGroupName"></param>
        /// <param name="refGroupName"></param>
        /// <returns></returns>
        public static bool HasAccess(string userGroupName, string intendedGroupName, bool onlyThisGroup=false, List<string> inclusionList=null, List<string> exclusionList=null)
        {
            Group userGroup = GetGroup(userGroupName);
            Group intendedGroup = GetGroup(intendedGroupName);

            if (exclusionList != null)
            {
                // Check if user's department is in exclusion list
                if (exclusionList.Any(y => Global.UserList.First(x => x.Email.ToLower() == HttpContext.Current.Session["Email"].ToString().ToLower()).Department.ToString().Contains(y)))
                    return false;
                // Check if email is in exclusion list
                if (exclusionList.Any(y => HttpContext.Current.Session["Email"].ToString().ToLower().Contains(y)))
                    return false;
            }

            if (inclusionList != null)
            {
                // Check if user's department is in inclusion list
                if (inclusionList.Any(y => Global.UserList.First(x => x.Email.ToLower() == HttpContext.Current.Session["Email"].ToString().ToLower()).Department.ToString().Contains(y)))
                    return true;
                // Check if email is in inclusion list
                inclusionList.ForEach(x => x.ToLower());
                if (inclusionList.Any(y => HttpContext.Current.Session["Email"].ToString().ToLower().Contains(y)))
                    return true;
            }

            // Only members of same group and administrators
            if (onlyThisGroup && (userGroup == intendedGroup || userGroup == Group.Administrator))
                return true;

            // Check if userGroup is equal to less than intendedGroup index (lower = higher in hierarchy)
            if (!onlyThisGroup && Hierarchy.IndexOf(userGroup) <= Hierarchy.IndexOf(intendedGroup))
            {
                return true;
            }

            return false;
        }
    }
}