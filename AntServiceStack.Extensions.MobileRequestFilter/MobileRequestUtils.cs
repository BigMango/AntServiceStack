using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common;
using AntServiceStack.Common.Types;
using AntServiceStack.Common.Utils;
using AntServiceStack.ServiceHost;
using System.Web;
using System.Net;

namespace AntServiceStack.Extensions.MobileRequestFilter
{
    public static class MobileRequestUtils
    {
        internal const string H5GateWaySpecialHeaderName = "x-gate-request.toplevel.uuid";
        internal const string NonMemberAuthType = "GetOrder";

        internal const string MobileAuthLoginTypeExtensionKey = "mobile-auth-login-type";
        internal const string MobileSecondAuthExtensionKey = "sauth";
        internal const string MobileAuthCookieKey = "cticket";

        internal static bool IsNonMemberAuthLoginType(string loginType)
        {
            return string.Equals(loginType, NonMemberAuthType, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsGatewayRequest(this IHttpRequest request)
        {
            return request.Headers[H5GateWaySpecialHeaderName] != null;
        }

        public static string GetMobileAuthLoginType(this IHasMobileRequestHead request)
        {
            return request.GetExtensionData(MobileAuthLoginTypeExtensionKey);
        }

        public static string GetAuth(this IHasMobileRequestHead mobileRequest, IHttpRequest request)
        {
            if (request != null && request.OriginalRequest != null)
            {
                try
                {
                    string auth = null;
                    if (request.OriginalRequest is HttpRequest)
                        auth = GetCookieValue(request.OriginalRequest as HttpRequest, MobileAuthCookieKey);
                    else if (request.OriginalRequest is HttpListenerRequest)
                        auth = GetCookieValue(request.OriginalRequest as HttpListenerRequest, MobileAuthCookieKey);

                    if (auth != null)
                        return auth;
                }
                catch { }
            }

            if (mobileRequest == null || mobileRequest.head == null || string.IsNullOrWhiteSpace(mobileRequest.head.auth))
                return null;

            return mobileRequest.head.auth;
        }

        /// <summary>
        /// 注意：此方法依赖于线程静态数据，只能在请求执行的同步线程里使用，不能在新开启的异步线程里使用。
        /// </summary>
        /// <param name="mobileRequest"></param>
        /// <returns></returns>
        public static string GetAuth(this IHasMobileRequestHead mobileRequest)
        {
            return GetAuth(mobileRequest, HostContext.Instance.Request);
        }

        internal static string GetSAuth(this IHasMobileRequestHead mobileRequest)
        {
            if (mobileRequest == null || mobileRequest.head == null)
                return null;
            return mobileRequest.head.sauth;
        }

        private static string GetCookieValue(HttpRequest request, string cookieName)
        {
            if (request == null || request.Cookies == null)
                return null;

            for (int i = 0; i < request.Cookies.Count; i++)
            {
                HttpCookie cookie = request.Cookies[i];
                if (cookie.Name != null && cookie.Name.Equals(cookieName))
                    return cookie.Value;
            }

            return null;
        }

        private static string GetCookieValue(HttpListenerRequest request, string cookieName)
        {
            if (request == null || request.Cookies == null)
                return null;


            for (int i = 0; i < request.Cookies.Count; i++)
            {
                Cookie cookie = request.Cookies[i];
                if (cookie.Name != null && cookie.Name.Equals(cookieName))
                    return cookie.Value;
            }

            return null;
        }
    }
}
