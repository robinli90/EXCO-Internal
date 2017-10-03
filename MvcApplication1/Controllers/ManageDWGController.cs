using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.Models;

namespace MvcApplication1.Controllers
{
    public class ManageDWGController : Controller
    {
        // GET: ManageDWG
        [Route("ManageDWG/ManageDWG/{paramOne}")]
        public ActionResult ManageDWG()
        {

            return View(new DWGRecipient());
        }

        [HttpPost]
        // GET: ManageDWG
        [Route("ManageDWG/ManageDWG/AddDWGRecipient")]
        public ActionResult AddDWGRecipient(DWGRecipient model)
        {
            if (ModelState.IsValid)
            {
                DWGEmail.AddRecipient(model.customerNo, model.emailAddress);
                Log.Append("DWG Email Recipient List updated");
                // save to db, for instance
                return Redirect("/ManageDWG/ManageDWG/manage");
            }

            return View("ManageDWG", model);
        }

        // GET: ManageDWG
        [Route("ManageDWG/DeleteRecipient/{identifyString}")]
        public ActionResult DeleteRecipient(string identifyString)
        {
            if (identifyString.Contains(","))
            {
                // Delete customerNo,ListIndex
                string[] deleteSeq = identifyString.Split(new string[] {","}, StringSplitOptions.None);
                DWGEmail.DeleteRecipient(deleteSeq[0], Convert.ToInt32(deleteSeq[1]));
            }

            return Redirect("/ManageDWG/ManageDWG/manage");
        }
    }
}