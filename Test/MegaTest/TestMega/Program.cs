using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;
using TestMegaContract.Client;

namespace TestMega
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = helloworldClient.GetInstance();
            var re = client.HelloWorld(new HelloWorldRequestType());
            re = client.StartIOCPTaskOfHelloWorld(new HelloWorldRequestType()).Result;
            re = client.CreateAsyncTaskOfHelloWorld(new HelloWorldRequestType()).Result;
            Console.WriteLine(re.Response);

        }
    }
}
