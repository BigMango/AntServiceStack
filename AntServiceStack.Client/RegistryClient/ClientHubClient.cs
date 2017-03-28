using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntServiceStack.Common.Configuration;
using AntServiceStack.Common.Consul;
using Freeway.Logging;
using Microsoft.AspNet.SignalR.Client;

namespace AntServiceStack.Client.RegistryClient
{
    internal class ClientHubClient : BaseHubClient,ILoadBalancerRequestContext,IDisposable
    {
        public static ClientHubClient Instance { get; private set; }

        static ClientHubClient()
        {
            Instance = new ClientHubClient();
        }

        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClientHubClient));
        public string fullName { get; set; }

        public ClientHubClient()
        {
        }

        public ClientHubClient Start(string _fullName)
        {
            fullName = _fullName;
            Init();
            return this;
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
            
            Init();
        }

        /// <summary>
        /// 第一次获取服务
        /// </summary>
        /// <param name="servers"></param>
        public void Recieve_GetMyServer(List<ConsulServiceResponse> servers)
        {
            _logger.Info("Recieve_GetMyServer raising :" + string.Join("||", servers.Select(r => r.ServiceAddress).ToArray()));
            Servers = servers.ToArray();
        }

        /// <summary>
        /// 有服务更新
        /// </summary>
        /// <param name="servers"></param>
        public void Recieve_UpdateServerList(List<ConsulServiceResponse> servers)
        {
            _logger.Info("Recieve_UpdateServerList raising :" + string.Join("||", servers.Select(r => r.ServiceAddress).ToArray()));
            Servers = servers.ToArray();
        }

        #region impl

        private List<ConsulServiceResponse> _servers;
        public ConsulServiceResponse[] Servers {
            get
            {
                if (_servers == null)
                    _servers = new List<ConsulServiceResponse>();
                return _servers.ToArray();
            }
            private set
            {
                _servers = new List<ConsulServiceResponse>(value);
                //if (OnChange != null)
                //{
                //    try
                //    {
                //        OnChange(this, new EventArgs());
                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.Warn("Error occurred while raising \"OnServerChange\" event!", ex);
                //    }
                //}
            }
        }
        public void MarkServerAvailable()
        {
            throw new NotImplementedException();
        }

        public void MarkServerUnavailable()
        {
            throw new NotImplementedException();
        }

        public override string GetToken()
        {
            return fullName;
        }

        #endregion

        public void Dispose()
        {
           this.CloseHub();
        }
    }
}
