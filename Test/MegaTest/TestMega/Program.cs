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
            var client = CloudBagRestFulApiClient.GetInstance("http://localhost/WebApplication");
            var re = client.HelloWorld(new HelloWorldRequestType());
            Console.WriteLine(re.Response);
        }
    }
}
