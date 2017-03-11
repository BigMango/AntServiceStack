using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using System.Reflection;
using AntServiceStack.ProtoBuf;
using AntServiceStack.ProtoBuf.Meta;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.Text;
using BaijiJsonSerializer = AntServiceStack.Baiji.JsonSerializer;
using BaijiBinarySerializer = AntServiceStack.Baiji.BinarySerializer;

namespace AntServiceStack.Common.Utils
{
    public static class GeneralSerializer
    {
        internal static readonly BaijiJsonSerializer jsonSerializer = new BaijiJsonSerializer();
        internal static readonly BaijiBinarySerializer binarySerializer = new BaijiBinarySerializer();
        
        static GeneralSerializer()
        {
            RuntimeTypeModel.Default.InferTagFromNameDefault = true;
        }

        public static void Serialize(object data, Stream stream, DataFormat format)
        {
            switch (format)
            {
                case DataFormat.JSON:
                    WrappedJsonSerializer.Instance.SerializeToStream(data, stream);
                    break;
                case DataFormat.XML:
                    WrappedXmlSerializer.SerializeToStream(data, stream);
                    break;
                case DataFormat.ProtoBuf:
                    Serializer.NonGeneric.Serialize(stream, data);
                    break;
                case DataFormat.BJJSON:
                    jsonSerializer.Serialize(data, stream);
                    break;
                case DataFormat.BJBIN:
                    binarySerializer.Serialize(data, stream);
                    break;
                default:
                    throw new NotSupportedException(format + " is not supported.");
            }
        }

        public static object Deserialize(Type type, Stream stream, DataFormat format)
        {
            object data = null;
            switch (format)
            {
                case DataFormat.JSON:
                    data = WrappedJsonDeserializer.Instance.DeserializeFromStream(type, stream);
                    break;
                case DataFormat.XML:
                    data = WrappedXmlSerializer.DeserializeFromStream(type, stream);
                    break;
                case DataFormat.ProtoBuf:
                    data = Serializer.NonGeneric.Deserialize(type, stream);
                    break;
                case DataFormat.BJJSON:
                    data = jsonSerializer.Deserialize(type, stream);
                    break;
                case DataFormat.BJBIN:
                    data = binarySerializer.Deserialize(type, stream);
                    break;
                default:
                    throw new NotSupportedException(format + " is not supported.");
            }

            return data;
        }

        public static T Deserialize<T>(Stream stream, DataFormat format)
        {
            return (T)Deserialize(typeof(T), stream, format);
        }

        public static string SerializeToQueryString(object data)
        {
            string queryString = string.Empty;
            if (data != null)
            {
                IDictionary<string, string> properties = data.ToStringDictionary();
                foreach (KeyValuePair<string, string> propertyInfo in properties)
                {
                    queryString += queryString == string.Empty ? "?" : "&";
                    queryString += string.Format("{0}={1}", propertyInfo.Key, HttpUtility.UrlEncode(propertyInfo.Value ?? string.Empty));
                }
            }

            return queryString;
        }

        public static object DeserializeFromQueryString(Type type, string queryString)
        {
            if (type == null)
                throw new ArgumentNullException("Type cannot be null.");

            if (string.IsNullOrWhiteSpace(queryString))
                return type.GetDefaultValue();

            queryString = queryString.TrimStart('?');

            NameValueCollection properties = new NameValueCollection();
            string[] pairs = queryString.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string pair in pairs)
            {
                string[] property = pair.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (property.Length != 2 || string.IsNullOrWhiteSpace(property[0]))
                    continue;
                properties.Add(property[0], property[1]);
            }
            return KeyValueDeserializer.Instance.Parse(properties, type);
        }

        public static T DeserializeFromQueryString<T>(string queryString)
        {
            return (T)DeserializeFromQueryString(typeof(T), queryString);
        }

        public static object DeserializeFromQueryString(Type type, NameValueCollection queryString)
        {
            if (type == null)
                throw new ArgumentNullException("Type cannot be null.");

            if (queryString == null)
                return type.GetDefaultValue();

            return KeyValueDeserializer.Instance.Parse(queryString, type);
        }
    }
}
