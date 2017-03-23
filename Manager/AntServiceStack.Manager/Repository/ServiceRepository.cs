using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AntData.ORM;
using AntServiceStack.DbModel;
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


            if (model.Tid > 0)
            {
                var district = await this.Entity.FirstOrDefaultAsync(r => r.ServiceName.Equals(model.ServiceName) && !r.Tid.Equals(model.Tid));
                if (district != null)
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
                var district = await this.Entity.FirstOrDefaultAsync(r => r.ServiceName.Equals(model.ServiceName));
                if (district != null)
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
        /// 删除服务
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public async Task<string> DelServiceAsync(long tid)
        {
            var result = await this.Entity.Where(r => r.Tid.Equals(tid)).DeleteAsync() > 0;
            return !result ? Tip.DeleteError : string.Empty;
        }
    }
}