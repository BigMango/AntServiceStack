using System;
using System.IO;
using AntServiceStack.ProtoBuf;
using AntServiceStack.ProtoBuf.Meta;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;

namespace AntServiceStack.Plugins.ProtoBuf
{
    public class ProtoBufFormat : IPlugin, IProtoBufPlugin
    {
        public void Register(IAppHost appHost)
        {
            appHost.ContentTypeFilters.Register(ContentType.ProtoBuf, Serialize, Deserialize);
            RuntimeTypeModel.Default.InferTagFromNameDefault = true;
        }

        /*
        private static RuntimeTypeModel model;

        public static RuntimeTypeModel Model
        {
            get { return model ?? (model = TypeModel.Create(); model.InferTagFromNameDefault= true); }
        }
        */

        public static void Serialize(IRequestContext requestContext, object dto, Stream outputStream)
        {
            //Model.Serialize(outputStream, dto);
            Serializer.NonGeneric.Serialize(outputStream, dto);
        }

        public static object Deserialize(Type type, Stream fromStream)
        {
            //var obj = Model.Deserialize(fromStream, null, type);
            //return obj;
            return Serializer.NonGeneric.Deserialize(type, fromStream);
        }
    }
}
