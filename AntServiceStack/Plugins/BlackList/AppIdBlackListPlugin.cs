using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
    public class AppIdBlackListPlugin : BlackListPlugin
    {
        protected const string EnableAppIdBlackListCheckSettingKey = "SOA.EnableAppIdBlackListCheck";
        protected const string AppIdBlackListSettingKey = "SOA.AppIdBlackList";

        protected override string EnableBlackListCheckSettingKey
        {
            get { return EnableAppIdBlackListCheckSettingKey; }
        }

        protected override string BlackListSettingKey
        {
            get { return AppIdBlackListSettingKey; }
        }

        protected override string Name
        {
            get
            {
                return "AppId Black list";
            }
        }

        protected override bool ValidateRequest(IHttpRequest request, out string requestIdentity)
        {
            requestIdentity = request.Headers[ServiceUtils.AppIdHttpHeaderKey];
            return string.IsNullOrWhiteSpace(requestIdentity)
                || BlackListSettings[request.ServicePath].BlackList.Count == 0 
                || !BlackListSettings[request.ServicePath].BlackList.Contains(requestIdentity);
        }
    }
}
