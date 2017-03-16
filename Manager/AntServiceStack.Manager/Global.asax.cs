using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using AntServiceStack.Manager.Controller;
using AntServiceStack.Manager.Model.JsonNet;

namespace AntServiceStack.Manager
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            ValueProviderFactories.Factories.Remove(ValueProviderFactories.Factories.OfType<JsonValueProviderFactory>().FirstOrDefault());
            ValueProviderFactories.Factories.Add(new JsonNetValueProviderFactory());

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            ModelBundle.RegisterBindles(ModelBinders.Binders);
        }

        

        protected void Application_Error(object sender, EventArgs e)
        {
            var app = (Global)sender;
            var context = app.Context;
            var ex = app.Server.GetLastError();
            context.Response.Clear();
            context.ClearError();
            var httpException = ex as HttpException;
            var routeData = new RouteData();
            routeData.Values["controller"] = "error";
            routeData.Values["exception"] = ex;
            routeData.Values["action"] = "http500";
            if (httpException != null)
            {

                var code = httpException.GetHttpCode();
                if (code == 404)
                {
                    if (IsAjaxRequest(HttpContext.Current))
                    {
                        code = 405;
                    }
                }
                switch (code)
                {
                    case 405:
                        routeData.Values["action"] = "http405";
                        break;
                    case 404:
                        routeData.Values["action"] = "http404";
                        break;
                    case 401:
                        routeData.Values["action"] = "http401";
                        break;
                    case 400:
                        routeData.Values["action"] = "NoLogin";
                        break;
                    case 500:
                        break;
                }
            }
            IController controller = new ErrorController();
            controller.Execute(new RequestContext(new HttpContextWrapper(context), routeData));
        }

        public static bool IsAjaxRequest(HttpContext context)
        {
            return ((context.Request["X-Requested-With"] == "XMLHttpRequest") || ((context.Request.Headers != null) && (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")));
        }
    }
}