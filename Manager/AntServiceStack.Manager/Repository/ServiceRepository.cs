using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AntData.ORM;
using AntData.ORM.Common;
using AntServiceStack.Common.Consul;
using AntServiceStack.Common.Utils;
using AntServiceStack.DbModel;
using AntServiceStack.DbModel.Mysql;
using AntServiceStack.Manager.Common;
using AntServiceStack.Manager.Model.Request;
using AutoMapper.QueryableExtensions;
using Node = AntServiceStack.DbModel.Node;

namespace AntServiceStack.Manager.Repository
{
    public class ServiceRepository: BaseRepository<Service>
    {
        /// <summary>
        /// 获取所有的服务
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Tuple<long, List<Service>>> GetServiveListAsync(ServiceVm model)
        {
            if (model == null)
            {
                return new Tuple<long, List<Service>>(0,new List<Service>());
            }

            var totalQuery = this.Entity;

            if (!string.IsNullOrEmpty(model.ServiceName))
            {
                totalQuery = totalQuery.Where(r => r.ServiceName.Equals(model.ServiceName));
            }

            var total = totalQuery.CountAsync();

            var query = this.Entity;
            if (!string.IsNullOrEmpty(model.ServiceName))
            {
                query = query.Where(r => r.ServiceName.Equals(model.ServiceName));
            }

            var list = await query.DynamicOrderBy(string.IsNullOrEmpty(model.OrderBy) ? "DataChangeLastTime" : model.OrderBy,
                           model.OrderSequence)
                           .Skip((model.PageIndex - 1) * model.PageSize)
                           .Take(model.PageSize)
                           .ToListAsync();
            return new Tuple<long, List<Service>>(await total, list);
        }

        /// <summary>
        /// 添加or修改服务
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<string> ModifyServiceAsync(Service model)
        {
            if (model == null || string.IsNullOrEmpty(model.ServiceName))
            {
                return Tip.BadRequest;
            }

            //服务名称 + 命名空间 是uniq的
            if (model.Tid > 0)
            {
                var entity = await this.Entity.FirstOrDefaultAsync(r => (r.ServiceName.Equals(model.ServiceName) && r.Namespace.Equals(model.Namespace)) && !r.Tid.Equals(model.Tid));
                if (entity != null)
                {
                    return Tip.IsExist;
                }
                model.DataChangeLastTime = DateTime.Now;
                //修改
                var update = this.DB.Update(model) > 0;
                if (!update)
                {
                    return Tip.UpdateError;
                }
            }
            else
            {
                var entity = await this.Entity.FirstOrDefaultAsync(r => (r.ServiceName.Equals(model.ServiceName) && r.Namespace.Equals(model.Namespace)));
                if (entity != null)
                {
                    return Tip.IsExist;
                }
                model.FullName = ServiceUtils.RefineServiceName(model.Namespace, model.ServiceName);
                var result = this.DB.Insert(model);
                if (result < 1)
                {
                    return Tip.InsertError;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 删除服务
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public async Task<string> DelServiceAsync(long tid)
        {
            var service = await this.Entity.FindByBkAsync(tid);
            if (service == null)
            {
                return Tip.NotFound;
            }
            //如果有节点则不能删除
            var haveNode = this.Entitys.Nodes.Any(r => r.IsActive && r.ServiceFullName.Equals(service.FullName));
            if (haveNode)
            {
                return Tip.HaveActiveNode;
            }
            try
            {

                this.DB.UseTransaction(con =>
                {
                    con.Tables.Nodes.Where(r => r.ServiceFullName.Equals(service.FullName)).Delete();
                    con.Tables.Services.Where(r => r.Tid.Equals(tid)).Delete();
                    return true;
                });

            }
            catch (Exception)
            {
                return Tip.DeleteError ;
            }
            return string.Empty;
        }

        /// <summary>
        /// 远程获取所有的服务
        /// </summary>
        /// <returns></returns>
        public async Task<RemoteServices> GetAllRemoteServices()
        {
            var result = new RemoteServices()
            {
                Success = true
            };

            var list = await this.Entity.Where(r => r.IsActive).ToListAsync();
            result.Domains = list.GroupBy(r => r.Domain, y => y).Select(r => new Domain()
            {
                Name = r.Key,
                Services = AutoMapperUtil.MapperToList<Service, ServiceRemote>(r.ToList())
            }).ToList();
            return result;
        }

        public async Task<List<Service>> GetLocalRemoteServices()
        {
            var list = await this.Entity.Where(r => r.IsActive).ToListAsync();
            return list;
        }

        /// <summary>
        /// 获取服务节点
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Tuple<long, List<NodeSm>>> GetServiveNodeListAsync(ServiceNodeVm model)
        {
            if (model == null)
            {
                return new Tuple<long, List<NodeSm>>(0, new List<NodeSm>());
            }

            var totalQuery = this.Entitys.Nodes;

            if (!string.IsNullOrEmpty(model.ServiceFullName))
            {
                totalQuery = totalQuery.Where(r => r.ServiceFullName.Equals(model.ServiceFullName));
            }

            var total = totalQuery.CountAsync();

            var query = this.Entitys.Nodes;
            if (!string.IsNullOrEmpty(model.ServiceFullName))
            {
                query = query.Where(r => r.ServiceFullName.Equals(model.ServiceFullName));
            }

            var list = await query.DynamicOrderBy(string.IsNullOrEmpty(model.OrderBy) ? "DataChangeLastTime" : model.OrderBy,
                           model.OrderSequence)
                           .Skip((model.PageIndex - 1) * model.PageSize)
                           .Take(model.PageSize)
                           .MappperTo<NodeSm>()
                           .ToListAsync();
            return new Tuple<long, List<NodeSm>>(await total, list);
        }

        public List<ConsulServiceResponse> GetServerNodeList(string name)
        {
            var result = new List<ConsulServiceResponse>();
            var list = this.Entitys.Nodes.Where(r => r.ServiceFullName.Equals(name) && r.IsActive).ToList();
            foreach (var server in list)
            {
                ConsulServiceResponse c = new ConsulServiceResponse
                {
                    ServiceAddress = server.Url,
                    ServiceID = name
                };

                result.Add(c);
            }

            return result;
        }

        /// <summary>
        /// 添加或修改节点
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<string> ModifyServiceNodeAsync(Node model)
        {
            if (model == null || string.IsNullOrEmpty(model.ServiceFullName))
            {
                return Tip.BadRequest;
            }

            if (model.Tid > 0)
            {
                var entity = await this.Entitys.Nodes.FirstOrDefaultAsync(r => r.Url.Equals(model.Url) && !r.Tid.Equals(model.Tid));
                if (entity != null)
                {
                    return Tip.IsExist;
                }
                model.DataChangeLastTime = DateTime.Now;
                //修改
                var update = this.DB.Update(model) > 0;
                if (!update)
                {
                    return Tip.UpdateError;
                }
            }
            else
            {
                var entity = await this.Entitys.Nodes.FirstOrDefaultAsync(r => r.Url.Equals(model.Url));
                if (entity != null)
                {
                    return Tip.IsExist;
                }
                model.Type = (int) NodeTypeEnum.SelfRegister;
                var result = this.DB.Insert(model);
                if (result < 1)
                {
                    return Tip.InsertError;
                }
            }
            SignalRUtil.PushServerToGroup(model.ServiceFullName, GetServerNodeList(model.ServiceFullName));
            return string.Empty;
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public async Task<string> DelServiceNodeAsync(long tid)
        {
            var node = await this.Entitys.Nodes.FindByBkAsync(tid);
            var result =  this.DB.Delete(node) > 0;
            if (result )
            {
                if (node.Type.Equals((int) NodeTypeEnum.Consul))
                {
                    //N:innovationwork.cloudbag.v1.cloudbagrestfulapi|A:[http://192.168.1.2:8088/]
                    //注销服务
                    var service = ConsulClient.GetService(node.ServiceFullName,
                        "N:{0}|A:[{1}]".Args(node.ServiceFullName, node.Url));
                    if (service != null)
                    {
                        ConsulClient.UnregisterService(service.ServiceID);
                    }
                }
                else
                {
                    SignalRUtil.PushServerToGroup(node.ServiceFullName, GetServerNodeList(node.ServiceFullName));
                }
                return string.Empty;
            }
            return Tip.DeleteError;
        }
    }
}