using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using AntServiceStack.DbModel;
using AntServiceStack.Manager.Model.Request;
using AntServiceStack.Manager.Model.Result;
using AntServiceStack.Manager.Repository;
using AntServiceStack.Manager.SignalR;
using Configuration;
using Microsoft.AspNet.SignalR;

namespace AntServiceStack.Manager.Controller
{
    public class ServiceController : BaseController
    {

        /// <summary>
        /// 远程获取所有的服务
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetAllRemoteServices()
        {
            //RemoteServices
            ServiceRepository rep = new ServiceRepository();
            var respositoryResult = await rep.GetAllRemoteServices();
            return Json(respositoryResult, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetLocalRemoteServices()
        {
            var result = new ResultJsonInfo<List<Service>>();
            //RemoteServices
            ServiceRepository rep = new ServiceRepository();
            var respositoryResult = await rep.GetLocalRemoteServices();
            result.Data = respositoryResult;
            result.Status = ResultConfig.Ok;
            result.Info = ResultConfig.SuccessfulMessage;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取活动列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetServiveList([FromUri] ServiceVm model)
        {
            var result = new SearchResult<List<Service>>();
            ServiceRepository rep = new ServiceRepository();
            var respositoryResult = await rep.GetServiveListAsync(model);
            result.Status = ResultConfig.Ok;
            result.Info = ResultConfig.SuccessfulMessage;
            result.Rows = respositoryResult.Item2;
            result.Total = respositoryResult.Item1;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 添加或修改活动
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> ModifyService([FromBody] Service model)
        {
            var result = new ResultJsonNoDataInfo();
            ServiceRepository rep = new ServiceRepository();
            var respositoryResult = await rep.ModifyServiceAsync(model);
            if (string.IsNullOrEmpty(respositoryResult))
            {
                result.Status = ResultConfig.Ok;
                result.Info = ResultConfig.SuccessfulMessage;
            }
            else
            {
                result.Status = ResultConfig.Fail;
                result.Info = string.IsNullOrEmpty(respositoryResult) ? ResultConfig.FailMessage : respositoryResult;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除活动
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> DelService([FromBody] long tid)
        {
            var result = new ResultJsonNoDataInfo();
            ServiceRepository rep = new ServiceRepository();
            var respositoryResult = await rep.DelServiceAsync(tid);
            if (string.IsNullOrEmpty(respositoryResult))
            {
                result.Status = ResultConfig.Ok;
                result.Info = ResultConfig.SuccessfulMessage;
            }
            else
            {
                result.Status = ResultConfig.Fail;
                result.Info = string.IsNullOrEmpty(respositoryResult) ? ResultConfig.FailMessage : respositoryResult;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 服务节点
        /// </summary>
        /// <returns></returns>
        public ActionResult Nodes()
        {
            return View();
        }

        /// <summary>
        /// 获取服务节点
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetServiveNodeList([FromUri] ServiceNodeVm model)
        {
            var result = new SearchResult<List<Node>>();
            ServiceRepository rep = new ServiceRepository();
            var respositoryResult = await rep.GetServiveNodeListAsync(model);
            result.Status = ResultConfig.Ok;
            result.Info = ResultConfig.SuccessfulMessage;
            result.Rows = respositoryResult.Item2;
            result.Total = respositoryResult.Item1;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 添加或修改活动
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> ModifyServiceNode([FromBody] Node model)
        {
            var result = new ResultJsonNoDataInfo();
            ServiceRepository rep = new ServiceRepository();
            var respositoryResult = await rep.ModifyServiceNodeAsync(model);
            if (string.IsNullOrEmpty(respositoryResult))
            {
                result.Status = ResultConfig.Ok;
                result.Info = ResultConfig.SuccessfulMessage;
            }
            else
            {
                result.Status = ResultConfig.Fail;
                result.Info = string.IsNullOrEmpty(respositoryResult) ? ResultConfig.FailMessage : respositoryResult;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除活动
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> DelServiceNode([FromBody] long tid)
        {
            var result = new ResultJsonNoDataInfo();
            ServiceRepository rep = new ServiceRepository();
            var respositoryResult = await rep.DelServiceNodeAsync(tid);
            if (string.IsNullOrEmpty(respositoryResult))
            {
                result.Status = ResultConfig.Ok;
                result.Info = ResultConfig.SuccessfulMessage;
            }
            else
            {
                result.Status = ResultConfig.Fail;
                result.Info = string.IsNullOrEmpty(respositoryResult) ? ResultConfig.FailMessage : respositoryResult;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}