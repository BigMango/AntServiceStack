
using System;
using AntServiceStack.Threading.Internal;

namespace AntServiceStack.Threading
{
    public partial class CThreadPool
    {
        #region ThreadEntry class

        internal class ThreadEntry
        {
            /// <summary>
            /// The thread creation time
            /// The value is stored as UTC value.
            /// </summary>
            private readonly DateTime _creationTime;

            /// <summary>
            /// The last time this thread has been running
            /// It is updated by IAmAlive() method
            /// The value is stored as UTC value.
            /// </summary>
            private DateTime _lastAliveTime;

            /// <summary>
            /// A reference from each thread in the thread pool to its CThreadPool
            /// object container.
            /// With this variable a thread can know whatever it belongs to a 
            /// CThreadPool.
            /// </summary>
            private readonly CThreadPool _associatedCThreadPool;

            /// <summary>
            /// A reference to the current work item a thread from the thread pool 
            /// is executing.
            /// </summary>            
            public WorkItem CurrentWorkItem { get; set; }

            public ThreadEntry(CThreadPool ctp)
            {
                _associatedCThreadPool = ctp;
                _creationTime = DateTime.UtcNow;
                _lastAliveTime = DateTime.MinValue;
            }

            public CThreadPool AssociatedCThreadPool
            {
                get { return _associatedCThreadPool; }
            }

            public void IAmAlive()
            {
                _lastAliveTime = DateTime.UtcNow;
            }
        }

        #endregion
    }
}