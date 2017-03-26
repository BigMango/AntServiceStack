using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using AntData.ORM.Common;
using AntServiceStack.Common.Consul;
using AntServiceStack.Manager.Model.Request;
using AntServiceStack.Manager.Repository;
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
        public void Heartbeat()
        {
            Clients.Caller.SendMessage("", "");
        }

        /// <summary>
        /// client主动获取服务
        /// </summary>
        public void GetMyServer()
        {
            var result = new List<ConsulServiceResponse>();
            var serviceFullName = GetServiceFullName();
            if (!string.IsNullOrEmpty(serviceFullName))
            {
                _slabLogger.Error("GetMyServer", "get serviceFullName error :" +　Context.ConnectionId);
                Clients.Caller.GetMyServer(result);
            }
            var respositoryResult = ConsulClient.GetServices(serviceFullName).ToList();
            if (respositoryResult.Count < 1)
            {
                _slabLogger.Error("GetMyServer",
                    "serviceFullName:{0} can not found any server :{1}".Args(serviceFullName, Context.ConnectionId));
            }
            else
            {
                //有服务才加组
                //SubscribeGroup(serviceFullName);
            }
            Clients.Caller.GetMyServer(respositoryResult);
        }

        /// <summary>
        /// 加入组  client加入group
        /// </summary>
        /// <param name="groupName"></param>
        public Task SubscribeGroup()
        {
            return Groups.Add(Context.ConnectionId, GetServiceFullName());
        }

        /// <summary>
        /// 离开组
        /// </summary>
        /// <param name="groupName"></param>
        public Task UnsubscribeGroup(string groupName)
        {
            return Groups.Remove(Context.ConnectionId, groupName);
        }

       

        public override Task OnConnected()
        {
            //$.connection.hub.qs = { "token" : tokenValue };
            //$.connection.hub.start().done(function() { /* ... */ });
            var serviceFullName = GetServiceFullName(true);
            if (!string.IsNullOrEmpty(serviceFullName))
            {
                //GetMyServer();
               // Heartbeat();
                //SubscribeGroup(serviceFullName);
            }
            _slabLogger.Info("OnConnected", Context.ConnectionId);
            return (base.OnConnected());
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var serviceFullName = GetServiceFullName(true);
            if (!string.IsNullOrEmpty(serviceFullName))
            {
                //离开退组
                UnsubscribeGroup(serviceFullName);
            }
            _slabLogger.Info("OnDisconnected", Context.ConnectionId);
            return (base.OnDisconnected(stopCalled));
        }

        public override Task OnReconnected()
        {
            var serviceFullName = GetServiceFullName(true);
            if (!string.IsNullOrEmpty(serviceFullName))
            {
                //GetMyServer();
                Heartbeat();
            }
            _slabLogger.Info("OnReconnected", Context.ConnectionId);
            return (base.OnReconnected());
        }

        /// <summary>
        /// 获取注册服务名称
        /// </summary>
        /// <returns></returns>
        public string GetServiceFullName(bool isInit = false)
        {
            if (isInit)
            {
                var principal = Context.Request.Environment[HubAuthorizeAttribute.EnvironmentUser] as ClaimsPrincipal;
                if (principal != null)
                {
                    var claim = principal.Claims.FirstOrDefault(r => r.Type.Equals(ClaimTypes.Authentication));
                    if (claim != null)
                    {
                        return claim.Value;
                    }
                }
                else
                {
                    return HubAuthorizeAttribute.GetToken(Context.Request);
                }
            }
            else
            {
                ClaimsIdentity identity = Context.User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    var claim = identity.FindFirst(ClaimTypes.Authentication);
                    if (claim != null)
                    {
                        return claim.Value;
                    }
                }
            }
            return string.Empty;
        }
    }
}