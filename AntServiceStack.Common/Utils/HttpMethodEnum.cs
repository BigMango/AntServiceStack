using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Utils
{
    public enum HttpMethodEnum
    {
        POST,
        PUT,
        GET,
        DELETE,
        NotSupported
    }

    public static class HttpMethodsExtension
    {
        static readonly string[] Methods;

        static HttpMethodsExtension()
        {
            Methods = Enum.GetNames(typeof(HttpMethodEnum));
        }

        public static HttpMethodEnum ToHttpMethod(this string httpMethod)
        {
            if (string.IsNullOrWhiteSpace(httpMethod))
                return HttpMethodEnum.NotSupported;

            httpMethod = httpMethod.Trim().ToUpper();
            if (Methods.Contains(httpMethod))
                return (HttpMethodEnum)Enum.Parse(typeof(HttpMethodEnum), httpMethod);

            return HttpMethodEnum.NotSupported;
        }
    }
}
