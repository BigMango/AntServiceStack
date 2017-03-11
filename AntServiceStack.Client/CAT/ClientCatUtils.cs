using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common;
using AntServiceStack.Common.Utils;

namespace AntServiceStack.Client.CAT
{
    internal static class ClientCatUtils
    {
        public static void LogCallerEvent()
        {
            if (HostContext.Instance != null && HostContext.Instance.Request != null)
            {
                object caller;
                if (HostContext.Instance.Request.Items.TryGetValue(InternalServiceUtils.SOA2CurrentOperationKey, out caller))
                {
                    var callerString = caller as string;
                    if (!string.IsNullOrWhiteSpace(callerString))
                    {
                        //Cat.LogEvent(ClientCatConstants.SOA2ClientCallerCatKey, callerString);
                    }
                }
            }
        }
    }
}
