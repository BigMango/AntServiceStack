using System;
using System.Collections.Generic;
using System.IO;

namespace AntServiceStack.Baiji.IO
{
    public class ByteBufferInputStream : InputStream
    {
        private readonly IList<MemoryStream> _buffers;
        private int _currentBuffer;

        public ByteBufferInputStream(IList<MemoryStream> buffers)
        {
            _buffers = buffers;
        }

        public override int Read(byte[] b, int off, int len)
        {
            if (len == 0)
            {
                return 0;
            }
            MemoryStream buffer = GetNextNonEmptyBuffer();
            long remaining = buffer.Length - buffer.Position;
            if (len > remaining)
            {
                int remainingCheck = buffer.Read(b, off, (int)remaining);

                if (remainingCheck != remaining)
                {
                    throw new InvalidCastException(
                        string.Format("remainingCheck [{0}] and remaining[{1}] are different.",
                            remainingCheck, remaining));
                }
                return (int)remaining;
            }

            int lenCheck = buffer.Read(b, off, len);
            if (lenCheck != len)
            {
                throw new InvalidCastException(string.Format("lenCheck [{0}] and len[{1}] are different.",
                    lenCheck, len));
            }
            return len;
        }

        private MemoryStream GetNextNonEmptyBuffer()
        {
            while (_currentBuffer < _buffers.Count)
            {
                MemoryStream buffer = _buffers[_currentBuffer];
                if (buffer.Position < buffer.Length)
                {
                    return buffer;
                }
                _currentBuffer++;
            }
            throw new EndOfStreamException();
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }
    }
}