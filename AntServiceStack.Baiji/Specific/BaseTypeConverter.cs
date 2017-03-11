
using System;

namespace AntServiceStack.Baiji.Specific
{
    public static class BaseTypeConverter
    {
        public static void RegisterConverters()
        {
            TypeConverter.RegisterConverter(typeof(SByte), typeof(int), (w) => { return (int)(sbyte)w; });
            TypeConverter.RegisterConverter(typeof(Decimal?), typeof(string), (w) => { if (w == null) return null; return w.ToString(); });
            TypeConverter.RegisterConverter(typeof(Decimal), typeof(string), (w) => { return w.ToString(); });
            TypeConverter.RegisterConverter(typeof(Int16), typeof(int), (w) => { return (int)(short)w; });
            TypeConverter.RegisterConverter(typeof(Byte), typeof(int), (w) => { return (int)(byte)w; });
            TypeConverter.RegisterConverter(typeof(UInt32), typeof(long), (w) => { return (long)(uint)w; });
            TypeConverter.RegisterConverter(typeof(UInt64?), typeof(string), (w) => { if (w == null) return null; return w.ToString(); });
            TypeConverter.RegisterConverter(typeof(UInt64), typeof(string), (w) => { return w.ToString(); });
            TypeConverter.RegisterConverter(typeof(UInt16), typeof(int), (w) => { return (int)(ushort)w; });
            TypeConverter.RegisterConverter(typeof(Guid?), typeof(string), (w) => { if (w == null) return null; return w.ToString(); });
            TypeConverter.RegisterConverter(typeof(Guid), typeof(string), (w) => { return w.ToString(); });
            TypeConverter.RegisterConverter(typeof(Uri), typeof(string), (w) => { if (w == null) return null; return w.ToString(); });
            TypeConverter.RegisterConverter(typeof(TimeSpan?), typeof(string), (w) => { if (w == null) return null; return ConvertTimeSpan((TimeSpan)w); });
            TypeConverter.RegisterConverter(typeof(TimeSpan), typeof(string), (w) => { return ConvertTimeSpan((TimeSpan)w); });

            TypeConverter.RegisterConverter(typeof(int), typeof(SByte), (w) => { return (SByte)(int)w; });
            TypeConverter.RegisterConverter(typeof(string), typeof(Decimal?), (w) => { if (w == null) return null; return Convert.ToDecimal((string)w); });
            TypeConverter.RegisterConverter(typeof(string), typeof(Decimal), (w) => { if (w == null) return default(Decimal); return Convert.ToDecimal((string)w); });
            TypeConverter.RegisterConverter(typeof(int), typeof(Byte), (w) => { return (Byte)(int)w; });
            TypeConverter.RegisterConverter(typeof(int), typeof(Int16), (w) => { return (Int16)(int)w; });
            TypeConverter.RegisterConverter(typeof(long), typeof(UInt32), (w) => { return (UInt32)(long)w; });
            TypeConverter.RegisterConverter(typeof(string), typeof(UInt64?), (w) => { if (w == null) return null; return Convert.ToUInt64((string)w); });
            TypeConverter.RegisterConverter(typeof(string), typeof(UInt64), (w) => { if (w == null) return default(UInt64); return Convert.ToUInt64((string)w); });
            TypeConverter.RegisterConverter(typeof(int), typeof(UInt16), (w) => { return (UInt16)(int)w; });
            TypeConverter.RegisterConverter(typeof(string), typeof(Guid?), (w) => { if (w == null) return null; return new Guid((string)w); });
            TypeConverter.RegisterConverter(typeof(string), typeof(Guid), (w) => { if (w == null) return default(Guid); return new Guid((string)w); });
            TypeConverter.RegisterConverter(typeof(string), typeof(Uri), (w) => { if (w == null) return null; return new Uri((string)w); });
            TypeConverter.RegisterConverter(typeof(string), typeof(TimeSpan?), (w) => { if (w == null) return null; return TimeSpan.FromMilliseconds(Convert.ToInt64((string)w)); });
            TypeConverter.RegisterConverter(typeof(string), typeof(TimeSpan), (w) => { if (w == null) return default(TimeSpan); return TimeSpan.FromMilliseconds(Convert.ToInt64((string)w)); });
        }

        private static string ConvertTimeSpan(TimeSpan duration)
        {
            double interval = ((TimeSpan)duration).TotalMilliseconds;
            return ((long)interval).ToString();
        }
    }
}
