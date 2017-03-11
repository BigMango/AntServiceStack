using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.ServiceClient
{
    /// <summary>
    /// 服务注册表查找失败相关例外
    /// </summary>
    [Serializable]
    public class ServiceLookupException : Exception
    {
        public ServiceLookupException() : base() { }

        public ServiceLookupException(string message) : base(message) { }
    }
}
