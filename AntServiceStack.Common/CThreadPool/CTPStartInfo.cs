using System;
using System.Threading;

namespace AntServiceStack.Threading
{
    /// <summary>
    /// Summary description for CTPStartInfo.
    /// </summary>
    public class CTPStartInfo : WIGStartInfo
    {
        private int _idleTimeout = CThreadPool.DefaultIdleTimeout;
        private int _minWorkerThreads = CThreadPool.DefaultMinWorkerThreads;
        private int _maxWorkerThreads = CThreadPool.DefaultMaxWorkerThreads;
        private ThreadPriority _threadPriority = CThreadPool.DefaultThreadPriority;
        private string _performanceCounterInstanceName = CThreadPool.DefaultPerformanceCounterInstanceName;
        private bool _areThreadsBackground = CThreadPool.DefaultAreThreadsBackground;
        private bool _enableLocalPerformanceCounters;
        private string _threadPoolName = CThreadPool.DefaultThreadPoolName;
        private int? _maxStackSize = CThreadPool.DefaultMaxStackSize;

        public CTPStartInfo()
        {
            _performanceCounterInstanceName = CThreadPool.DefaultPerformanceCounterInstanceName;
            _threadPriority = CThreadPool.DefaultThreadPriority;
            _maxWorkerThreads = CThreadPool.DefaultMaxWorkerThreads;
            _idleTimeout = CThreadPool.DefaultIdleTimeout;
            _minWorkerThreads = CThreadPool.DefaultMinWorkerThreads;
        }

        public CTPStartInfo(CTPStartInfo ctpStartInfo)
            : base(ctpStartInfo)
        {
            _idleTimeout = ctpStartInfo.IdleTimeout;
            _minWorkerThreads = ctpStartInfo.MinWorkerThreads;
            _maxWorkerThreads = ctpStartInfo.MaxWorkerThreads;
            _threadPriority = ctpStartInfo.ThreadPriority;
            _performanceCounterInstanceName = ctpStartInfo.PerformanceCounterInstanceName;
            _enableLocalPerformanceCounters = ctpStartInfo._enableLocalPerformanceCounters;
            _threadPoolName = ctpStartInfo._threadPoolName;
            _areThreadsBackground = ctpStartInfo.AreThreadsBackground;
            _apartmentState = ctpStartInfo._apartmentState;
        }

        /// <summary>
        /// Get/Set the idle timeout in milliseconds.
        /// If a thread is idle (starved) longer than IdleTimeout then it may quit.
        /// </summary>
        public virtual int IdleTimeout
        {
            get { return _idleTimeout; }
            set
            {
                ThrowIfReadOnly();
                _idleTimeout = value;
            }
        }


        /// <summary>
        /// Get/Set the lower limit of threads in the pool.
        /// </summary>
        public virtual int MinWorkerThreads
        {
            get { return _minWorkerThreads; }
            set
            {
                ThrowIfReadOnly();
                _minWorkerThreads = value;
            }
        }


        /// <summary>
        /// Get/Set the upper limit of threads in the pool.
        /// </summary>
        public virtual int MaxWorkerThreads
        {
            get { return _maxWorkerThreads; }
            set
            {
                ThrowIfReadOnly();
                _maxWorkerThreads = value;
            }
        }

        /// <summary>
        /// Get/Set the scheduling priority of the threads in the pool.
        /// The Os handles the scheduling.
        /// </summary>
        public virtual ThreadPriority ThreadPriority
        {
            get { return _threadPriority; }
            set
            {
                ThrowIfReadOnly();
                _threadPriority = value;
            }
        }

        /// <summary>
        /// Get/Set the thread pool name. Threads will get names depending on this.
        /// </summary>
        public virtual string ThreadPoolName
        {
            get { return _threadPoolName; }
            set
            {
                ThrowIfReadOnly();
                _threadPoolName = value;
            }
        }

        /// <summary>
        /// Get/Set the performance counter instance name of this CThreadPool
        /// The default is null which indicate not to use performance counters at all.
        /// </summary>
        public virtual string PerformanceCounterInstanceName
        {
            get { return _performanceCounterInstanceName; }
            set
            {
                ThrowIfReadOnly();
                _performanceCounterInstanceName = value;
            }
        }

        /// <summary>
        /// Enable/Disable the local performance counter.
        /// This enables the user to get some performance information about the CThreadPool 
        /// without using Windows performance counters. (Useful on WindowsCE, Silverlight, etc.)
        /// The default is false.
        /// </summary>
        public virtual bool EnableLocalPerformanceCounters
        {
            get { return _enableLocalPerformanceCounters; }
            set
            {
                ThrowIfReadOnly();
                _enableLocalPerformanceCounters = value;
            }
        }

        /// <summary>
        /// Get/Set backgroundness of thread in thread pool.
        /// </summary>
        public virtual bool AreThreadsBackground
        {
            get { return _areThreadsBackground; }
            set
            {
                ThrowIfReadOnly();
                _areThreadsBackground = value;
            }
        }

        /// <summary>
        /// Get a readonly version of this CTPStartInfo.
        /// </summary>
        /// <returns>Returns a readonly reference to this CTPStartInfo</returns>
        public new CTPStartInfo AsReadOnly()
        {
            return new CTPStartInfo(this) { _readOnly = true };
        }

        private ApartmentState _apartmentState = CThreadPool.DefaultApartmentState;

        /// <summary>
        /// Get/Set the apartment state of threads in the thread pool
        /// </summary>
        public ApartmentState ApartmentState
        {
            get { return _apartmentState; }
            set
            {
                ThrowIfReadOnly();
                _apartmentState = value;
            }
        }

        /// <summary>
        /// Get/Set the max stack size of threads in the thread pool
        /// </summary>
        public int? MaxStackSize
        {
            get { return _maxStackSize; }
            set
            {
                ThrowIfReadOnly();
                if (value.HasValue && value.Value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Value must be greater than 0.");
                }
                _maxStackSize = value;
            }
        }
    }
}
