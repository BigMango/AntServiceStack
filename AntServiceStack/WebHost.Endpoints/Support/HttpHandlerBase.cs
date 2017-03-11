using System;
using System.Web;
using Freeway.Logging;
using System.Collections.Generic;

namespace AntServiceStack.WebHost.Endpoints.Support
{
    public abstract class HttpHandlerBase : IHttpHandler
    {
        private readonly ILog log;

        protected HttpHandlerBase()
        {
            this.log = LogManager.GetLogger(this.GetType());
        }

        public void ProcessRequest(HttpContext context)
        {
            var before = DateTime.UtcNow;
            Execute(context);
            var elapsed = DateTime.UtcNow - before;
            log.Debug(string.Format("'{0}' was completed in {1}ms", this.GetType().Name, elapsed.TotalMilliseconds), 
                new Dictionary<string, string>() 
                {
                    {"ErrorCode", "FXD300064"}
                });
        }

        public abstract void Execute(HttpContext context);

        public bool IsReusable
        {
            get { return false; }
        }
    }
}