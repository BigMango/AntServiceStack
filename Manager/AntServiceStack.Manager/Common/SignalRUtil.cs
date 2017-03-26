using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntServiceStack.Common.Consul;
using AntServiceStack.Manager.SignalR;
using Microsoft.AspNet.SignalR;

namespace AntServiceStack.Manager.Common
{
    public class SignalRUtil
    {
        /// <summary>
        /// 服务节点变更通知
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="server"></param>
        public static void PushServerToGroup(string groupName,List<ConsulServiceResponse> server )
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<AntSoaHub>();
            context.Clients.Group(groupName).UpdateServerList(server);
        }


    }
}