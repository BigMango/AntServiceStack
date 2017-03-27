using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Consul;

namespace AntServiceStack.Manager.Common
{
    public class ConsulUtil
    {

        /// <summary>
        /// 监控所有的services
        /// 如果有服务上下线就能检测的到
        /// </summary>
        public static void StartWatchConsulServices()
        {
            var url = ConfigUtil.GetConfig<string>("SOA.consul.url");
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("SOA.consul.url");
            }

            Task.Run(async () =>
            {
                using (var client = new ConsulClient(configuration => configuration.Address = new Uri(url)))
                {
                    while (true)
                    {
                        var result = await client.Catalog.Services();
                        try
                        {
                            //默认是5分钟 一次 long polling
                            result = await client.Catalog.Services(new QueryOptions
                            {
                                WaitIndex = result.LastIndex
                            });


                        }
                        catch (Exception)
                        {
                            //ignore
                        }
                    }
                }

            }).Wait();
        }
    }
}