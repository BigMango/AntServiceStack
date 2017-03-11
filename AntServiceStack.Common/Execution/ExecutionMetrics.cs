using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Execution
{
    public class ExecutionMetrics
    {
        public long? RequestSize { get; set; }

        public long? ResponseSize { get; set; }

        public long? ExecutionTime { get; set; }

        public long? SerializationTime { get; set; }

        public long? DeserializationTime { get; set; }

        public long? TotalTime { get; set; }
    }
}
