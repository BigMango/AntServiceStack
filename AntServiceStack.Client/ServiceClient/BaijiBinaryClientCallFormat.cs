using System;
using System.IO;
using AntServiceStack.Common.ServiceClient;
using ContentTypes = AntServiceStack.Common.Web.ContentType;
using AntServiceStack.ServiceModel.Serialization;

namespace AntServiceStack.ServiceClient
{
    public class BaijiBinaryClientCallFormat : IClientCallFormat
    {
        internal static readonly string ContentFormat;

        static BaijiBinaryClientCallFormat()
        {
            ContentFormat = ContentTypes.ToContentFormat(ContentTypes.BaijiBinary);
        }

        public string Format
        {
            get 
            { 
                return ContentFormat; 
            }
        }

        public string ContentType
        {
            get 
            {
                return ContentTypes.BaijiBinary; 
            }
        }

        public ClientStreamDeserializerDelegate StreamDeserializer
        {
            get 
            {
                return WrappedBaijiBinarySerializer.Deserialize;
            }
        }

        public ClientStreamSerializerDelegate StreamSerializer
        {
            get
            {
                return WrappedBaijiBinarySerializer.Serialize;
            }
        }
    }
}
