using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;

namespace AntServiceStack.Manager.SignalR
{
    public class ErrorHandlingPipelineModule : HubPipelineModule
    {
        private readonly IHubLogger _slabLogger;
        public ErrorHandlingPipelineModule(IHubLogger logger)
        {
            _slabLogger = logger;
        }

        protected override void OnIncomingError(ExceptionContext ex, IHubIncomingInvokerContext context)
        {
            _slabLogger.Error("ErrorHandlingPipelineModule.OnIncomingError", "=> Exception " + ex.Error + " " + ex.Result);
            if (ex.Error.InnerException != null)
            {
                _slabLogger.Error("ErrorHandlingPipelineModule.OnIncomingError", "=> Inner Exception " + ex.Error.InnerException.Message);
            }
            base.OnIncomingError(ex, context);
        }
    }
}