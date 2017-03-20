using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using AntServiceStack.Common.Consul;
using AntServiceStack.Manager.Model.Request;
using AntServiceStack.Manager.Model.Result;
using Configuration;

namespace AntServiceStack.Manager.Controller
{
    public class ConsulController : BaseController
    {

        /// <summary>
        /// 根据serviceName获取一个服务
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public JsonResult GetConsulServiceList([FromUri] ConsulVm model)
        {
            var result = new SearchResult<List<ComsulSm>>();
            var respositoryResult = ConsulClient.GetAllService();
            result.Status = ResultConfig.Ok;
            result.Info = ResultConfig.SuccessfulMessage;
            //result.Rows = respositoryResult.Item2;
            result.Total = respositoryResult.Length;
            return Json(result);
        }
    }
}