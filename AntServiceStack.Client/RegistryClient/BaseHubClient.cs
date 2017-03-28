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

        private bool isConnected = false;
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


            //Closed will be called when reconnecting have failed (When IIS have been down for a longer period than accepted by Reconnect timeout).
            Action start = () =>
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        _hubConnection.Start().Wait();
                        if (isConnected)
                            _hubConnection_Reconnected();
                        else
                        {
                            isConnected = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _hubConnection_Error(ex);
                    }
                });
            };
            _hubConnection.Closed += start;
            _hubConnection.Headers.Add("token", GetToken());

        }

        public void CloseHub()
        {
            try
            {
                if (_hubConnection != null)
                {
                    _hubConnection.Stop();
                    _hubConnection.Dispose();
                }
            }
            catch (Exception)
            {
                //ignore
            }
        }

        protected void StartHubInternal()
        {
            try
            {
                _hubConnection.Start().Wait();
                isConnected = true;
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
