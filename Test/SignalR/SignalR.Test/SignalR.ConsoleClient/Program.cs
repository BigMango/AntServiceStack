using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace SignalR.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
           

            string name = "AA";

            var myHubClient = new MyHubClient("test1");
            var myHubClient2 = new MyHubClient("test2");
            while (true)
            {
                string key = Console.ReadLine();
                
                
                if (key.ToUpper() == "H")
                {
                    if (myHubClient.State == ConnectionState.Connected)
                    {
                        myHubClient.Heartbeat();
                    }
                    else
                    {
                        Console.WriteLine("Can't send message, connectionState= " + myHubClient.State);
                    }

                }
               
            }
        }
    }
}
