using AntServiceStack.ProtoBuf;
using AntServiceStack.ProtoBuf.Meta;
using System;
using System.IO;
using AntServiceStack.Common.ServiceClient;

namespace AntServiceStack.Plugins.ProtoBuf
{
    public class ProtoBufClientCallFormat : IClientCallFormat
    {

        static ProtoBufClientCallFormat()
        {
            RuntimeTypeModel.Default.InferTagFromNameDefault = true;
        }

        public string Format
        {
            get { return "x-protobuf"; }
        }

        public string ContentType
        {
            get { return String.Format("application/{0}", Format); }
        }

        /*private static RuntimeTypeModel model;

        public static RuntimeTypeModel Model
        {
            get { return model ?? (model = TypeModel.Create()); }
        }
        */

        public ClientStreamDeserializerDelegate StreamDeserializer
        {
            get { return Serializer.NonGeneric.Deserialize; }
        }

        public ClientStreamSerializerDelegate StreamSerializer
        {
            get { return Serialize; }
        }

        public static void Serialize(object dto, Stream outputStream)
        {
            Serializer.NonGeneric.Serialize(outputStream, dto);
        }
    }
}
