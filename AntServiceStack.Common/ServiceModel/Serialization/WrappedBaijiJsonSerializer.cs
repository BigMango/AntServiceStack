using System;
using System.IO;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.Baiji;

namespace AntServiceStack.ServiceModel.Serialization
{
    internal static class WrappedBaijiJsonSerializer
    {
        private static readonly JsonSerializer jsonSerializer = new JsonSerializer();

        public static void Serialize(IRequestContext requestContext, object dto, Stream outputStream)
        {
            jsonSerializer.Serialize(dto, outputStream);
        }

        public static object Deserialize(Type type, Stream fromStream)
        {
            return jsonSerializer.Deserialize(type, fromStream);
        }

        public static void Serialize(object dto, Stream outputStream)
        {
            jsonSerializer.Serialize(dto, outputStream);
        }
    }
}
