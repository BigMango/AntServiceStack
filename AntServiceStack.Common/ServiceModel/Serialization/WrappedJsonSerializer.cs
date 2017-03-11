using System;
using System.IO;
using System.Runtime.Serialization;
using AntServiceStack.Text;

namespace AntServiceStack.ServiceModel.Serialization
{
    public class WrappedJsonSerializer
    {
        public static WrappedJsonSerializer Instance = new WrappedJsonSerializer();

        public bool UseBcl { get; set; }

        public string SerializeToString<T>(T obj)
        {
            if (!UseBcl)
                return JsonSerializer.SerializeToString(obj);

            if (obj == null) return null;
            var type = obj.GetType();
            try
            {
                using (var ms = new MemoryStream())
                {
                    var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(type);
                    serializer.WriteObject(ms, obj);
                    ms.Position = 0;
                    using (var sr = new StreamReader(ms))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException("JsonSerializer: Error converting type: " + ex.Message, ex);
            }
        }

        public void SerializeToStream<T>(T obj, Stream stream)
        {
            if (UseBcl)
            {
                var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
                serializer.WriteObject(stream, obj);
            }
            else
            {
                JsonSerializer.SerializeToStream(obj, stream);
            }
        }
    }
}
