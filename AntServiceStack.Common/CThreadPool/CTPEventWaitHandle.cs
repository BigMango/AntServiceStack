using System;
using System.Threading;

namespace AntServiceStack.Threading.Internal
{
    internal static class CTPEventWaitHandle
    {
        public const int WaitTimeout = Timeout.Infinite;

        internal static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext)
        {
            return WaitHandle.WaitAll(waitHandles, millisecondsTimeout, exitContext);
        }

        internal static int WaitAny(WaitHandle[] waitHandles)
        {
            return WaitHandle.WaitAny(waitHandles);
        }

        internal static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext)
        {
            return WaitHandle.WaitAny(waitHandles, millisecondsTimeout, exitContext);
        }

        internal static bool WaitOne(WaitHandle waitHandle, int millisecondsTimeout, bool exitContext)
        {
            return waitHandle.WaitOne(millisecondsTimeout, exitContext);
        }
    }

}