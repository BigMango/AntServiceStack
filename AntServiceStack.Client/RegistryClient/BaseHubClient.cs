using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Freeway.Logging;
using Microsoft.AspNet.SignalR.Client;

namespace AntServiceStack.Client.RegistryClient
{
    internal abstract class BaseHubClient
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BaseHubClient));
        protected HubConnection _hubConnection;
        protected IHubProxy _myHubProxy;

        public string HubConnectionUrl { get; set; }
        public string HubProxyName { get; set; }

        public ConnectionState State
        {
            get { return _hubConnection.State; }
        }

        protected void Init()
        {
            _hubConnection = new HubConnection(HubConnectionUrl);
            _myHubProxy = _hubConnection.CreateHubProxy(HubProxyName);

            _hubConnection.Received += _hubConnection_Received;
            _hubConnection.Reconnected += _hubConnection_Reconnected;
            _hubConnection.Reconnecting += _hubConnection_Reconnecting;
            _hubConnection.StateChanged += _hubConnection_StateChanged;
            _hubConnection.Error += _hubConnection_Error;
            _hubConnection.ConnectionSlow += _hubConnection_ConnectionSlow;
            _hubConnection.Closed += _hubConnection_Closed;
            _hubConnection.Headers.Add("token", GetToken());

        }

        public void CloseHub()
        {
            if (_hubConnection!=null)
            {
                _hubConnection.Stop();
                _hubConnection.Dispose();
            }
        }

        protected void StartHubInternal()
        {
            try
            {
                _hubConnection.Start().Wait();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + " " + ex.StackTrace);
                throw new Exception("can not access SOA.ant.url:" + HubConnectionUrl);
            }

        }

        public abstract void StartHub();
        public abstract string GetToken();

        void _hubConnection_Closed()
        {
            log.Info("_hubConnection_Closed New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_ConnectionSlow()
        {
            log.Info("_hubConnection_ConnectionSlow New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_Error(Exception obj)
        {
            log.Info("_hubConnection_Error New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_StateChanged(StateChange obj)
        {
            log.Info("_hubConnection_StateChanged New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_Reconnecting()
        {
            log.Info("_hubConnection_Reconnecting New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_Reconnected()
        {
            log.Info("_hubConnection_Reconnected New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_Received(string obj)
        {
        }
    }
}
