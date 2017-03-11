using System;
using System.IO;
using AntServiceStack.Common.ServiceClient;
using AntServiceStack.Text;

namespace AntServiceStack.ServiceClient
{
    public class XmlClientCallFormat : IClientCallFormat
    {
        internal const string ContentFormat = "xml";

        public string Format
        {
            get { return ContentFormat; }
        }

        public string ContentType
        {
            get { return String.Format("application/{0}", Format); }
        }

        public ClientStreamDeserializerDelegate StreamDeserializer
        {
            get { return WrappedXmlSerializer.DeserializeFromStream; }
        }

        public ClientStreamSerializerDelegate StreamSerializer
        {
            get { return WrappedXmlSerializer.SerializeToStream; }
        }
    }
}
