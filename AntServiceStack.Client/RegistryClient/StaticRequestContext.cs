using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Consul;

namespace AntServiceStack.Client.RegistryClient
{
    public class StaticRequestContext: ILoadBalancerRequestContext
    {
        public StaticRequestContext(string serverKey, string address)
        {
            Server = new[]
            {
                new ConsulServiceResponse
                {
                    ServiceID = serverKey,
                    ServiceAddress = address
                }
            };
        }
        public ConsulServiceResponse[] Server { get; }
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
