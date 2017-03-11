namespace CHystrix.Web
{
    using System;
    using System.Web;
    using System.Web.Routing;

    internal class HystrixRouteHandler : IRouteHandler, IHttpHandler
    {
        public const string ActionVariable = "action";
        public const string ControllerVariable = "controller";
        public const string HystrixRoutePrefix = "__chystrix";
        public const string Route = "{controller}/{*action}";

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            if ((requestContext.RouteData != null) && (requestContext.RouteData.Values != null))
            {
                if (!requestContext.RouteData.Values.ContainsKey("controller") || (requestContext.RouteData.Values["controller"] == null))
                {
                    return this;
                }
                if (requestContext.RouteData.Values["controller"].ToString().ToLower() != "__chystrix")
                {
                    return this;
                }
                if (!requestContext.RouteData.Values.ContainsKey("action") || (requestContext.RouteData.Values["action"] == null))
                {
                    return this;
                }
                string[] strArray = requestContext.RouteData.Values["action"].ToString().ToLower().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (strArray.Length == 0)
                {
                    return this;
                }
                switch (strArray[0].Trim().ToLower())
                {
                    case "_hystrix_stream":
                        return new HystrixStreamHandler();

                    case "_config":
                        return new HystrixConfigHandler();

                    case "_metrics":
                        return new HystrixMetricsHandler();

                    case "_command":
                        return new HystrixCommandHandler();
                }
            }
            return this;
        }

        public void ProcessRequest(HttpContext context)
        {
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}

