using System;
using System.IO;
using AntServiceStack.Common.ServiceClient;
using ContentTypes = AntServiceStack.Common.Web.ContentType;
using AntServiceStack.ServiceModel.Serialization;

namespace AntServiceStack.ServiceClient
{
    public class BaijiJsonClientCallFormat : IClientCallFormat
    {
        internal static readonly string ContentFormat;

        static BaijiJsonClientCallFormat()
        {
            ContentFormat = ContentTypes.ToContentFormat(ContentTypes.BaijiJson);
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
                return ContentTypes.BaijiJson; 
            }
        }

        public ClientStreamDeserializerDelegate StreamDeserializer
        {
            get 
            {
                return WrappedBaijiJsonSerializer.Deserialize;
            }
        }

        public ClientStreamSerializerDelegate StreamSerializer
        {
            get
            {
                return WrappedBaijiJsonSerializer.Serialize;
            }
        }
    }
}
