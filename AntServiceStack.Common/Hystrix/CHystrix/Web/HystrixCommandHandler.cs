namespace CHystrix.Web
{
    using CHystrix;
    using CHystrix.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    internal class HystrixCommandHandler : IHttpHandler
    {
        public const string OperationName = "_command";

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                List<CommandInfo> list = (from v in HystrixCommandBase.CommandComponentsCollection.Values select v.CommandInfo).ToList<CommandInfo>();
                context.Response.ContentType = "application/json";
                context.Response.Write(list.ToJson());
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

