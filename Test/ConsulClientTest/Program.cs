using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Consul;

namespace ConsulClientTest
{
    class Program
    {
        static void Main(string[] args)
        {

            LongpollingForServices();
        }

        static void LongpollingForServices()
        {
            Task.Run(async () =>
            {
                using (var client = new ConsulClient())
                {

                    var result = await client.Catalog.Services();

                    result = await client.Catalog.Services(new QueryOptions
                    {
                        WaitIndex = result.LastIndex
                    });

                    Console.WriteLine(result.Response.Keys);
                    Console.WriteLine("get new value");
                }

            }).Wait();
            Console.ReadLine();
        }

        static void LongPollingForKV()
        {
            Task.Run(async () =>
            {
                using (var client = new ConsulClient())
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(2000);
                        await client.KV.Put(new KVPair("test")
                        {
                            Value = Encoding.UTF8.GetBytes(DateTime.Now.ToString(CultureInfo.InvariantCulture))
                        });
                        Console.WriteLine("set ok");
                    });

                    var result = await client.KV.Get("test");
                    Console.WriteLine(result.Response == null ? "null" : Encoding.UTF8.GetString(result.Response.Value));

                    result = await client.KV.Get("test", new QueryOptions
                    {
                        WaitIndex = result.LastIndex
                    });
                    Console.WriteLine("get new value");
                    Console.WriteLine(result.Response == null ? "null" : Encoding.UTF8.GetString(result.Response.Value));
                }

            }).Wait();
            Console.ReadLine();
        }
    }
}
