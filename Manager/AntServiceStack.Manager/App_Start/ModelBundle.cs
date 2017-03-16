using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntServiceStack.Manager.Model.JsonNet;
using System.Web.Mvc;
namespace AntServiceStack.Manager
{

    public class ModelBundle
    {
        public static void RegisterBindles(ModelBinderDictionary modelBinder)
        {
            //modelBinder.Add(typeof(AddRoleVm), new JsonNetModelBinder());
        }
    }
}