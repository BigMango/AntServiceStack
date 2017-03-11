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

namespace AntServiceStack.Plugins.WhiteList
{
    public class IPWhiteListPlugin : WhiteListPlugin
    {
        protected const string EnableIPWhiteListCheckSettingKey = "SOA.EnableIPWhiteListCheck";
        protected const string IPWhiteListSettingKey = "SOA.IPWhiteList";

        protected override string EnableWhiteListCheckSettingKey
        {
            get { return EnableIPWhiteListCheckSettingKey; }
        }

        protected override string WhiteListSettingKey
        {
            get { return IPWhiteListSettingKey; }
        }

        protected override string Name
        {
            get
            {
                return "IP White list";
            }
        }

        protected override bool ValidateRequest(IHttpRequest request, out string requestIdentity)
        {
            requestIdentity = request.RemoteIp;

            if (WhiteListSettings[request.ServicePath].WhiteList == null || requestIdentity == null)
                return false;

            if ((requestIdentity == "::1" || requestIdentity.Contains("[::1]")) && WhiteListSettings[request.ServicePath].WhiteList.Contains("127.0.0.1"))
                return true;

            if (requestIdentity.Contains("127.0.0.1") && WhiteListSettings[request.ServicePath].WhiteList.Contains("::1"))
                return true;

            Func<List<string>, string, bool> containsIP = (whiteList, ip) =>
            {
                foreach (string item in whiteList)
                {
                    if (ip.Contains(item))
                        return true;
                }

                return false;
            };

            string ipv4 = ServiceUtils.GetIPV4Address(requestIdentity);
            if (ipv4 == null)
                return false;

            requestIdentity = ipv4;
            return containsIP(WhiteListSettings[request.ServicePath].WhiteList, requestIdentity);
        }
    }
}
