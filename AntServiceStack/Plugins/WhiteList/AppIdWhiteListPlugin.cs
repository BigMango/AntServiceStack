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

namespace AntServiceStack.Plugins.WhiteList
{
    public class AppIdWhiteListPlugin : WhiteListPlugin
    {
        protected const string EnableAppIdWhiteListCheckSettingKey = "SOA.EnableAppIdWhiteListCheck";
        protected const string AppIdWhiteListSettingKey = "SOA.AppIdWhiteList";

        protected override string EnableWhiteListCheckSettingKey
        {
            get { return EnableAppIdWhiteListCheckSettingKey; }
        }

        protected override string WhiteListSettingKey
        {
            get { return AppIdWhiteListSettingKey; }
        }

        protected override string Name
        {
            get
            {
                return "AppId White list";
            }
        }

        protected override bool ValidateRequest(IHttpRequest request, out string requestIdentity)
        {
            requestIdentity = request.Headers[ServiceUtils.AppIdHttpHeaderKey];
            return !string.IsNullOrWhiteSpace(requestIdentity)
                && (WhiteListSettings[request.ServicePath].WhiteList.Count == 0
                    || WhiteListSettings[request.ServicePath].WhiteList.Contains(requestIdentity));
        }
    }
}
