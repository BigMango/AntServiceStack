using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints.Utils;

namespace AntServiceStack.WebHost.Endpoints.Support
{
    public interface IServiceStackHttpAsyncHandler
    {
        Task ProcessRequestAsync(IHttpRequest httpReq, IHttpResponse httpRes, string operationName);
    }
}
