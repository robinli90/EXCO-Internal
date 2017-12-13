using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using MvcApplication1.Models;

namespace MvcApplication1.Controllers
{
    public class UserController : Controller
    {
        //============================================================================================================================================
        //==========================================================[ USER MANAGEMENT ]===============================================================
        //============================================================================================================================================

        // GET: User
        [Route("User/UserManagement/{paramOne}")]
        public ActionResult UserManagement(string paramOne)
        {
            if (!Permissions.HasAccess(HttpContext.Session["Department"].ToString(), "Administrator"))
                return HttpNotFound();

            // if UserManagement
            if (paramOne.ToLower() == "newuser")
                return View("~/Views/User/UserModify.cshtml", new User() {InternalID = 99999999});

            // if UserManagement
            if (paramOne.ToLower() == "tasks")
                return View("~/Views/User/UserTasks.cshtml");


            return View();
        }

        [Route("User/UserTasks/{paramOne}")]
        public ActionResult UserTasks(string paramOne)
        {
            if (!Permissions.HasAccess(HttpContext.Session["Department"].ToString(), "Administrator"))
                return HttpNotFound();

            return View();
        }
        
        [Route("User/UserModify/{userInternalID}")]
        public ActionResult UserModify(string userInternalID)
        {

            if (!Permissions.HasAccess(HttpContext.Session["Department"].ToString(), "Administrator"))
                return HttpNotFound();

            // if UserManagement
            User refUser = Global.UserList.First(x => x.InternalID.ToString() == userInternalID);
            return View("~/Views/User/UserModify.cshtml", refUser);
        }
        
        [Route("User/UserDelete/{userInternalID}")]
        public ActionResult UserDelete(string userInternalID)
        {
            if (!Permissions.HasAccess(HttpContext.Session["Department"].ToString(), "Administrator"))
                return HttpNotFound();

            User refUser = Global.UserList.First(x => x.InternalID.ToString() == userInternalID);

            Log.Append(String.Format("Deleted user '{0}' by {1}", refUser.Email, HttpContext.Session["Email"]));

            // remove user from userlist
            Global.UserList.Remove(refUser);
            Global.SaveSettings();

            return Redirect("/User/UserManagement/manage");
        }

        [HttpPost]
        [Route("User/UserManagement/UserChangeAdd")]
        public ActionResult UserChangeAdd(User model)
        {
            if (!Permissions.HasAccess(HttpContext.Session["Department"].ToString(), "Administrator"))
                return HttpNotFound();

            // If fields not complete
            if (model.Name == null || model.Email == null || model.LoginPassword == null || 
                Global.UserList.Any(x => x.Email.ToLower() == model.Email.ToLower() && x.InternalID != model.InternalID)) // Check same email exists with not same ID (id check for editing)
                return View("~/Views/User/UserModify.cshtml", model);

            User refUser = Global.UserList.FirstOrDefault(x => x.InternalID == model.InternalID);

            // If user exists
            if (refUser != null)
            {
                refUser.Email = model.Email;
                refUser.Password = model.Password;
                refUser.LoginPassword = model.LoginPassword;
                refUser.Name = model.Name;
                refUser.SyncEmails = model.SyncEmails;
                refUser.Department = model.Department;
                refUser.ReceivingPort = model.ReceivingPort;
                refUser.ReceivingProtocol = model.ReceivingProtocol;

                Log.Append(String.Format("User '{0}' has been modified by '{1}'", model.Email, HttpContext.Session["Email"]));
            }
            else
            {
                model.LastUpdateTime = new DateTime(2010, 1, 1);
                Global.AddUser(model);
            }

            Global.SaveSettings();

            return Redirect("/User/UserManagement/manage");
        }


        //============================================================================================================================================
        //=============================================================[ USER TASKS ]=================================================================
        //============================================================================================================================================

        [Route("User/UserResetUpdateTime/{userInternalID}")]
        public ActionResult UserResetUpdateTime(string userInternalID)
        {
            if (!Permissions.HasAccess(HttpContext.Session["Department"].ToString(), "Administrator"))
                return HttpNotFound();

            User refUser = Global.UserList.First(x => x.InternalID.ToString() == userInternalID);
            Log.Append(String.Format("Last Update Time reset for user '{0}'", refUser.Email));
            refUser.LastUpdateTime = new DateTime(2010, 1, 1);
            Global.SaveSettings();
            return Redirect("/User/UserTasks/tasks");
        }

        [Route("User/UserForceSync/{userInternalID}")]
        public ActionResult UserForceSync(string userInternalID)
        {
            if (!Permissions.HasAccess(HttpContext.Session["Department"].ToString(), "Administrator"))
                return HttpNotFound();

            User refUser = Global.UserList.First(x => x.InternalID.ToString() == userInternalID);
            Task.Run(() => refUser.GetEmails());
            return Redirect("/User/UserTasks/tasks");
        }

        [Route("User/UserPurgeEmail/{userInternalID}")]
        public ActionResult UserPurgeEmail(string userInternalID)
        {
            if (!Permissions.HasAccess(HttpContext.Session["Department"].ToString(), "Administrator"))
                return HttpNotFound();

            User refUser = Global.UserList.First(x => x.InternalID.ToString() == userInternalID);
            Task.Run(() => refUser.DeleteEmailsForUser());
            return Redirect("/User/UserTasks/tasks");
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult doesEmailExist(string email)
        {

            var user = Global.UserList.First(x => x.Email.ToLower() == email.ToLower());

            return Json(user == null);
        }
    }
}