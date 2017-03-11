using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using AntServiceStack.Text;

namespace AntServiceStack.ServiceModel.Serialization
{
    public class WrappedJsonDeserializer
    {
        public static WrappedJsonDeserializer Instance = new WrappedJsonDeserializer();

        public bool UseBcl { get; set; }

        public object DeserializeFromString(string json, Type returnType)
        {

            if (!UseBcl)
                return JsonSerializer.DeserializeFromString(json, returnType);

            try
            {
                using (var ms = new MemoryStream())
                {
                    var bytes = Encoding.UTF8.GetBytes(json);
                    ms.Write(bytes, 0, bytes.Length);
                    ms.Position = 0;
                    var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(returnType);
                    return serializer.ReadObject(ms);
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException("JsonDeserializer: Error converting to type: " + ex.Message, ex);
            }
        }

        public T DeserializeFromString<T>(string json)
        {
            if (UseBcl)
                return (T)DeserializeFromString(json, typeof(T));

            return JsonSerializer.DeserializeFromString<T>(json);
        }

        public T DeserializeFromStream<T>(Stream stream)
        {
            if (UseBcl)
            {
                var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(stream);
            }
            return JsonSerializer.DeserializeFromStream<T>(stream);
        }

        public object DeserializeFromStream(Type type, Stream stream)
        {

#if !SILVERLIGHT && !MONOTOUCH && !XBOX && !ANDROIDINDIE
            if (UseBcl)
            {
                var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(type);
                return serializer.ReadObject(stream);
            }
#endif

            return JsonSerializer.DeserializeFromStream(type, stream);
        }
    }
}
