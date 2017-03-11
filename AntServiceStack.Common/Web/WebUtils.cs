using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;

using AntServiceStack.ServiceHost;

namespace AntServiceStack.Common.Web
{
    public static class WebUtils
    {
        public static string GetRequestReferer()
        {
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Headers != null)
                    return HttpContext.Current.Request.Headers[HttpHeaders.Referer];
            }
            catch { }

            return null;
        }

        public static string GetRequestAbsoluteUri()
        {
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Url != null)
                    return HttpContext.Current.Request.Url.AbsoluteUri;
            }
            catch { }

            return null;
        }

        public static bool EnableCrossDomainSupport(this HttpContext current)
        {
            bool isPreflight = false;

            HttpRequest request = current.Request;
            HttpResponse response = current.Response;
            if (request.Headers[HttpHeaders.Origin] != null)
            {
                response.AddHeader(HttpHeaders.AllowOrigin, request.Headers[HttpHeaders.Origin]);

                //preflight request
                if (request.HttpMethod == HttpMethods.Options
                    && (request.Headers[HttpHeaders.RequestMethod] != null || request.Headers[HttpHeaders.RequestHeaders] != null))
                {
                    response.AddHeader(HttpHeaders.AllowHeaders, request.Headers[HttpHeaders.RequestHeaders]);
                    response.AddHeader(HttpHeaders.AllowMethods, request.Headers[HttpHeaders.RequestMethod]);

                    response.StatusCode = (int)HttpStatusCode.OK;
                    isPreflight = true;
                }
            }

            return isPreflight;
        }

        public static bool EnableCrossDomainSupport(IHttpRequest request, IHttpResponse response)
        {
            bool isPreflight = false;
            if (request.Headers[HttpHeaders.Origin] != null)
            {
                response.AddHeader(HttpHeaders.AllowOrigin, request.Headers[HttpHeaders.Origin]);

                //preflight request
                if (request.HttpMethod == HttpMethods.Options
                    && (request.Headers[HttpHeaders.RequestMethod] != null || request.Headers[HttpHeaders.RequestHeaders] != null))
                {
                    response.AddHeader(HttpHeaders.AllowHeaders, request.Headers[HttpHeaders.RequestHeaders]);
                    response.AddHeader(HttpHeaders.AllowMethods, request.Headers[HttpHeaders.RequestMethod]);

                    response.StatusCode = (int)HttpStatusCode.OK;
                    isPreflight = true;
                }
            }

            return isPreflight;
        }
    }
}
