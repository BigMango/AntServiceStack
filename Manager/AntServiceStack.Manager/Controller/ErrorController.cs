using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AntServiceStack.Manager.Model.Result;
using Configuration;

namespace AntServiceStack.Manager.Controller
{
    public class ErrorController : BaseController
    {
        public ActionResult Http404()
        {
            return View();
        }

        public ActionResult Http403(string userInfo)
        {
            ViewBag.userInfo = userInfo;
            return View();
        }

        public ActionResult NoPower(string acionInfo)
        {
            ViewBag.ActionInfo = acionInfo;
            return View();
        }

        /// <summary>
        /// 未登录
        /// </summary>
        //public ActionResult NoLogin()
        //{
        //    return RedirectToAction("Login", "Account");
        //}

        /// <summary>
        /// 没有权限
        /// </summary>
        /// <returns>ActionResult.</returns>
        public JsonResult Http401()
        {
            var result = new ResultJsonNoDataInfo();
            result.Status = ResultConfig.Fail;
            result.Info = ResultConfig.FailMessageForNoPower;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 系统错误
        /// </summary>
        /// <returns></returns>
        public JsonResult Http500()
        {
            var result = new ResultJsonNoDataInfo();
            result.Status = ResultConfig.Fail;
            result.Info = ResultConfig.FailMessageForSystem;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Http405()
        {
            var result = new ResultJsonNoDataInfo();
            result.Status = ResultConfig.Fail;
            result.Info = ResultConfig.FailMessageForNotFound;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}