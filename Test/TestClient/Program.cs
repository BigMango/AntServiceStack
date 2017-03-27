using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestContract.Client;

namespace TestClient
{
    class Program
    {
        static helloworldClient client = helloworldClient.GetInstance();
        static void Main(string[] args)
        {
            TestCreateTask();
            //TestConsul();
            //TestHystrix();

            Console.ReadLine();
        }

        static void TestCreateTask()
        {
            var hystrixIn = new HelloWorldRequestType();
            try
            {
                var hystrixOut = client.CreateAsyncTaskOfHelloWorld(hystrixIn).Result;
                Console.WriteLine(hystrixOut.Response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }

        static void TestConsul()
        {

            HelloWorldRequestType hystrixIn = new HelloWorldRequestType();
            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    var hystrixOut = client.StartIOCPTaskOfHelloWorld(hystrixIn).Result;
                    Console.WriteLine(hystrixOut.Response);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
        static void TestHystrix()
        {
            int threadcnt = 50;
            Thread[] threads = new Thread[threadcnt];
            Exception[] exceptions = new Exception[threadcnt];
            int excpCnt = 0;
            for (int i = 0; i < threads.Length; i++)
            {
                int tmp = i;
                threads[tmp] = new Thread(() => SendHystrixTestServiceRequest(tmp, exceptions));
                threads[tmp].IsBackground = true;
                threads[tmp].Start();
            }
            for (var i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }
            for (int i = 0; i < threadcnt; i++)
            {
                if (exceptions[i] == null)
                {
                    excpCnt++;
                }
            }

            Console.WriteLine(excpCnt);

            Thread.Sleep(10000);//休息10秒 因为熔断器开启后悔5秒后放行一个请求

            //var response = client.HelloWorld(request);
            //var response = client.StartIOCPTaskOfHelloWorld(request).Result;
            //Console.WriteLine(response.Response);
            Console.WriteLine("over");
            Console.ReadLine();

            HelloWorldRequestType hystrixIn;
            hystrixIn = new HelloWorldRequestType();
            try
            {
                var hystrixOut = client.StartIOCPTaskOfHelloWorld(hystrixIn).Result;
                Console.WriteLine(hystrixOut.Response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
        static void SendHystrixTestServiceRequest(int i, Exception[] exceptions)
        {
            HelloWorldRequestType hystrixIn;
            hystrixIn = new HelloWorldRequestType() ;
            try
            {
                var hystrixOut = client.StartIOCPTaskOfHelloWorld(hystrixIn).Result;
                Console.WriteLine("hystrix success " + i);
            }
            catch (Exception e)
            {
                exceptions[i] = e;
                Console.WriteLine(e.Message + e.InnerException.Message + i);
            }
        }
    }
}
