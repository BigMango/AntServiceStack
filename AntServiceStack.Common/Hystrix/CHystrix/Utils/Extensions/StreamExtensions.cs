namespace CHystrix.Utils.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal static class StreamExtensions
    {
        private const int DefaultBufferSize = 0x2000;

        public static void CopyTo(this Stream input, Stream output)
        {
            input.CopyTo(output, 0x2000);
        }

        public static void CopyTo(this Stream input, Stream output, int bufferSize)
        {
            if (bufferSize < 1)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }
            input.CopyTo(output, new byte[bufferSize]);
        }

        public static void CopyTo(this Stream input, Stream output, byte[] buffer)
        {
            int num;
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }
            if (buffer.Length == 0)
            {
                throw new ArgumentException("Buffer has length of 0");
            }
            while ((num = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, num);
            }
        }

        public static byte[] ReadExactly(this Stream input, int bytesToRead)
        {
            return input.ReadExactly(new byte[bytesToRead]);
        }

        public static byte[] ReadExactly(this Stream input, byte[] buffer)
        {
            return input.ReadExactly(buffer, buffer.Length);
        }

        public static byte[] ReadExactly(this Stream input, byte[] buffer, int bytesToRead)
        {
            return input.ReadExactly(buffer, 0, bytesToRead);
        }

        public static byte[] ReadExactly(this Stream input, byte[] buffer, int startIndex, int bytesToRead)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((startIndex < 0) || (startIndex >= buffer.Length))
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if ((bytesToRead < 1) || ((startIndex + bytesToRead) > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("bytesToRead");
            }
            return ReadExactlyFast(input, buffer, startIndex, bytesToRead);
        }

        private static byte[] ReadExactlyFast(Stream fromStream, byte[] intoBuffer, int startAtIndex, int bytesToRead)
        {
            int num2;
            for (int i = 0; i < bytesToRead; i += num2)
            {
                num2 = fromStream.Read(intoBuffer, startAtIndex + i, bytesToRead - i);
                if (num2 == 0)
                {
                    throw new EndOfStreamException(string.Format("End of stream reached with {0} byte{1} left to read.", bytesToRead - i, ((bytesToRead - i) == 1) ? "s" : ""));
                }
            }
            return intoBuffer;
        }

        public static byte[] ReadFully(this Stream input)
        {
            return input.ReadFully(0x2000);
        }

        public static byte[] ReadFully(this Stream input, int bufferSize)
        {
            if (bufferSize < 1)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }
            return input.ReadFully(new byte[bufferSize]);
        }

        public static byte[] ReadFully(this Stream input, byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (buffer.Length == 0)
            {
                throw new ArgumentException("Buffer has length of 0");
            }
            using (MemoryStream stream = new MemoryStream())
            {
                input.CopyTo(stream, buffer);
                if (stream.Length == stream.GetBuffer().Length)
                {
                    return stream.GetBuffer();
                }
                return stream.ToArray();
            }
        }

        public static IEnumerable<string> ReadLines(this StreamReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            while (true)
            {
                string iteratorVariable0 = reader.ReadLine();
                if (iteratorVariable0 == null)
                {
                    yield break;
                }
                yield return iteratorVariable0;
            }
        }

        public static void WriteTo(this Stream inStream, Stream outStream)
        {
            MemoryStream stream = inStream as MemoryStream;
            if (stream != null)
            {
                stream.WriteTo(outStream);
            }
            else
            {
                int num;
                byte[] buffer = new byte[0x1000];
                while ((num = inStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outStream.Write(buffer, 0, num);
                }
            }
        }

    }
}

