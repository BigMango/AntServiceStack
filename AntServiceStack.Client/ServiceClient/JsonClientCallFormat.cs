using System;
using AntServiceStack.Common.ServiceClient;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.Text;

namespace AntServiceStack.ServiceClient
{
    public class JsonClientCallFormat : IClientCallFormat
    {
        private static bool _useBcl;
        public static bool UseBclJsonSerializers
        {
            get
            {
                return _useBcl;
            }
            set
            {
                _useBcl = value;
                WrappedJsonSerializer.Instance.UseBcl = _useBcl;
                WrappedJsonDeserializer.Instance.UseBcl = _useBcl;
            }
        }

        public string Format
        {
            get { return "json"; }
        }

        public string ContentType
        {
            get { return String.Format("application/{0}", Format); }
        }

        public ClientStreamDeserializerDelegate StreamDeserializer
        {
            get { return WrappedJsonDeserializer.Instance.DeserializeFromStream; }
        }

        public ClientStreamSerializerDelegate StreamSerializer
        {
            get { return WrappedJsonSerializer.Instance.SerializeToStream; }
        }
    }
}
