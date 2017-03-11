using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using AntServiceStack.Common.Utils;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Utils;
using AntServiceStack.WebHost.Endpoints.Extensions;

namespace AntServiceStack.Plugins.BlackList
{
    public class IPBlackListPlugin : BlackListPlugin
    {
        protected const string EnableIPBlackListCheckSettingKey = "SOA.EnableIPBlackListCheck";
        protected const string IPBlackListSettingKey = "SOA.IPBlackList";

        protected override string EnableBlackListCheckSettingKey
        {
            get { return EnableIPBlackListCheckSettingKey; }
        }

        protected override string BlackListSettingKey
        {
            get { return IPBlackListSettingKey; }
        }

        protected override string Name
        {
            get
            {
                return "IP Black list";
            }
        }

        protected override bool ValidateRequest(IHttpRequest request, out string requestIdentity)
        {
            requestIdentity = request.RemoteIp;

            if (BlackListSettings[request.ServicePath].BlackList == null || requestIdentity == null)
                return true;

            if ((requestIdentity == "::1" || requestIdentity.Contains("[::1]")) && BlackListSettings[request.ServicePath].BlackList.Contains("127.0.0.1"))
                return false;

            if (requestIdentity.Contains("127.0.0.1") && BlackListSettings[request.ServicePath].BlackList.Contains("::1"))
                return false;

            Func<List<string>, string, bool> containsIP = (blackList, ip) =>
            {
                foreach (string item in blackList)
                {
                    if (ip.Contains(item))
                        return true;
                }

                return false;
            };

            string ipv4 = ServiceUtils.GetIPV4Address(requestIdentity);
            if (ipv4 == null)
                return true;

            requestIdentity = ipv4;
            return !containsIP(BlackListSettings[request.ServicePath].BlackList, requestIdentity);
        }
    }
}
