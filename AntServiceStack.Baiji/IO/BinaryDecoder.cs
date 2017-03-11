using AntServiceStack.Baiji.Utils;
using System;
using System.IO;
using System.Text;
using AntServiceStack.Baiji.Exceptions;

namespace AntServiceStack.Baiji.IO
{
    /// <summary>
    /// Decoder for Baiji binary format
    /// </summary>
    public class BinaryDecoder : IDecoder
    {
        private readonly Stream _stream;

        public BinaryDecoder(Stream stream)
        {
            _stream = stream;
        }

        /// <summary>
        /// null is written as zero bytes
        /// </summary>
        public void ReadNull()
        {
        }

        /// <summary>
        /// a boolean is written as a single byte 
        /// whose value is either 0 (false) or 1 (true).
        /// </summary>
        /// <returns></returns>
        public bool ReadBoolean()
        {
            byte b = Read();
            if (b == 0)
            {
                return false;
            }
            if (b == 1)
            {
                return true;
            }
            throw new BaijiException("Not a boolean value in the stream: " + b);
        }

        /// <summary>
        /// int and long values are written using variable-length, zig-zag coding.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public int ReadInt()
        {
            return (int)ReadLong();
        }

        /// <summary>
        /// int and long values are written using variable-length, zig-zag coding.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public long ReadLong()
        {
            byte b = Read();
            ulong n = b & 0x7FUL;
            int shift = 7;
            while ((b & 0x80) != 0)
            {
                b = Read();
                n |= (b & 0x7FUL) << shift;
                shift += 7;
            }
            long value = (long)n;
            return (-(value & 0x01L)) ^ ((value >> 1) & 0x7fffffffffffffffL);
        }

        /// <summary>
        /// A float is written as 4 bytes.
        /// The float is converted into a 32-bit integer using a method equivalent to
        /// Java's floatToIntBits and then encoded in little-endian format.
        /// </summary>
        /// <returns></returns>
        public float ReadFloat()
        {
            byte[] buffer = Read(4);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            return BitConverter.ToSingle(buffer, 0);

            //int bits = (Stream.ReadByte() & 0xff |
            //(Stream.ReadByte()) & 0xff << 8 |
            //(Stream.ReadByte()) & 0xff << 16 |
            //(Stream.ReadByte()) & 0xff << 24);
            //return intBitsToFloat(bits);
        }

        /// <summary>
        /// A double is written as 8 bytes.
        /// The double is converted into a 64-bit integer using a method equivalent to
        /// Java's doubleToLongBits and then encoded in little-endian format.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public double ReadDouble()
        {
            long bits = (_stream.ReadByte() & 0xffL) |
                        (_stream.ReadByte() & 0xffL) << 8 |
                        (_stream.ReadByte() & 0xffL) << 16 |
                        (_stream.ReadByte() & 0xffL) << 24 |
                        (_stream.ReadByte() & 0xffL) << 32 |
                        (_stream.ReadByte() & 0xffL) << 40 |
                        (_stream.ReadByte() & 0xffL) << 48 |
                        (_stream.ReadByte() & 0xffL) << 56;
            return BitConverter.Int64BitsToDouble(bits);
        }

        /// <summary>
        /// Bytes are encoded as a long followed by that many bytes of data. 
        /// </summary>
        /// <returns></returns>
        public byte[] ReadBytes()
        {
            return Read(ReadLong());
        }

        public string ReadString()
        {
            int length = ReadInt();
            byte[] buffer = new byte[length];
            Read(buffer, 0, length);
            return Encoding.UTF8.GetString(buffer, 0, length);
        }

        public int ReadEnum()
        {
            return ReadInt();
        }

        public DateTime ReadDateTime()
        {
            return DateTimeUtils.GetDateFromTimeInterval(ReadLong(), ReadLong());
        }

        public long ReadArrayStart()
        {
            return DoReadItemCount();
        }

        public long ReadArrayNext()
        {
            return DoReadItemCount();
        }

        public long ReadMapStart()
        {
            return DoReadItemCount();
        }

        public long ReadMapNext()
        {
            return DoReadItemCount();
        }

        public int ReadUnionIndex()
        {
            return ReadInt();
        }
        
        // Read p bytes into a new byte buffer
        private byte[] Read(long p)
        {
            byte[] buffer = new byte[p];
            Read(buffer, 0, buffer.Length);
            return buffer;
        }

        private byte Read()
        {
            int n = _stream.ReadByte();
            if (n >= 0)
            {
                return (byte)n;
            }
            throw new BaijiException("End of stream reached");
        }

        private void Read(byte[] buffer, int start, int len)
        {
            while (len > 0)
            {
                int n = _stream.Read(buffer, start, len);
                if (n <= 0)
                {
                    throw new BaijiException("End of stream reached");
                }
                start += n;
                len -= n;
            }
        }

        private long DoReadItemCount()
        {
            long result = ReadLong();
            if (result < 0)
            {
                ReadLong(); // Consume byte-count if present
                result = -result;
            }
            return result;
        }
    }
}