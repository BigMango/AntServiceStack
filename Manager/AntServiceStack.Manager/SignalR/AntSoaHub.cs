using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace AntServiceStack.Manager.SignalR
{
    [HubAuthorize]
    public class AntSoaHub : Hub
    {
        private readonly IHubLogger _slabLogger;
        public AntSoaHub(IHubLogger slabLogger)
        {
            _slabLogger = slabLogger;
        }

        /// <summary>
        /// 心跳
        /// </summary>
        public void Heartbeat()
        {
            Clients.Caller.Heartbeat();
        }

        public override Task OnConnected()
        {
            //$.connection.hub.qs = { "token" : tokenValue };
            //$.connection.hub.start().done(function() { /* ... */ });
            _slabLogger.Info("OnConnected", Context.ConnectionId);
            return (base.OnConnected());
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _slabLogger.Info("OnDisconnected", Context.ConnectionId);
            return (base.OnDisconnected(stopCalled));
        }

        public override Task OnReconnected()
        {
            _slabLogger.Info("OnReconnected", Context.ConnectionId);
            return (base.OnReconnected());
        }

        /// <summary>
        /// 获取注册服务名称
        /// </summary>
        /// <returns></returns>
        public string GetServiceFullName()
        {
            var user = Context.User;
            var identity = Context.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var claim = identity.FindFirst(ClaimTypes.Authentication);
                if (claim != null)
                {
                    var domain = claim.Value;
                    return domain;
                }
            }

            return string.Empty;
        }
    }
}