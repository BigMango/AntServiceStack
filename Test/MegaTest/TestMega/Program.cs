using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestContract.Client;

namespace TestMega
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = CloudBagRestFulApiClient.GetInstance("http://localhost/MegaApplication");
            var re = client.HelloWorld(new HelloWorldRequestType());
            re = client.StartIOCPTaskOfHelloWorld(new HelloWorldRequestType()).Result;
            //re = client.CreateAsyncTaskOfHelloWorld(new HelloWorldRequestType()).Result;
            Console.WriteLine(re.Response);
        }
    }
}
