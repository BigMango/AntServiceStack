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
            Servers = new[]
            {
                ConsulServiceResponse.Create(new ConsulHealthResponse
                {
                    Node = new Node(),
                    Service = new ConsulHealthService
                        {
                            Service = serverKey,
                            Address = address,
                            ID = serverKey
                        }
                })
            };
        }
        public ConsulServiceResponse[] Servers { get; }
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
