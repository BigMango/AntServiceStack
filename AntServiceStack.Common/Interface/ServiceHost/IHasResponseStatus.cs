using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Types;

namespace AntServiceStack.ServiceHost
{
    public interface IHasResponseStatus
    {
        ResponseStatusType ResponseStatus { get; set; }
    }
}
