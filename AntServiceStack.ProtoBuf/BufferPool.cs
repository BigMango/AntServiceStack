using System;
using System.Collections.Concurrent;
using System.Threading;

namespace AntServiceStack.ProtoBuf
{
    //After
    internal static class BufferPool
    {
        const int BaseLength = 1024;
        const int PoolSize = 22;

        static readonly BytePool[] BytePools = new BytePool[PoolSize];

        static BufferPool()
        {
            Reset();
        }

        static void Reset()
        {
            BytePools[BytePools.Length - 1] = new BytePool(int.MaxValue);
            for (int i = 0; i < BytePools.Length - 1; i++)
                BytePools[i] = new BytePool((int)Math.Pow(2, i + 10));
        }

        internal static void Flush()
        {
            Reset();
        }

        internal static byte[] GetBuffer()
        {
            return TakeBufferBytes(BaseLength);
        }

        internal static void ResizeAndFlushLeft(ref byte[] buffer, int toFitAtLeastBytes, int copyFromIndex, int copyBytes)
        {
            if (buffer == null || toFitAtLeastBytes < buffer.Length)
                return;
            if (copyFromIndex < 0 || copyBytes < 0)
                return;

            // try doubling, else match
            byte[] newBuffer = TakeBufferBytes(Math.Max(buffer.Length * 2, toFitAtLeastBytes));

            if (copyBytes > 0)
            {
                Helpers.BlockCopy(buffer, copyFromIndex, newBuffer, 0, copyBytes);
            }

            ReleaseBufferToPool(ref buffer);
            buffer = newBuffer;
        }

        internal static void ReleaseBufferToPool(ref byte[] buffer)
        {
            if (buffer == null)
                return;

            BytePool bytePool = BytePools[GetPoolIndex(buffer.Length)];
            bytePool.Return(ref buffer);

            buffer = null;
        }

        private static byte[] TakeBufferBytes(int size)
        {
            BytePool bytePool = BytePools[GetPoolIndex(size)];

            return bytePool.Take();
        }

        private static int GetPoolIndex(int size)
        {
            if (size < BaseLength)
                return 0;

            int idx = (int)Math.Ceiling(Math.Log(size, 2)) - 10;
            return idx < BytePools.Length ? idx : BytePools.Length - 1;
        }

        class BytePool : IComparable<BytePool>
        {
            private readonly int _size;
            private ConcurrentQueue<WeakReference> _pool;

            private const int QueueSizeLimit = 512;

            public int Size
            {
                get { return _size; }
            }

            public BytePool(int size)
            {
                Helpers.DebugAssert(size > 0, "BytePool size must be greater than 0.");

                _size = size;
                _pool = new ConcurrentQueue<WeakReference>();
            }

            public byte[] Take()
            {
                WeakReference weak;
                object result = null;
                while (_pool.TryDequeue(out weak))
                {
                    result = weak.Target;
                    if (result != null)
                        break;
                }

                return result == null ? new byte[_size] : (byte[])result;
            }

            public bool Return(ref byte[] item)
            {
                if (item == null || item.Length != _size)
                    return false;

                if (_pool.Count > QueueSizeLimit)
                    return false;

                _pool.Enqueue(new WeakReference(item));
                item = null;
                return true;
            }

            public int CompareTo(BytePool other)
            {
                if (other == null)
                    return 1;

                if (this._size < other._size)
                    return -1;

                if (this._size > other._size)
                    return 1;

                return 0;
            }
        }
    }
}