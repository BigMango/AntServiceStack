namespace CHystrix.Utils.Web
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Web;

    internal static class CrossDomainFilter
    {
        public static bool EnableCrossDomainSupport(this HttpContext current)
        {
            bool flag = false;
            HttpRequest request = current.Request;
            HttpResponse response = current.Response;
            if (request.Headers["Origin"] == null)
            {
                return flag;
            }
            response.AddHeader("Access-Control-Allow-Origin", request.Headers["Origin"]);
            if (!(request.HttpMethod == "OPTIONS") || ((request.Headers["Access-Control-Request-Method"] == null) && (request.Headers["Access-Control-Request-Headers"] == null)))
            {
                return flag;
            }
            response.AddHeader("Access-Control-Allow-Headers", request.Headers["Access-Control-Request-Headers"]);
            response.AddHeader("Access-Control-Allow-Methods", request.Headers["Access-Control-Request-Method"]);
            response.StatusCode = 200;
            return true;
        }
    }
}

