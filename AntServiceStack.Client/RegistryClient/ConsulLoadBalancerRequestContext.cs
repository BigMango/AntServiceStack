using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Configuration;
using AntServiceStack.Common.Consul;

namespace AntServiceStack.Client.RegistryClient
{
    public class ConsulLoadBalancerRequestContext: ILoadBalancerRequestContext
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
                _server = ConsulClient.GetServices(serviceKey).GroupBy(r => r.ServiceAddress, y => y).Select(r => r.First()).ToArray();
            }
            else
            {
                _server =new []{ ConsulClient.GetService(serviceKey,version)};
            }

        }

        private ConsulServiceResponse[] _server;
        public ConsulServiceResponse[] Server { get {return _server; } }
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
