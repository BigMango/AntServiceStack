using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Utils
{
    public enum DataFormat
    {
        XML,
        JSON,
        ProtoBuf,
        BJJSON,
        BJBIN,
        NotSupported
    }

    public static class DataFormatExtension
    {
        public const string JsonContentType = "application/json";
        public const string XmlContentType = "application/xml";
        public const string ProtoBufContentType = "application/x-protobuf";
        public const string BjjsonContentType = "application/bjjson";
        public const string BjbinContentType = "application/bjbin";

        public const string JsonFormatName = "json";
        public const string XmlFormatName = "xml";
        public const string ProtoBufFormatName = "x-protobuf";
        public const string BjjsonFormatName = "bjjson";
        public const string BjbinFormatName = "bjbin";

        public static string ToContentType(this DataFormat format)
        {
            string contentType = null;
            switch(format)
            {
                case DataFormat.JSON:
                    contentType = JsonContentType;
                    break;
                case DataFormat.XML:
                    contentType = XmlContentType;
                    break;
                case DataFormat.ProtoBuf:
                    contentType = ProtoBufContentType;
                    break;
                case DataFormat.BJJSON:
                    contentType = BjjsonContentType;
                    break;
                case DataFormat.BJBIN:
                    contentType = BjbinContentType;
                    break;
            }
            return contentType;
        }

        public static DataFormat ToDataFormat(this string format)
        {
            if (format != null)
                format = format.Trim().ToLower();

            DataFormat dataFormat;
            switch (format)
            {
                case JsonFormatName:
                case JsonContentType:
                    dataFormat = DataFormat.JSON;
                    break;
                case XmlFormatName:
                case XmlContentType:
                    dataFormat = DataFormat.XML;
                    break;
                case ProtoBufFormatName:
                case ProtoBufContentType:
                    dataFormat = DataFormat.ProtoBuf;
                    break;
                case BjjsonFormatName:
                case BjjsonContentType:
                    dataFormat = DataFormat.BJJSON;
                    break;
                case BjbinFormatName:
                case BjbinContentType:
                    dataFormat = DataFormat.BJBIN;
                    break;
                default:
                    dataFormat = DataFormat.NotSupported;
                    break;
            }
            return dataFormat;
        }
    }
}
