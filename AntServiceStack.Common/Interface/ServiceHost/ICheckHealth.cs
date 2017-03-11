using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Types;

namespace AntServiceStack.Common.Interface.ServiceHost
{
    public interface ICheckHealth
    {
        CheckHealthResponseType CheckHealth(CheckHealthRequestType request);
    }
}
