using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Configuration;
using AntServiceStack.ServiceHost;

namespace AntServiceStack.Common.Utils
{
    public static class AppIdExtensions
    {
        private const string AppIdSettingKey = "AppId";

        /// <summary>
        /// If appId is null, auto get it from configuration.
        /// </summary>
        /// <param name="generalServiceClient"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static GeneralServiceClient WithAppId(this GeneralServiceClient generalServiceClient, string appId = null)
        {
            if (generalServiceClient == null)
                return null;

            if (string.IsNullOrWhiteSpace(appId))
                appId = ConfigUtils.GetNullableAppSetting(AppIdSettingKey);

            if (!string.IsNullOrWhiteSpace(appId))
            {
                generalServiceClient.Headers.Remove(ServiceUtils.AppIdHttpHeaderKey);
                generalServiceClient.Headers.Add(ServiceUtils.AppIdHttpHeaderKey, appId.Trim());
            }

            return generalServiceClient;
        }

        public static string GetAppId(this IHttpRequest httpRequest)
        {
            if (httpRequest == null)
                return null;

            string appId = httpRequest.Headers[ServiceUtils.AppIdHttpHeaderKey];
            if (string.IsNullOrWhiteSpace(appId))
                appId = null;
            return appId;
        }
    }
}
