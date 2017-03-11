using System;
using System.Collections.Generic;
using AntServiceStack.Baiji.Exceptions;

namespace AntServiceStack.Baiji.Specific
{
    public static class TypeConverter
    {
        public delegate object ConvertDelegate(object obj);

        private static IDictionary<string, ConvertDelegate> _converters = new Dictionary<string, ConvertDelegate>();

        static TypeConverter()
        {
            BaseTypeConverter.RegisterConverters();
        }

        public static void RegisterConverter(Type fromType, Type toType, ConvertDelegate converter)
        {
            var key = fromType.ToString() + "-" + toType.ToString();
            if (_converters.ContainsKey(key))
            {
                _converters[key] = converter;
                return;
            }
            _converters.Add(key, converter);
        }

        private static ConvertDelegate GetConverter(Type fromType, Type toType)
        {
            ConvertDelegate converter;
            var key = fromType.ToString() + "-" + toType.ToString();
            if (_converters.TryGetValue(key, out converter))
            {
                return converter;
            }
            else
            {
                throw new BaijiRuntimeException(String.Format(
                    "Cannot find type converter converting {0} to {1}", fromType.ToString(), toType.ToString()));
            }
        }

        public static W Convert<T, W>(T from)
        {
            var converter = GetConverter(typeof(T), typeof(W));
            return (W)converter(from);
        }

        public static W? Convert<T, W>(T? from)
            where T : struct
            where W : struct
        {
            if (from == null)
                return null;
            var converter = GetConverter(typeof(T), typeof(W));
            return (W?)converter(from);
        }

        public static List<W> ConvertToList<T, W>(IEnumerable<T> from)
        {
            if (from == null)
                return null;
            List<W> list = new List<W>();
            foreach (var t in from)
            {
                var w = Convert<T, W>(t);
                list.Add(w);
            }
            return list;
        }

        public static W[] ConvertToArray<T, W>(IEnumerable<T> from)
        {
            var list = ConvertToList<T, W>(from);
            return list == null ? null : list.ToArray();
        }

        public static List<W?> ConvertToList<T, W>(IEnumerable<T?> from)
            where W : struct
            where T : struct
        {
            if (from == null)
                return null;
            List<W?> list = new List<W?>();
            foreach (var t in from)
            {
                var w = Convert<T, W>(t);
                list.Add(w);
            }
            return list;
        }

        public static W?[] ConvertToArray<T, W>(IEnumerable<T?> from)
            where W : struct
            where T : struct
        {
            var list = ConvertToList<T, W>(from);
            return list == null ? null : list.ToArray();
        }
    }
}