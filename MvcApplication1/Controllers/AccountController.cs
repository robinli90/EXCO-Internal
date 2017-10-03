using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.Models;

namespace MvcApplication1.Controllers
{
    public class AccountController : Controller
    {

        [Route("Account/Login/{paramOne}")]
        public ActionResult Login(string paramOne)
        {
            Account tempAcc = new Account();
            tempAcc.CheckForRememberMe();
            tempAcc.CheckForRememberMeName();
            return View(tempAcc);
        }

        [HttpPost]
        [Route("Account/Login/CheckCredentials")]
        public ActionResult CheckCredentials(Account model)
        {

            Log.Append(String.Format("Login request created for login '{0}'...", model.Email));

            if (ModelState.IsValid)
            {
                if (Global.UserList.Any(x => x.Email.ToLower() == model.Email.ToLower() &&
                                             x.LoginPassword == model.Password))
                {
                    Log.Append("Login granted. Session for user created");

                    model.RemoveRememberMeCookie();

                    if (model.RememberMe)
                    {
                        model.CreateRememberMeName(model.Email);
                        model.CreateRememberMe(true);
                    }

                    //TODO: AUTHENTICATE SESSION
                    HttpContext.Session["Email"] = model.Email.ToLower();
                    HttpContext.Session["Department"] = Global.UserList.First(x => x.Email.ToLower() == model.Email.ToLower()).Department;

                    return Redirect("/");
                }
            }

            Log.Append("Error: Login denied. Invalid credentials provided");
            return Redirect("/Account/Login/welcomePage");
        }
        
        [Route("Account/Logout/{paramOne}")]
        public ActionResult Logout(string paramOne)
        {
            Log.Append(String.Format("Logout request for login '{0}'...", HttpContext.Session["Email"]));
            HttpContext.Session.Abandon();
            return Redirect("/Account/Login/welcomePage");
        }
    }
}