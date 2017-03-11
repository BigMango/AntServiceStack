using AntServiceStack.Baiji.Specific;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AntServiceStack.Baiji
{
    public class JsonSerializer : ISerializer
    {
        #region [Private Fields]
        private static readonly IDictionary<Type, SpecificJsonStreamParser> _readerCache =
            new Dictionary<Type, SpecificJsonStreamParser>();

        /// <summary>
        /// UTF8 without bom
        /// </summary>
        private static readonly Encoding DEFAULT_ENCODING = new UTF8Encoding(false);

        public Encoding Encoding
        {
            get;
            set;
        }
        #endregion [Private Fields]

        public JsonSerializer()
        {
            Encoding = DEFAULT_ENCODING;
        }

        public void Serialize(object obj, Stream stream)
        {
            var record = obj as ISpecificRecord;
            if (record == null)
            {
                throw new ArgumentException("obj doesn't implement ISpecifiedRecord interface.");
            }
            var writer = new SpecificJsonWriter(record.GetSchema(), Encoding);
            writer.Write(record, stream);
        }

        public void Serialize<T>(T obj, Stream stream) where T : ISpecificRecord, new()
        {
            var writer = new SpecificJsonWriter(obj.GetSchema(), Encoding);
            writer.Write(obj, stream);
        }

        public object Deserialize(Type type, Stream stream)
        {
            if (!type.GetInterfaces().Any(i => i == typeof(ISpecificRecord)))
            {
                throw new ArgumentException("type doesn't implement ISpecifiedRecord interface.");
            }
            var parser = GetParser(type, Encoding);
            return parser.Parse(null, stream);
        }

        public T Deserialize<T>(Stream stream) where T : ISpecificRecord, new()
        {
            var parser = GetParser(typeof(T), Encoding);
            return parser.Parse(default(T), stream);
        }

        /// <summary>
        /// This method is designed for testing, and is not expected to be used in any other cases.
        /// </summary>
        public void ClearCache()
        {
            lock (_readerCache)
            {
                _readerCache.Clear();
            }
        }

        #region [Private Methods]
        private static SpecificJsonStreamParser GetParser(Type type, Encoding encoding)
        {
            SpecificJsonStreamParser specificJsonParser;
            if (!_readerCache.TryGetValue(type, out specificJsonParser))
            {
                lock (_readerCache)
                {
                    if (!_readerCache.TryGetValue(type, out specificJsonParser))
                    {
                        specificJsonParser = CreateParser(type, encoding);
                        _readerCache[type] = specificJsonParser;
                    }
                }
            }
            return specificJsonParser;
        }

        private static SpecificJsonStreamParser CreateParser(Type type, Encoding encoding)
        {
            var instance = (ISpecificRecord)Activator.CreateInstance(type);
            return new SpecificJsonStreamParser(instance.GetSchema(), encoding);
        }
        #endregion [Private Methods]
    }
}