using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.ServiceHost;

namespace AntServiceStack.ServiceHost
{
    public interface IRequiresHttpResponse
    {
        IHttpResponse HttpResponse { get; set; }
    }
}
