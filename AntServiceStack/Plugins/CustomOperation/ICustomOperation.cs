using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.ServiceHost;

namespace AntServiceStack.Plugins.CustomOperation
{
    public interface ICustomOperation
    {
        Type RequestDTOType { get; }
        string OperationRestPath { get; }
        bool IsValidRequest(IHttpRequest request, object requestDTO);
        object ExecuteOperation(IHttpRequest request, object requestDTO);
    }
}
