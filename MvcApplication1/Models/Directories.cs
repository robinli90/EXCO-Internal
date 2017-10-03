using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models
{
    public static class Repository
    {
        // Key=folder name, List<string>=list of messageID associated with this folder
        private static Dictionary<string, List<string>> Directories { get; set; }

        public static void CreateFolder(string folderName)
        {
            if (!Directories.ContainsKey(folderName))
            {
                Directories.Add(folderName, new List<string>());
            }

            throw new Exception("Folder with same name exists");
        }

        public static void DeleteFolder(string folderName)
        {
            if (Directories.ContainsKey(folderName))
            {
                Directories.Remove(folderName);
            }

            throw new Exception("Folder with name DNE");
        }
    }
}