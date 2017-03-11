using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.ServiceHost
{
    internal class ServiceCatConstants
    {
        public const string ResponseSizeCatKey = "SOA2Service.resSize";
        public const string SOA2ServiceVersionCatKey = "SOA2Service.version";
        public const string SOA2ServiceCallFormatCatKey = "SOA2Service.callFormat";
        public const string SOA2ServiceAckEventCatKey = "SOA2Service.ack";

        public const string CreateExecutionTaskTransactionName = "SOA2AsyncService.createExecutionTask";
        public const string HandleExecutionTaskTransactionName = "SOA2AsyncService.handleExecutionResult";
        public const string WriteResponseTransactionName = "SOA2AsyncService.writeResponse";

        public const string SOA2AsyncServiceTransactionName = "SOA2AsyncService";

        public const string SOA2AsyncServiceStartTimeKey = "SOA2AsyncService.StartTime";
    }
}
