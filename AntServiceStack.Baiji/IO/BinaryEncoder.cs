using AntServiceStack.Baiji.Utils;
using System;
using System.IO;
using System.Text;

namespace AntServiceStack.Baiji.IO
{
    /// <summary>
    /// Write leaf values.
    /// </summary>
    public class BinaryEncoder : IEncoder
    {
        private readonly Stream _stream;

        public BinaryEncoder() : this(null)
        {
        }

        public BinaryEncoder(Stream stream)
        {
            _stream = stream;
        }

        /// <summary>
        /// null is written as zero bytes
        /// </summary>
        public void WriteNull()
        {
        }

        /// <summary>
        /// true is written as 1 and false 0.
        /// </summary>
        /// <param name="b">Boolean value to write</param>
        public void WriteBoolean(bool b)
        {
            DoWriteByte((byte)(b ? 1 : 0));
        }

        /// <summary>
        /// int and long values are written using variable-length, zig-zag coding.
        /// </summary>
        /// <param name="value"></param>
        public void WriteInt(int value)
        {
            WriteLong(value);
        }

        /// <summary>
        /// int and long values are written using variable-length, zig-zag coding.
        /// </summary>
        /// <param name="value"></param>
        public void WriteLong(long value)
        {
            var n = (ulong)((value << 1) ^ (value >> 63));
            while ((n & ~0x7FUL) != 0)
            {
                DoWriteByte((byte)((n & 0x7f) | 0x80));
                n >>= 7;
            }
            DoWriteByte((byte)n);
        }

        /// <summary>
        /// A float is written as 4 bytes.
        /// The float is converted into a 32-bit integer using a method equivalent to
        /// Java's floatToIntBits and then encoded in little-endian format.
        /// </summary>
        /// <param name="value"></param>
        public void WriteFloat(float value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            DoWriteBytes(buffer);
        }

        /// <summary>
        ///A double is written as 8 bytes.
        ///The double is converted into a 64-bit integer using a method equivalent to
        ///Java's doubleToLongBits and then encoded in little-endian format.
        /// </summary>
        /// <param name="value"></param>
        public void WriteDouble(double value)
        {
            long bits = BitConverter.DoubleToInt64Bits(value);

            DoWriteByte((byte)((bits) & 0xFF));
            DoWriteByte((byte)((bits >> 8) & 0xFF));
            DoWriteByte((byte)((bits >> 16) & 0xFF));
            DoWriteByte((byte)((bits >> 24) & 0xFF));
            DoWriteByte((byte)((bits >> 32) & 0xFF));
            DoWriteByte((byte)((bits >> 40) & 0xFF));
            DoWriteByte((byte)((bits >> 48) & 0xFF));
            DoWriteByte((byte)((bits >> 56) & 0xFF));
        }

        /// <summary>
        /// Bytes are encoded as a long followed by that many bytes of data.
        /// </summary>
        /// <param name="value"></param>
        public void WriteBytes(byte[] value)
        {
            WriteLong(value.Length);
            DoWriteBytes(value);
        }

        /// <summary>
        /// A string is encoded as a long followed by
        /// that many bytes of UTF-8 encoded character data.
        /// </summary>
        /// <param name="value"></param>
        public void WriteString(string value)
        {
            WriteBytes(Encoding.UTF8.GetBytes(value));
        }

        public void WriteEnum(int value)
        {
            WriteLong(value);
        }

        public void WriteDateTime(DateTime value)
        {
            WriteLong(DateTimeUtils.GetTimeIntervalLongValue(value));
            WriteLong(DateTimeUtils.GetTimeOffsetTotalMinutes(value));
        }

        public void StartItem()
        {
        }

        public void SetItemCount(long value)
        {
            if (value > 0)
            {
                WriteLong(value);
            }
        }

        public void WriteArrayStart()
        {
        }

        public void WriteArrayEnd()
        {
            WriteLong(0);
        }

        public void WriteMapStart()
        {
        }

        public void WriteMapEnd()
        {
            WriteLong(0);
        }

        public void WriteUnionIndex(int value)
        {
            WriteLong(value);
        }

        private void DoWriteBytes(byte[] bytes)
        {
            _stream.Write(bytes, 0, bytes.Length);
        }

        private void DoWriteByte(byte b)
        {
            _stream.WriteByte(b);
        }

        public void Flush()
        {
            _stream.Flush();
        }
    }
}