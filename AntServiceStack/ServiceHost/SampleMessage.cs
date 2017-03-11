using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.ServiceHost
{
    public class SampleMessage
    {
        public object Request { get; set; }

        public object Response { get; set; }

        public SampleMessage(object request, object response)
        {
            this.Request = request;
            this.Response = response;
        }
    }
}
