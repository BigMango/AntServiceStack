using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.ServiceHost;

namespace AntServiceStack.WebHost.Endpoints
{
    public class PostResponseFilterArgs
    {
        internal PostResponseFilterArgs() { }

        public IExecutionResult ExecutionResult { get; internal set; }

        public string ServicePath { get; internal set; }

        public string OperationName { get; internal set; }

        public long RequestDeserializeTimeInMilliseconds { get; internal set; }

        public long ResponseSerializeTimeInMilliseconds { get; internal set; }
    }
}
