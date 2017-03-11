
#if !XBOX360 && !SILVERLIGHT && !WINDOWS_PHONE && !MONOTOUCH
using System.IO.Compression;
#endif

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Text;
using System.Xml;

namespace AntServiceStack.Text
{
#if !XBOX
    public class WrappedXmlSerializer
    {
        private static readonly Encoding Encoding = Encoding.UTF8;
        private readonly XmlDictionaryReaderQuotas quotas;

        public static bool UseDataContract {get; set;}

        public static bool CheckCharacters { get; set; }

        public static WrappedXmlSerializer Instance
            = new WrappedXmlSerializer(
#if !SILVERLIGHT && !WINDOWS_PHONE && !MONOTOUCH
                new XmlDictionaryReaderQuotas { MaxStringContentLength = 1024 * 1024, }
#endif
);

        public WrappedXmlSerializer(XmlDictionaryReaderQuotas quotas = null, bool omitXmlDeclaration = false)
        {
            this.quotas = quotas;
        }

        private static object Deserialize(string xml, Type type, XmlDictionaryReaderQuotas quotas)
        {
            try
            {
#if WINDOWS_PHONE
                StringReader stringReader = new StringReader(xml);
                using (var reader = XmlDictionaryReader.Create(stringReader))
                {
                    var serializer = new DataContractSerializer(type);
                    return serializer.ReadObject(reader);
                }
#else
                var bytes = Encoding.GetBytes(xml);
                using (var reader = XmlDictionaryReader.CreateTextReader(bytes, quotas))
                {
                    if (UseDataContract) // use DataContractSerializer
                    {
                        var serializer = new DataContractSerializer(type);
                        return serializer.ReadObject(reader);
                    }
                    else // use class XmlSerializer
                    {
                        var serializer = new XmlSerializer(type);
                        return serializer.Deserialize(reader);
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                throw new SerializationException("DeserializeXml: Error converting type: " + ex.Message, ex);
            }
        }

        public static object DeserializeFromString(string xml, Type type)
        {
            return Deserialize(xml, type, Instance.quotas);
        }

        public static T DeserializeFromString<T>(string xml)
        {
            var type = typeof(T);
            return (T)Deserialize(xml, type, Instance.quotas);
        }

        public static T DeserializeFromReader<T>(TextReader reader)
        {
            return DeserializeFromString<T>(reader.ReadToEnd());
        }

        public static T DeserializeFromStream<T>(Stream stream)
        {
            return (T)DeserializeFromStream(typeof(T), stream);
        }

        public static object DeserializeFromStream(Type type, Stream stream)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                CheckCharacters = CheckCharacters
            };
            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                if (UseDataContract)
                {
                    var serializer = new DataContractSerializer(type);
                    return serializer.ReadObject(reader);
                }
                else
                {
                    var serializer = new XmlSerializer(type);
                    return serializer.Deserialize(reader);
                }
            }
        }

        public static string SerializeToString<T>(T from, bool indentXml)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    XmlWriterSettings settings = new XmlWriterSettings()
                    {
                        Encoding = Encoding,
                        CheckCharacters = CheckCharacters,
                        Indent = indentXml
                    };
                    using (var xw = XmlWriter.Create(ms, settings))
                    {
                        if (UseDataContract) // use DataContractSerializer
                        {
                            var serializer = new DataContractSerializer(from.GetType());
                            serializer.WriteObject(xw, from);
                        }
                        else // use Classic XmlSerializer
                        {
                            var serializer = new XmlSerializer(from.GetType());
                            serializer.Serialize(xw, from);
                        }
                        xw.Flush();
                        ms.Seek(0, SeekOrigin.Begin);
                        var reader = new StreamReader(ms);
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Error serializing object of type {0}", from.GetType().FullName), ex);
            }
        }

        public static string SerializeToStringWithoutXmlDeclaration<T>(T from, bool indentXml)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    XmlWriterSettings settings = new XmlWriterSettings()
                    {
                        Encoding = Encoding,
                        CheckCharacters = CheckCharacters,
                        Indent = indentXml,
                        OmitXmlDeclaration = true
                    };
                    using (var xw = XmlWriter.Create(ms, settings))
                    {   
                        if (UseDataContract) // use DataContractSerializer
                        {
                            var serializer = new DataContractSerializer(from.GetType());
                            serializer.WriteObject(xw, from);
                        }
                        else // use Classic XmlSerializer
                        {
                            var serializer = new XmlSerializer(from.GetType());
                            serializer.Serialize(xw, from);
                        }
                        xw.Flush();
                        ms.Seek(0, SeekOrigin.Begin);
                        var reader = new StreamReader(ms);
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Error serializing object of type {0}", from.GetType().FullName), ex);
            }
        }

        public static void SerializeToWriter<T>(T value, TextWriter writer)
        {
            try
            {
#if !SILVERLIGHT
				using (var xw = new XmlTextWriter(writer))
#else
                using (var xw = XmlWriter.Create(writer))
#endif
                {
                    if (UseDataContract)
                    {
                        var serializer = new DataContractSerializer(value.GetType());
                        serializer.WriteObject(xw, value);
                    }
                    else
                    {
                        var serializer = new XmlSerializer(value.GetType());
                        serializer.Serialize(xw, value);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Error serializing object of type {0}", value.GetType().FullName), ex);
            }
        }

        public static void SerializeToStreamWithoutXmlDeclaration(object obj, Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = true,
                Encoding = Encoding,
                CheckCharacters = CheckCharacters,
            };
            using (var xw = XmlWriter.Create(stream, settings))
            {
                if (UseDataContract)
                {
                    var serializer = new DataContractSerializer(obj.GetType());
                    serializer.WriteObject(xw, obj);
                }
                else
                {
                    var serializer = new XmlSerializer(obj.GetType());
                    serializer.Serialize(xw, obj);
                }
            }
        }


        public static void SerializeToStream(object obj, Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Encoding = Encoding,
                CheckCharacters = CheckCharacters 
            };
#if !SILVERLIGHT
            using (var xw = XmlWriter.Create(stream, settings))
#else
            using (var xw = XmlWriter.Create(stream))
#endif
            {
                if (UseDataContract)
                {
                    var serializer = new DataContractSerializer(obj.GetType());
                    serializer.WriteObject(xw, obj);
                }
                else
                {
                    var serializer = new XmlSerializer(obj.GetType());
                    serializer.Serialize(xw, obj);
                }
            }
        }


#if !SILVERLIGHT && !MONOTOUCH
        public static void CompressToStream<TXmlDto>(TXmlDto from, Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Encoding = Encoding,
                CheckCharacters = CheckCharacters 
            };
            using (var deflateStream = new DeflateStream(stream, CompressionMode.Compress))
            using (var xw = XmlWriter.Create(deflateStream, settings))
            {
                if (UseDataContract)
                {
                    var serializer = new DataContractSerializer(from.GetType());
                    serializer.WriteObject(xw, from);
                }
                else
                {
                    var serializer = new XmlSerializer(from.GetType());
                    serializer.Serialize(xw, from);
                }
                xw.Flush();
            }
        }

        public static byte[] Compress<TXmlDto>(TXmlDto from)
        {
            using (var ms = new MemoryStream())
            {
                CompressToStream(from, ms);

                return ms.ToArray();
            }
        }
#endif

    }
#endif
}
