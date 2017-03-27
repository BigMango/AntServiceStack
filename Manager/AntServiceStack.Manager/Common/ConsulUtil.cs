using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AntData.ORM;
using AntData.ORM.Data;
using AntServiceStack.DbModel.Mysql;
using AntServiceStack.Manager.Repository;
using Consul;
using Node = AntServiceStack.DbModel.Node;
namespace AntServiceStack.Manager.Common
{
    public class ConsulUtil 
    {
        protected static ServiceRepository _serviceRepository = new ServiceRepository();
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
                    var result = await client.Catalog.Services();
                    if (result.LastIndex < 1)
                    {
                        throw new Exception("consul get services error");
                    }
                    SyncNodesToDB(result.Response);
                    while (true)
                    {
                        try
                        {
                            //默认是5分钟 一次 long polling
                            result = await client.Catalog.Services(new QueryOptions
                            {
                                WaitIndex = result.LastIndex
                            });
                            SyncNodesToDB(result.Response);
                        }
                        catch (Exception ex)
                        {
                            LogUtil.WriteErrorLog(ex);
                        }
                    }
                }

            });
        }

        /// <summary>
        /// consul 上的 service的变更实时推送给client
        /// </summary>
        /// <param name="result"></param>
        private static void SyncNodesToDB(Dictionary<string, string[]> result)
        {
            if (result == null || result.Count<1)
            {
                return;
            }
            
            var allConsulNodes = _serviceRepository.Entitys.Nodes.Where(r => r.Type.Equals((int) NodeTypeEnum.Consul)).ToList()
                .GroupBy(r=>r.ServiceFullName,y=>y).ToDictionary(r=>r.Key,y=>y.ToList());
            Func<string[], List<string>> ParseUrls = strings =>
            {
                return strings.Where(p => p.StartsWith("N:"))
                            .Select(y => y.Split(new string[] { "A:" }, StringSplitOptions.None)[1].Replace("[","").Replace("]",""))
                            .ToList();
            };
           
            foreach (var r in result)
            {
                List<Node> nodes;
                List<string> toAddList;
                var isChange = false;
                if (allConsulNodes.TryGetValue(r.Key, out nodes))
                {
                    var consulUrls = ParseUrls(r.Value);
                    var excludedConsulUrls = new HashSet<string>(consulUrls.Select(p => p));
                    var toDelList = nodes.Where(rr => !excludedConsulUrls.Contains(rr.Url)).ToList();
                    var dbNodeUrls = new HashSet<string>(nodes.Select(p => p.Url));
                    toAddList = consulUrls.Where(rr => !dbNodeUrls.Contains(rr)).ToList();
                    if (toDelList.Count > 0)
                    {
                        isChange = _serviceRepository.Entitys.Nodes.Where(
                            rr => toDelList.Select(tt => tt.Tid).ToList().Contains(rr.Tid)).Delete() > 0;
                        
                    }
                }
                else
                {
                    toAddList = ParseUrls(r.Value);
                }
                if (toAddList.Count > 0)
                {
                    List<Node> toAdd = new List<Node>();
                    foreach (var item in toAddList)
                    {
                        toAdd.Add(new Node
                        {
                            DataChangeLastTime = DateTime.Now,
                            IsActive = true,
                            ServiceFullName = r.Key,
                            Type = (int)NodeTypeEnum.Consul,
                            Url = item
                        });
                    }
                    isChange = _serviceRepository.DB.BulkCopy(toAdd).RowsCopied > 0;
                }

                if (isChange)
                {
                    SignalRUtil.PushServerToGroup(r.Key, _serviceRepository.GetServerNodeList(r.Key));
                }
            }

            var excludedUrls = new HashSet<string>(result.Select(p => p.Key));
            var toDelList2 = allConsulNodes.Keys.Where(rr => !excludedUrls.Contains(rr)).ToList();
            if (toDelList2.Count > 0)
            {
                _serviceRepository.Entitys.Nodes.Where(
                    rr => toDelList2.Contains(rr.ServiceFullName) && rr.Type.Equals((int)NodeTypeEnum.Consul)).Delete();
                foreach (var del in toDelList2)
                {
                    SignalRUtil.PushServerToGroup(del,_serviceRepository.GetServerNodeList(del));
                }
            }
        }

    }
}