using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntServiceStack.DbModel;
using AntServiceStack.DbModel.Mysql;
using AntServiceStack.Manager.Common;
using AntServiceStack.Manager.Model.Condition;
using CloudBag.Infrastructure.StaticExt;

namespace AntServiceStack.Manager.Model.Request
{
    public class ServiceVm : ConditionBase
    {
        public string ServiceName { get; set; }
    }
    public class ServiceNodeVm : ConditionBase
    {
        public string ServiceFullName { get; set; }
    }
    #region 所有的服务

    public class RemoteServices
    {
        public bool Success { get; set; }
        public List<Domain> Domains { get; set; }
    }

    public class Domain
    {
        public string Name { get; set; }
        public List<ServiceRemote> Services { get; set; }
    }

    public class ServiceRemote
    {
        public string ServiceName { get; set; }
        public string Namespace { get; set; }
    } 

    #endregion

    public class NodeSm:Node,IMapFrom<Node>
    {
        public string TypeStr {
            get
            {
                try
                {
                    var str = EnumUtil.IntToEnum<NodeTypeEnum>(this.Type).GetDescription();
                    return str;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }
    }
}