using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using MvcApplication1.Models;
using MvcApplication1.Paperless_System.PDF_Generators;

namespace MvcApplication1.Controllers
{
    public class EmailRepositoryController : Controller
    {
        // GET: EmailRepository
        [Route("EmailRepository/OrganizeEmails/{paramOne}")]
        public ActionResult OrganizeEmails()
        {
            return View();
        }

        // GET: EmailRepository
        [Route("EmailRepository/SearchEmails/{paramOne}")]
        public ActionResult SearchEmails()
        {
            return View();
        }

        [HttpPost]
        // GET: EmailRepository
        [Route("EmailRepository/SearchEmailsById/{id}")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string SearchEmailsById(string id)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            Email refEmail = Global.EmailList.FirstOrDefault(x => x.ID == id);
            refEmail.RetrieveMsg();
            string serialized = js.Serialize(refEmail);
            return serialized;
        }

        [HttpGet]
        // GET: EmailRepository
        [Route("EmailRepository/OpenFileById/{id}")]
        public FileResult OpenFileById(string id)
        {

            string filePath = Global.messagesDirectoryPath + id + ".eml";
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            string fileName = "emailFile_" + string.Format("{0}_{1}_{2}.eml", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Year);
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}