using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AntData.ORM;
using AntServiceStack.Common.Utils;
using AntServiceStack.DbModel;
using AntServiceStack.Manager.Common;
using AntServiceStack.Manager.Model.Request;

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
        public async Task<Tuple<long, List<Node>>> GetServiveNodeListAsync(ServiceNodeVm model)
        {
            if (model == null)
            {
                return new Tuple<long, List<Node>>(0, new List<Node>());
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
                           .ToListAsync();
            return new Tuple<long, List<Node>>(await total, list);
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
                var result = this.DB.Insert(model);
                if (result < 1)
                {
                    return Tip.InsertError;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public async Task<string> DelServiceNodeAsync(long tid)
        {
            var result = await this.Entitys.Nodes.Where(r => r.Tid.Equals(tid)).DeleteAsync() > 0;
            return !result ? Tip.DeleteError : string.Empty;
        }
    }
}