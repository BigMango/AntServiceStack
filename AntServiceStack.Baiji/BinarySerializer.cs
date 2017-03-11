using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AntServiceStack.Baiji.Generic;
using AntServiceStack.Baiji.IO;
using AntServiceStack.Baiji.Specific;

namespace AntServiceStack.Baiji
{
    public class BinarySerializer : ISerializer
    {
        #region [Private Fields]
        private static readonly IDictionary<Type, object> _readerCache =
            new Dictionary<Type, object>();

        private static readonly IDictionary<Type, object> _writerCache =
            new Dictionary<Type, object>();
        #endregion

        #region Implementation of ISerializer
        public void Serialize(object obj, Stream stream)
        {
            var record = obj as ISpecificRecord;
            if (record == null)
            {
                throw new ArgumentException("obj doesn't implement ISpecifiedRecord interface.");
            }
            var writer = GetWriter(obj);
            writer.Write(obj, new BinaryEncoder(stream));
        }

        public void Serialize<T>(T obj, Stream stream) where T : ISpecificRecord, new()
        {
            var writer = GetWriter(obj);
            writer.Write(obj, new BinaryEncoder(stream));
        }

        public object Deserialize(Type type, Stream stream)
        {
            if (!type.GetInterfaces().Any(i => i == typeof(ISpecificRecord)))
            {
                throw new ArgumentException("type doesn't implement ISpecifiedRecord interface.");
            }
            var reader = GetReader(type);
            return reader.Read(null, new BinaryDecoder(stream));
        }

        public T Deserialize<T>(Stream stream) where T : ISpecificRecord, new()
        {
            var reader = GetReader(typeof(T));
            return reader.Read(default(T), new BinaryDecoder(stream));
        }

        /// <summary>
        /// This method is designed for testing, and is not expected to be used in any other cases.
        /// </summary>
        public void ClearCache()
        {
            lock (this)
            {
                _readerCache.Clear();
                _writerCache.Clear();
            }
        }
        #endregion

        #region [Private Methods]
        private static DatumWriter GetWriter(object obj)
        {
            Object ojt ;
            if (!_writerCache.TryGetValue(obj.GetType(), out ojt))
            {
                lock (_writerCache)
                {
                    if (!_writerCache.TryGetValue(obj.GetType(), out ojt))
                    {
                        ojt =  CreateWrite(obj.GetType());
                        _writerCache[obj.GetType()] = ojt;
                    }
                }
            }
            return (DatumWriter)ojt;
        }

        private static DatumReader GetReader(Type type)
        {
            Object obj;
            if (!_readerCache.TryGetValue(type, out obj))
            {
                lock (_readerCache)
                {
                    if (!_readerCache.TryGetValue(type, out obj))
                    {
                        obj = CreateReader(type);
                        _readerCache[type] = obj;
                    }
                }
            }
            return (DatumReader)obj;
        }

        private static DatumReader CreateReader(Type type)
        {
            var instance = (ISpecificRecord)Activator.CreateInstance(type);
            return new SpecificDatumReader(instance.GetSchema());
        }

        private static DatumWriter CreateWrite(Type type)
        {
            var instance = (ISpecificRecord)Activator.CreateInstance(type);
            return new SpecificDatumWriter(instance.GetSchema());
        }
        #endregion
    }
}