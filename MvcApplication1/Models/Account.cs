using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using Org.BouncyCastle.Asn1.Ocsp;


namespace MvcApplication1.Models
{
    public class Account
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        private const string RememberMeNameCookie = "EXCOAPI_REMEMBERNAME";
        private const string RememberMeCookie = "EXCOAPI_REMEMBERME";

        public void CheckForRememberMeName()
        {
            
            string returnValue = string.Empty;
            HttpCookie rememberMeUserNameCookie = HttpContext.Current.Request.Cookies.Get(RememberMeNameCookie);
            if (null != rememberMeUserNameCookie)
            {
                /* Note, the browser only sends the name/value to the webserver, and not the expiration date */
                returnValue = rememberMeUserNameCookie.Value;
            }

            Email = returnValue;
            //return returnValue;
        }
        public void CheckForRememberMe()
        {
            
            bool returnValue = false;
            HttpCookie rememberMeUserNameCookie = HttpContext.Current.Request.Cookies.Get(RememberMeCookie);
            if (null != rememberMeUserNameCookie)
            {
                /* Note, the browser only sends the name/value to the webserver, and not the expiration date */
                returnValue = rememberMeUserNameCookie.Value == "1";
            }

            RememberMe = returnValue;
            //return returnValue;
        }

        public void CreateRememberMeName(string userName)
        {
            HttpCookie rememberMeCookie = new HttpCookie(RememberMeNameCookie, userName);
            rememberMeCookie.Expires = DateTime.MaxValue;
            HttpContext.Current.Response.SetCookie(rememberMeCookie);
        }
        public void CreateRememberMe(bool rememberMe)
        {
            HttpCookie rememberMeCookie = new HttpCookie(RememberMeCookie, rememberMe ? "1" : "0");
            rememberMeCookie.Expires = DateTime.MaxValue;
            HttpContext.Current.Response.SetCookie(rememberMeCookie);
        }

        public void RemoveRememberMeCookie()
        {
            /* k1ll the cookie ! */
            HttpCookie rememberMeUserNameCookie = HttpContext.Current.Request.Cookies[RememberMeNameCookie];
            if (null != rememberMeUserNameCookie)
            {
                HttpContext.Current.Response.Cookies.Remove(RememberMeNameCookie);
                rememberMeUserNameCookie.Expires = DateTime.Now.AddYears(-1);
                rememberMeUserNameCookie.Value = null;
                HttpContext.Current.Response.SetCookie(rememberMeUserNameCookie);
            }
            rememberMeUserNameCookie = HttpContext.Current.Request.Cookies[RememberMeCookie];
            if (null != rememberMeUserNameCookie)
            {
                HttpContext.Current.Response.Cookies.Remove(RememberMeCookie);
                rememberMeUserNameCookie.Expires = DateTime.Now.AddYears(-1);
                rememberMeUserNameCookie.Value = null;
                HttpContext.Current.Response.SetCookie(rememberMeUserNameCookie);
            }
        }
    }
}