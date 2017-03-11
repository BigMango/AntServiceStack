using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Utils
{
    internal static class InternalServiceUtils
    {
        internal const string IOCPRequestSpanName = "SOA 2.0 IOCP Request";

        internal const string IOCPResponseSpanName = "SOA 2.0 IOCP Response";

        internal const string ServiceErrorTitle = "服务端处理请求时出错。";

        internal const string ValidationErrorTitle = "服务端验证请求时出错。";

        internal const string FrameworkErrorTitle = ServiceErrorTitle;

        internal const string SLAErrorTitle = "服务端发生了熔断，服务端恢复后此错误就会消失。";

        internal const string AsyncServiceStartTimeKey = "AsyncServiceStartTime";

        internal const string SOA2CurrentOperationKey = "SOA2_Current_Operation";
    }
}
