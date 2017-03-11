// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Configuration;
using System.Web;
using AntServiceStack.Common.Consul;
using AntServiceStack.Common.Consul.Discovery;
using AntServiceStack.Common.Consul.Dtos;
using AntServiceStack.Common.Web;
using AntServiceStack.Plugins.Consul.Services;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;

namespace AntServiceStack.Plugins.Consul
{
    /// <summary>
    /// Enables remote service calls by dynamically looking up remote service url
    /// </summary>
    public class ConsulFeature : IPlugin
    {
        private IServiceDiscovery<ConsulService, ServiceRegistration> ServiceDiscovery { get; set; }

        public ConsulFeatureSettings Settings { get; }

        /// <summary>
        /// Enables service discovery using consul to resolve the correct url for a remote RequestDTO
        /// </summary>
        public ConsulFeature(ConsulSettings settings = null)
        {
            Settings = new ConsulFeatureSettings();
            settings?.Invoke(Settings);
        }
        
        public void Register(IAppHost appHost)
        {

            if (string.IsNullOrEmpty(ConsulUris.LocalAgent))
            {
                throw new ConfigurationErrorsException("can not find SOA.consul.url in Appseting file");
            }
            // HACK: not great but unsure how to improve
            // throws exception if WebHostUrl isn't set as this is how we get endpoint url:port
            //if (appHost.Config?.WebHostUrl == null)
            //{
            //    if (appHost.Config?.WebHostIP == null)
            //        throw new ApplicationException("appHost.Config.WebHostUrl or WebHostIp must be set to use the Consul plugin, this is so consul will know the full external http://url:port for the service");
            //}

            // register callbacks
            appHost.AfterInitCallbacks.Add(RegisterService);
            appHost.OnDisposeCallbacks.Add(UnRegisterService);

            appHost.Config.RawHttpHandlers.Add(ConsulResolveHttpHandler);
            //appHost.RegisterService<HealthCheckService>();
            //appHost.RegisterService<DiscoveryService>();

            // register plugin link
            //appHost.GetPlugin<MetadataFeature>()?.AddPluginLink(ConsulUris.LocalAgent.CombineWith("ui"), "Consul Agent WebUI");


            //获取当前的服务器的内网IP Or 外网IP OR 外部指定的IP
            //当所有的init动作全部完成之后 开始注册服务ip到consul里面
            //当关闭时从consul里面下掉

            //指定2个服务给consul服务
            // 1.HealthCheck接口

            // 2.服务发现接口


        }

        private IHttpHandler ConsulResolveHttpHandler(IHttpRequest request)
        {
            if (request.HttpMethod == HttpMethods.Post)
                return null;

            var paths = GetPathController(request);
            if (paths == null) return null;

            if (paths.EndsWith("/json/consul/heartbeat"))
            {
                return new ConsulHeartbeatHandler(request.ServicePath);
            }
            return null;
        }

        private string GetPathController(IHttpRequest request)
        {
            var pathParts = request.PathInfo;
            return pathParts;
        }

        private void RegisterService(IAppHost host)
        {
            ServiceDiscovery = Settings.GetDiscoveryClient() ?? new ConsulDiscovery();
            ServiceDiscovery.Register(host);

            // register servicestack discovery services
            host.Register(ServiceDiscovery);
           
        }

        private void UnRegisterService(IAppHost host = null)
        {
            ServiceDiscovery.Unregister(host);
        }
    }

    public delegate HealthCheck HealthCheckDelegate(IAppHost appHost);


    public delegate void ConsulSettings(ConsulFeatureSettings settings);
}