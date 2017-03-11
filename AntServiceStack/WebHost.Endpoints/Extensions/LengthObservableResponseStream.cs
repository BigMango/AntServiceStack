using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AntServiceStack.WebHost.Endpoints.Extensions
{
    /// <summary>
    /// A wrapper around http response stream to make the length of the stream observable
    /// </summary>
    public class LengthObservableResponseStream : Stream
    {
        private Stream _stream = null;
        private long _length = 0;

        public LengthObservableResponseStream(Stream wrappedStream)
        {
            this._stream = wrappedStream;
        }

        public override bool CanRead
        {
            get { return _stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _stream.CanWrite; }
        }

        public override void Flush()
        {
            this._stream.Flush();
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get
            {
                return _stream.Position;
            }
            set
            {
                _stream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this._stream.SetLength(value);
            this._length = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this._stream.Write(buffer, offset, count);
            this._length += (long)count;
        }

        public override void Close()
        {
            // We do not close inner wrapped stream here
            // Since outer caller will close it.
            base.Close();
        }
    }
}
