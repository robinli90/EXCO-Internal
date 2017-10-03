using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.Services;

namespace MvcApplication1.Controllers
{
    public class HomeController : Controller
    {
        [SessionAuthorize]
        public ActionResult Index()
        {
            return View();
        }
    }
}
