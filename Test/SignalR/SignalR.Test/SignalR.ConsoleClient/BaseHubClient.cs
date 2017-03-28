using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace SignalR.ConsoleClient
{
    public abstract class BaseHubClient
    {
        protected HubConnection _hubConnection;
        protected IHubProxy _myHubProxy;

        public string Token { get; set; }
        public string HubConnectionUrl { get; set; }
        public string HubProxyName { get; set; }
        public TraceLevels HubTraceLevel { get; set; }
        public System.IO.TextWriter HubTraceWriter { get; set; }

        public ConnectionState State
        {
            get { return _hubConnection.State; }
        }

        protected void Init()
        {
            _hubConnection = new HubConnection(HubConnectionUrl)
            {
                TraceLevel = HubTraceLevel,
                TraceWriter = HubTraceWriter
            };
            _myHubProxy = _hubConnection.CreateHubProxy(HubProxyName);

            _hubConnection.Received += _hubConnection_Received;
            _hubConnection.Reconnected += _hubConnection_Reconnected;
            _hubConnection.Reconnecting += _hubConnection_Reconnecting;
            _hubConnection.StateChanged += _hubConnection_StateChanged;
            _hubConnection.Error += _hubConnection_Error;
            _hubConnection.ConnectionSlow += _hubConnection_ConnectionSlow;
            _hubConnection.Closed += _hubConnection_Closed;
            _hubConnection.Headers.Add("token",Token);

        }

        public void CloseHub()
        {
            _hubConnection.Stop();
            _hubConnection.Dispose();
        }

        protected void StartHubInternal()
        {
            try
            {
                _hubConnection.Start().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
            }

        }

        public abstract void StartHub();

        void _hubConnection_Closed()
        {
            Console.WriteLine(
                "_hubConnection_Closed New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_ConnectionSlow()
        {
            Console.WriteLine(
              "_hubConnection_ConnectionSlow New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_Error(Exception obj)
        {
            Console.WriteLine(
             "_hubConnection_Error New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_StateChanged(StateChange obj)
        {
            Console.WriteLine(
           "_hubConnection_StateChanged New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_Reconnecting()
        {
            Console.WriteLine(
           "_hubConnection_Reconnecting New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_Reconnected()
        {
            Console.WriteLine(
           "_hubConnection_Reconnected New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_Received(string obj)
        {
            //SlabClientLogger.Log(HubClientType.HubClientInformational,
            //"_hubConnection_Received New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }
    }
}
