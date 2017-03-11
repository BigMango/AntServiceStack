using System;
using System.IO;
using AntServiceStack.Common.ServiceClient;
using AntServiceStack.Text;

namespace AntServiceStack.ServiceClient
{
    public class JsvClientCallFormat : IClientCallFormat
    {

        public string Format
        {
            get { return "jsv"; }
        }

        public string ContentType
        {
            get { return String.Format("application/{0}", Format); }
        }

        public ClientStreamDeserializerDelegate StreamDeserializer
        {
            get { return TypeSerializer.DeserializeFromStream; }
        }

        public static void SerializeToStream(object obj, Stream stream)
        {
            TypeSerializer.SerializeToStream(obj, obj.GetType(), stream);
        }

        public ClientStreamSerializerDelegate StreamSerializer
        {
            get { return SerializeToStream; }
        }
    }
}
