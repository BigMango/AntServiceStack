using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace AntServiceStack.Manager.SignalR
{
    public class AntSoaHub : Hub
    {
        private readonly IHubLogger _slabLogger;
        public AntSoaHub(IHubLogger slabLogger)
        {
            _slabLogger = slabLogger;
        }


        public override Task OnConnected()
        {
            _slabLogger.Info("OnConnected", Context.ConnectionId);
            return (base.OnConnected());
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _slabLogger.Info("OnDisconnected", Context.ConnectionId);
            return (base.OnDisconnected(stopCalled));
        }

        public override Task OnReconnected()
        {
            _slabLogger.Info("OnReconnected", Context.ConnectionId);
            return (base.OnReconnected());
        }
    }
}