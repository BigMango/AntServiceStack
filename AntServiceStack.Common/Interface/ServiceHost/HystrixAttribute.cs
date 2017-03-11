using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.ServiceHost
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HystrixAttribute : Attribute
    {

        public HystrixAttribute()
        {
            Timeout = 2000;
        }

        /// <summary>
        /// Service invocation timeout, must be > 0, default is 2000, unit is milliseconds
        /// 
        /// This is passive timeout, which means event if the service invocation
        /// has been successfully executed, it will still be counted as timeout 
        /// if the total service invocation time > this timeout setting.
        /// </summary>
        public int Timeout { get; set; }
    }
}
