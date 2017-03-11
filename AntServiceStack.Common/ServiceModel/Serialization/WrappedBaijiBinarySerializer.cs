using System;
using System.IO;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.Baiji;

namespace AntServiceStack.ServiceModel.Serialization
{
    internal static class WrappedBaijiBinarySerializer
    {
        private static readonly BinarySerializer binarySerializer = new BinarySerializer();

        public static void Serialize(IRequestContext requestContext, object dto, Stream outputStream)
        {
            binarySerializer.Serialize(dto, outputStream);
        }

        public static object Deserialize(Type type, Stream fromStream)
        {
            return binarySerializer.Deserialize(type, fromStream);
        }

        public static void Serialize(object dto, Stream outputStream)
        {
            binarySerializer.Serialize(dto, outputStream);
        }
    }
}
