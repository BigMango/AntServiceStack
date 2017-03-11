namespace CHystrix.Web
{
    using CHystrix.Utils.Extensions;
    using System;
    using System.Web;

    internal class HystrixMetricsHandler : IHttpHandler
    {
        public const string OperationName = "_metrics";

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                MetricsInfo info = new MetricsInfo {
                    CommandInfoList = HystrixStreamHandler.GetHystrixCommandInfoList(),
                    ThreadPoolInfoList = HystrixStreamHandler.GetHystrixThreadPoolList()
                };
                info.CommandCount = info.CommandInfoList.Count;
                info.ThreadPoolCount = info.ThreadPoolInfoList.Count;
                context.Response.ContentType = "application/json";
                context.Response.Write(info.ToJson());
            }
            catch (Exception exception)
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write(exception.Message);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}

