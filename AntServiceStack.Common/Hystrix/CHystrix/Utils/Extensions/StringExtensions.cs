namespace CHystrix.Utils.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;

    internal static class StringExtensions
    {
        private static object DeserializeFromString(this string json, Type type)
        {
            object obj2;
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Position = 0L;
                    obj2 = new DataContractJsonSerializer(type).ReadObject(stream);
                }
            }
            catch (Exception exception)
            {
                throw new SerializationException("JsonDeserializer: Error converting string to type: " + exception.Message, exception);
            }
            return obj2;
        }

        public static string EncodeJson(this string value)
        {
            return ("\"" + value.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", @"\n") + "\"");
        }

        public static string EncodeXml(this string value)
        {
            return value.Replace("<", "&lt;").Replace(">", "&gt;").Replace("&", "&amp;");
        }

        public static T FromJson<T>(this string json)
        {
            return (T) json.DeserializeFromString(typeof(T));
        }

        public static string ToJson(this object obj)
        {
            if (obj == null)
            {
                return null;
            }
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            string str = null;
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, obj);
                stream.Seek(0L, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(stream))
                {
                    str = reader.ReadToEnd();
                }
            }
            return str;
        }

        public static string UrlDecode(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            List<byte> list = new List<byte>();
            int length = text.Length;
            for (int i = 0; i < length; i++)
            {
                char ch = text[i];
                switch (ch)
                {
                    case '+':
                        list.Add(0x20);
                        break;

                    case '%':
                    {
                        byte item = Convert.ToByte(text.Substring(i + 1, 2), 0x10);
                        list.Add(item);
                        i += 2;
                        break;
                    }
                    default:
                        list.Add((byte) ch);
                        break;
                }
            }
            return Encoding.UTF8.GetString(list.ToArray());
        }

        public static string UrlEncode(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            StringBuilder builder = new StringBuilder();
            foreach (byte num in Encoding.UTF8.GetBytes(text))
            {
                if ((((num >= 0x41) && (num <= 90)) || ((num >= 0x61) && (num <= 0x7a))) || (((num >= 0x30) && (num <= 0x39)) || ((num >= 0x2c) && (num <= 0x2e))))
                {
                    builder.Append((char) num);
                }
                else
                {
                    builder.Append('%' + num.ToString("x2"));
                }
            }
            return builder.ToString();
        }

        public static string WithTrailingSlash(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            if (path[path.Length - 1] != '/')
            {
                return (path + "/");
            }
            return path;
        }
    }
}

