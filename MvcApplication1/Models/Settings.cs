using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication1.Models
{
    public static class Settings
    {
        public static Dictionary<string, string> SettingsDictionary { get; set; }
        
        public static void AddSettings(string name, string value)
        {
            if (SettingsDictionary.ContainsKey(name))
            {
                SettingsDictionary[name] = value;
                return;
            }

            SettingsDictionary.Add(name, value);
        }
        
        public static string GetSettingsValue(string settingsName, string noValueReturn = "ERROR_NO_KEY_FOUND_FOR")
        {
            if (SettingsDictionary.ContainsKey(settingsName))
            {
                return SettingsDictionary[settingsName];
            }

            return noValueReturn;
        }

        public static string GetSaveSettingsStr()
        {
            if (SettingsDictionary.Count == 0)
                return string.Empty;

            StringBuilder str = new StringBuilder();
            foreach (KeyValuePair<string, string> settingsPair in SettingsDictionary)
            {
                str.Append(string.Format("#SETVAL||[ST_NAME_]={0}||[ST_VAL_]={1}", settingsPair.Key,
                    settingsPair.Value) + Environment.NewLine);
            }

            return str.ToString();
        }

        public static void LoadSettingsStr(string settingsLine)
        {
            if (settingsLine.Contains("#SETVAL||[ST_NAME_]"))
            {
                AddSettings(Parser.Parse(settingsLine, "ST_NAME_"), Parser.Parse(settingsLine, "ST_VAL_"));
            }
        }
    }

    public class ApiSettings_EmailSync
    {
        public bool SyncOn { get; set; }
        public int SyncHour { get; set; }

        public int SelectedId { get; set; }
        public IList<SelectListItem> AllItems { get; set; }

        public ApiSettings_EmailSync()
        {
            SyncHour = 1;
            SyncOn = false;

            AllItems = new List<SelectListItem>();
            for (int i = 0; i < 24; i++)
            {
                AllItems.Add(new SelectListItem() { Value = i.ToString(), Text = i.ToString()});
            }
        }
        
    }
}