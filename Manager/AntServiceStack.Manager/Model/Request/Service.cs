using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntServiceStack.Manager.Model.Condition;

namespace AntServiceStack.Manager.Model.Request
{
    public class ServiceVm:ConditionBase
    {
        public string ServiceName { get; set; } 
    }
}