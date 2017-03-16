using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AntServiceStack.Manager.Model.Result;
using Configuration;

namespace AntServiceStack.Manager.Filter
{
    public class GlobalHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                HandleAjaxRequestException(filterContext);
            }
            else
            {
                base.OnException(filterContext);
            }
        }

        private void HandleAjaxRequestException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }
            var errorMsg = filterContext.Exception.InnerException != null
                ? filterContext.Exception.InnerException.Message
                : filterContext.Exception.Message;

            filterContext.Result = new JsonResult
            {
                Data = new ResultJsonNoDataInfo()
                {
                    Info = errorMsg,
                    Status = ResultConfig.Fail
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}