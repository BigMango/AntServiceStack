using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntServiceStack.Text;

namespace ConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //序列化日期
            JsConfig.UseStandardLongDateTime();
            var appHost = new Host();
            appHost.Init();
            Console.WriteLine("Service is running");
            Console.WriteLine("Started Time: {0}", DateTime.Now);
            Console.ReadKey();
            Console.WriteLine("Service is STop");
            appHost.Stop();
            appHost.Dispose();
        }
    }
}
