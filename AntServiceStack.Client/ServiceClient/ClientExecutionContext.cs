using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Execution;
using AntServiceStack.Common.ServiceClient;

namespace AntServiceStack.ServiceClient
{
    public class ClientExecutionContext : ExecutionContext
    {
        public string Accept 
        {
            get
            {
                return ContentType;
            }
        }

        public string ContentType
        {
            get
            {
                if (CallFormat == null)
                    return null;
                return CallFormat.ContentType;
            }
        }

        public IClientCallFormat CallFormat { get; internal set; }


        internal string Host { get; set; }
    }
}
