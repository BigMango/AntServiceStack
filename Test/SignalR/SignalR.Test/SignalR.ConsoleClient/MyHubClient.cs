using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace SignalR.ConsoleClient
{
    public class MyHubClient : BaseHubClient
    {
        public MyHubClient(string token)
        {
            Token = token;
            Init();
        }

        public new void Init()
        {
            HubConnectionUrl = "http://localhost/AntServiceStack.Manager/antsoa";
            HubProxyName = "AntSoaHub";
            HubTraceLevel = TraceLevels.None;
            HubTraceWriter = Console.Out;

            base.Init();

            #region Recieve事件注册
            _myHubProxy.On<string, string>("SendMessage", Recieve_SendMessage);
            _myHubProxy.On("Heartbeat", Recieve_Heartbeat);
            #endregion

            StartHubInternal();
        }
        public override void StartHub()
        {
            _hubConnection.Dispose();
            Init();
        }


       
    


        public void Recieve_Heartbeat()
        {
            Console.WriteLine("Recieved heartbeat Success");
        }

        public void Recieve_SendMessage(string name, string message)
        {
            Console.WriteLine($"【{name}】:{message}");
        }

        

      

        public void Heartbeat()
        {
            _myHubProxy.Invoke("Heartbeat").ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    if (task.Exception != null)
                        Console.WriteLine("There was an error opening the connection:" + task.Exception.GetBaseException());
                }

            }).Wait();

           
        }

     
        
    }
}
