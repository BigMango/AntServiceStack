using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.Plugins.ConfigInfo;
using AntServiceStack.ServiceHost;
using AntServiceStack.Common.Hystrix.Atomic;
using AntServiceStack.Common.Hystrix;

namespace AntServiceStack.Plugins.RequestCounter
{
    internal class AsyncRequestCounterPlugin : IPlugin, IHasConfigInfo
    {
        public void Register(IAppHost appHost)
        {
            ConfigInfoHandler.RegisterConfigInfoOwner(this);
        }

        public IEnumerable<KeyValuePair<string, object>> GetConfigInfo(string servicePath)
        {
            var operationConcurrentRequestCountMap = new Dictionary<string, int>();
            var operationMaxConcurrentRequestCountMap = new Dictionary<string, int>();

            var metadata = EndpointHost.MetadataMap[servicePath];
            foreach (var operation in metadata.OperationNameMap.Values)
            {
                if (operation.IsAsync)
                {
                    operationConcurrentRequestCountMap.Add(operation.Name, operation.HystrixCommand.Metrics.CurrentConcurrentExecutionCount);
                    operationMaxConcurrentRequestCountMap.Add(operation.Name, operation.HystrixCommand.Metrics.MaxConcurrentExecutionCount);
                }
            }

            return new Dictionary<string, object>() 
            {
                {"SOA.ServiceAsyncRequestCount.Current", metadata.ServiceCurrentConcurrentExecutionCount },
                {"SOA.ServiceAsyncRequestCount.Max", metadata.ServiceMaxConcurrentExecutionCount},
                {"SOA.OperationAsyncRequestCount.Current", operationConcurrentRequestCountMap},
                {"SOA.OperationAsyncRequestCount.Max", operationMaxConcurrentRequestCountMap},
            };
        }
    }
}
