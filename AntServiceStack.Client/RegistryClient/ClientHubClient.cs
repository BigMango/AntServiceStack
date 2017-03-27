using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntServiceStack.Common.Configuration;
using AntServiceStack.Common.Consul;
using Microsoft.AspNet.SignalR.Client;

namespace AntServiceStack.Client.RegistryClient
{
    public class ClientHubClient : BaseHubClient,ILoadBalancerRequestContext
    {
        private readonly string fullName;
        public ClientHubClient(string _fullName)
        {
            if (string.IsNullOrEmpty(_fullName))
            {
                throw new ArgumentException("_fullName");
            }
            fullName = _fullName;
            Init();
        }

        public new void Init()
        {
            HubConnectionUrl = ConfigUtils.GetAppSetting("SOA.ant.url");
            if (string.IsNullOrEmpty(HubConnectionUrl))
            {
                throw new ArgumentException("SOA.ant.url");
            }
            HubProxyName = "AntSoaHub";

            base.Init();

            #region 事件注册
            _myHubProxy.On<List<ConsulServiceResponse>>("GetMyServer", Recieve_GetMyServer);
            _myHubProxy.On<List<ConsulServiceResponse>> ("UpdateServerList", Recieve_UpdateServerList);
            #endregion

            StartHubInternal();
        }
        public override void StartHub()
        {
            _hubConnection.Dispose();
            _hubConnection.Headers.Add("token", fullName);
            Init();
        }

        /// <summary>
        /// 第一次获取服务
        /// </summary>
        /// <param name="servers"></param>
        public void Recieve_GetMyServer(List<ConsulServiceResponse> servers)
        {
            _server = servers.ToArray();
        }

        /// <summary>
        /// 有服务更新
        /// </summary>
        /// <param name="servers"></param>
        public void Recieve_UpdateServerList(List<ConsulServiceResponse> servers)
        {
            _server = servers.ToArray();
        }

        #region impl

        private ConsulServiceResponse[] _server;
        public ConsulServiceResponse[] Server {
            get { return _server; } 
        }
        public void MarkServerAvailable()
        {
            throw new NotImplementedException();
        }

        public void MarkServerUnavailable()
        {
            throw new NotImplementedException();
        } 
        #endregion
    }
}
