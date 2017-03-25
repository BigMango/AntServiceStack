using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;

namespace AntServiceStack.Manager.SignalR
{
    public class LoggingPipelineModule : HubPipelineModule
    {
        private readonly IHubLogger _slabLogger;
        public LoggingPipelineModule(IHubLogger logger)
        {
            _slabLogger = logger;
        }

        protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
        {
            _slabLogger.Info("LoggingPipelineModule.OnBeforeIncoming", "=> Invoking " + context.MethodDescriptor.Name + " on hub " + context.MethodDescriptor.Hub.Name);
            return base.OnBeforeIncoming(context);
        }
        protected override bool OnBeforeOutgoing(IHubOutgoingInvokerContext context)
        {
            _slabLogger.Info("LoggingPipelineModule.OnBeforeOutgoing", "<= Invoking " + context.Invocation.Method + " on client hub " + context.Invocation.Hub);
            return base.OnBeforeOutgoing(context);
        }
    }
}