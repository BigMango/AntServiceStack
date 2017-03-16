using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AntServiceStack.Manager.Controller
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.UserName = "Admin";
            return View();
        }

        public ActionResult DashBord()
        {
            return View();
        }
    }
}