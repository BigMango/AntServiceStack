using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Configuration;
using AntServiceStack.Common.Consul;

namespace AntServiceStack.Client.RegistryClient
{
    internal class ConsulLoadBalancerRequestContext: ILoadBalancerRequestContext
    {

        public ConsulLoadBalancerRequestContext(string serviceKey,string version = null)
        {
           
            if (string.IsNullOrEmpty(ConsulUris.LocalAgent))
            {
                throw new ConfigurationErrorsException("can not find SOA.consul.url in Appseting file");
            }

            if (string.IsNullOrEmpty(version))
            {
                //_server = ConsulClient.GetServices(serviceKey);
                _servers = ConsulClient.GetServices(serviceKey).GroupBy(r => r.ServiceAddress, y => y).Select(r => r.First()).ToArray();
            }
            else
            {
                _servers =new []{ ConsulClient.GetService(serviceKey,version)};
            }

        }

        private ConsulServiceResponse[] _servers;
        public ConsulServiceResponse[] Servers { get {return _servers; } }
        public void MarkServerAvailable()
        {
            throw new NotImplementedException();
        }
                    
        public void MarkServerUnavailable()
        {
            throw new NotImplementedException();
        }
    }
}
