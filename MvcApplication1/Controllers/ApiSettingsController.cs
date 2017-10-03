using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.Models;

namespace MvcApplication1.Controllers
{
    public class ApiSettingsController : Controller
    {
        // GET: Settings
        [Route("ApiSettings/EmailSyncSettings/{paramOne}")]
        public ActionResult EmailSyncSettings()
        {
            ApiSettings_EmailSync ASES = new ApiSettings_EmailSync()
            {
                SyncOn = (Settings.GetSettingsValue("EmailSyncOn", "0") == "1"),
                SyncHour = Convert.ToInt32(Settings.GetSettingsValue("EmailSyncHour", "1"))
            };

            return View(ASES);
        }

        [HttpPost]
        [Route("ApiSettings/EmailSyncSettings/SaveEmailSyncSettings")]
        public ActionResult SaveEmailSyncSettings(ApiSettings_EmailSync model)
        {
            Settings.AddSettings("EmailSyncOn", model.SyncOn ? "1" : "0");
            Settings.AddSettings("EmailSyncHour", model.SyncHour.ToString());

            Global.SaveSettings();

            return Redirect("/ApiSettings/EmailSyncSettings/syncSettings");
        }

        [Route("ApiSettings/ClearLog/{paramOne}")]
        public ActionResult ClearLog()
        {;

            Log.DeleteLogFile();
            return Redirect(Request.UrlReferrer.ToString()); // Return to current view
        }
    }
}