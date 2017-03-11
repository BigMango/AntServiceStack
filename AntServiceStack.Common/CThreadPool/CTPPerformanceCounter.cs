using System;
using System.Diagnostics;
using System.Threading;

namespace AntServiceStack.Threading
{
    public interface ICTPPerformanceCountersReader
    {
        long InUseThreads { get; }
        long ActiveThreads { get; }
        long WorkItemsQueued { get; }
        long WorkItemsProcessed { get; }
    }
}

namespace AntServiceStack.Threading.Internal
{
    internal interface ICTPInstancePerformanceCounters : IDisposable
    {
        void Close();
        void SampleThreads(long activeThreads, long inUseThreads);
        void SampleWorkItems(long workItemsQueued, long workItemsProcessed);
        void SampleWorkItemsWaitTime(TimeSpan workItemWaitTime);
        void SampleWorkItemsProcessTime(TimeSpan workItemProcessTime);
    }

    internal enum CTPPerformanceCounterType
    {
        // Fields
        ActiveThreads = 0,
        InUseThreads = 1,
        OverheadThreads = 2,
        OverheadThreadsPercent = 3,
        OverheadThreadsPercentBase = 4,

        WorkItems = 5,
        WorkItemsInQueue = 6,
        WorkItemsProcessed = 7,

        WorkItemsQueuedPerSecond = 8,
        WorkItemsProcessedPerSecond = 9,

        AvgWorkItemWaitTime = 10,
        AvgWorkItemWaitTimeBase = 11,

        AvgWorkItemProcessTime = 12,
        AvgWorkItemProcessTimeBase = 13,

        WorkItemsGroups = 14,

        LastCounter = 14,
    }


    /// <summary>
    /// Summary description for CTPPerformanceCounter.
    /// </summary>
    internal class CTPPerformanceCounter
    {
        // Fields
        private readonly PerformanceCounterType _pcType;
        protected string _counterHelp;
        protected string _counterName;

        // Methods
        public CTPPerformanceCounter(
            string counterName,
            string counterHelp,
            PerformanceCounterType pcType)
        {
            _counterName = counterName;
            _counterHelp = counterHelp;
            _pcType = pcType;
        }

        public void AddCounterToCollection(CounterCreationDataCollection counterData)
        {
            CounterCreationData counterCreationData = new CounterCreationData(
                _counterName,
                _counterHelp,
                _pcType);

            counterData.Add(counterCreationData);
        }

        // Properties
        public string Name
        {
            get
            {
                return _counterName;
            }
        }
    }

    internal class CTPPerformanceCounters
    {
        // Fields
        internal CTPPerformanceCounter[] _ctpPerformanceCounters;
        private static readonly CTPPerformanceCounters _instance;
        internal const string _ctpCategoryHelp = "CThreadPool performance counters";
        internal const string _ctpCategoryName = "CThreadPool";

        // Methods
        static CTPPerformanceCounters()
        {
            _instance = new CTPPerformanceCounters();
        }

        private CTPPerformanceCounters()
        {
            CTPPerformanceCounter[] ctpPerformanceCounters = new CTPPerformanceCounter[] 
				{ 
					new CTPPerformanceCounter("Active threads", "The current number of available in the thread pool.", PerformanceCounterType.NumberOfItems32), 
					new CTPPerformanceCounter("In use threads", "The current number of threads that execute a work item.", PerformanceCounterType.NumberOfItems32), 
					new CTPPerformanceCounter("Overhead threads", "The current number of threads that are active, but are not in use.", PerformanceCounterType.NumberOfItems32), 
					new CTPPerformanceCounter("% overhead threads", "The current number of threads that are active, but are not in use in percents.", PerformanceCounterType.RawFraction), 
					new CTPPerformanceCounter("% overhead threads base", "The current number of threads that are active, but are not in use in percents.", PerformanceCounterType.RawBase), 

					new CTPPerformanceCounter("Work Items", "The number of work items in the CTrip Thread Pool. Both queued and processed.", PerformanceCounterType.NumberOfItems32), 
					new CTPPerformanceCounter("Work Items in queue", "The current number of work items in the queue", PerformanceCounterType.NumberOfItems32), 
					new CTPPerformanceCounter("Work Items processed", "The number of work items already processed", PerformanceCounterType.NumberOfItems32), 

					new CTPPerformanceCounter("Work Items queued/sec", "The number of work items queued per second", PerformanceCounterType.RateOfCountsPerSecond32), 
					new CTPPerformanceCounter("Work Items processed/sec", "The number of work items processed per second", PerformanceCounterType.RateOfCountsPerSecond32), 

					new CTPPerformanceCounter("Avg. Work Item wait time(sec)", "The average time a work item supends in the queue waiting for its turn to execute.", PerformanceCounterType.AverageCount64), 
					new CTPPerformanceCounter("Avg. Work Item wait time base", "The average time a work item supends in the queue waiting for its turn to execute.", PerformanceCounterType.AverageBase), 

					new CTPPerformanceCounter("Avg. Work Item process time(sec)", "The average time it takes to process a work item.", PerformanceCounterType.AverageCount64), 
					new CTPPerformanceCounter("Avg. Work Item process time base", "The average time it takes to process a work item.", PerformanceCounterType.AverageBase), 

					new CTPPerformanceCounter("Work Items Groups", "The current number of work item groups associated with the CTrip Thread Pool.", PerformanceCounterType.NumberOfItems32), 
				};

            _ctpPerformanceCounters = ctpPerformanceCounters;
            SetupCategory();
        }

        private void SetupCategory()
        {
            if (!PerformanceCounterCategory.Exists(_ctpCategoryName))
            {
                CounterCreationDataCollection counters = new CounterCreationDataCollection();

                for (int i = 0; i < _ctpPerformanceCounters.Length; i++)
                {
                    _ctpPerformanceCounters[i].AddCounterToCollection(counters);
                }

                PerformanceCounterCategory.Create(
                    _ctpCategoryName,
                    _ctpCategoryHelp,
                    PerformanceCounterCategoryType.MultiInstance,
                    counters);

            }
        }

        // Properties
        public static CTPPerformanceCounters Instance
        {
            get
            {
                return _instance;
            }
        }
    }

    internal class CTPInstancePerformanceCounter : IDisposable
    {
        // Fields
        private bool _isDisposed;
        private PerformanceCounter _pcs;

        // Methods
        protected CTPInstancePerformanceCounter()
        {
            _isDisposed = false;
        }

        public CTPInstancePerformanceCounter(
            string instance,
            CTPPerformanceCounterType spcType)
            : this()
        {
            CTPPerformanceCounters counters = CTPPerformanceCounters.Instance;
            _pcs = new PerformanceCounter(
                CTPPerformanceCounters._ctpCategoryName,
                counters._ctpPerformanceCounters[(int)spcType].Name,
                instance,
                false);
            _pcs.RawValue = _pcs.RawValue;
        }


        public void Close()
        {
            if (_pcs != null)
            {
                _pcs.RemoveInstance();
                _pcs.Close();
                _pcs = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Close();
                }
            }
            _isDisposed = true;
        }

        public virtual void Increment()
        {
            _pcs.Increment();
        }

        public virtual void IncrementBy(long val)
        {
            _pcs.IncrementBy(val);
        }

        public virtual void Set(long val)
        {
            _pcs.RawValue = val;
        }
    }

    internal class CTPInstanceNullPerformanceCounter : CTPInstancePerformanceCounter
    {
        // Methods
        public override void Increment() { }
        public override void IncrementBy(long value) { }
        public override void Set(long val) { }
    }



    internal class CTPInstancePerformanceCounters : ICTPInstancePerformanceCounters
    {
        private bool _isDisposed;
        // Fields
        private CTPInstancePerformanceCounter[] _pcs;
        private static readonly CTPInstancePerformanceCounter _ctpInstanceNullPerformanceCounter;

        // Methods
        static CTPInstancePerformanceCounters()
        {
            _ctpInstanceNullPerformanceCounter = new CTPInstanceNullPerformanceCounter();
        }

        public CTPInstancePerformanceCounters(string instance)
        {
            _isDisposed = false;
            _pcs = new CTPInstancePerformanceCounter[(int)CTPPerformanceCounterType.LastCounter];

            // Call the CTPPerformanceCounters.Instance so the static constructor will
            // intialize the CTPPerformanceCounters singleton.
            CTPPerformanceCounters.Instance.GetHashCode();

            for (int i = 0; i < _pcs.Length; i++)
            {
                if (instance != null)
                {
                    _pcs[i] = new CTPInstancePerformanceCounter(
                        instance,
                        (CTPPerformanceCounterType)i);
                }
                else
                {
                    _pcs[i] = _ctpInstanceNullPerformanceCounter;
                }
            }
        }


        public void Close()
        {
            if (null != _pcs)
            {
                for (int i = 0; i < _pcs.Length; i++)
                {
                    if (null != _pcs[i])
                    {
                        _pcs[i].Dispose();
                    }
                }
                _pcs = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Close();
                }
            }
            _isDisposed = true;
        }

        private CTPInstancePerformanceCounter GetCounter(CTPPerformanceCounterType spcType)
        {
            return _pcs[(int)spcType];
        }

        public void SampleThreads(long activeThreads, long inUseThreads)
        {
            GetCounter(CTPPerformanceCounterType.ActiveThreads).Set(activeThreads);
            GetCounter(CTPPerformanceCounterType.InUseThreads).Set(inUseThreads);
            GetCounter(CTPPerformanceCounterType.OverheadThreads).Set(activeThreads - inUseThreads);

            GetCounter(CTPPerformanceCounterType.OverheadThreadsPercentBase).Set(activeThreads);
            GetCounter(CTPPerformanceCounterType.OverheadThreadsPercent).Set(activeThreads - inUseThreads);
        }

        public void SampleWorkItems(long workItemsQueued, long workItemsProcessed)
        {
            GetCounter(CTPPerformanceCounterType.WorkItems).Set(workItemsQueued + workItemsProcessed);
            GetCounter(CTPPerformanceCounterType.WorkItemsInQueue).Set(workItemsQueued);
            GetCounter(CTPPerformanceCounterType.WorkItemsProcessed).Set(workItemsProcessed);

            GetCounter(CTPPerformanceCounterType.WorkItemsQueuedPerSecond).Set(workItemsQueued);
            GetCounter(CTPPerformanceCounterType.WorkItemsProcessedPerSecond).Set(workItemsProcessed);
        }

        public void SampleWorkItemsWaitTime(TimeSpan workItemWaitTime)
        {
            GetCounter(CTPPerformanceCounterType.AvgWorkItemWaitTime).IncrementBy((long)workItemWaitTime.TotalMilliseconds);
            GetCounter(CTPPerformanceCounterType.AvgWorkItemWaitTimeBase).Increment();
        }

        public void SampleWorkItemsProcessTime(TimeSpan workItemProcessTime)
        {
            GetCounter(CTPPerformanceCounterType.AvgWorkItemProcessTime).IncrementBy((long)workItemProcessTime.TotalMilliseconds);
            GetCounter(CTPPerformanceCounterType.AvgWorkItemProcessTimeBase).Increment();
        }
    }

    internal class NullCTPInstancePerformanceCounters : ICTPInstancePerformanceCounters, ICTPPerformanceCountersReader
    {
        private static readonly NullCTPInstancePerformanceCounters _instance = new NullCTPInstancePerformanceCounters();

        public static NullCTPInstancePerformanceCounters Instance
        {
            get { return _instance; }
        }

        public void Close() { }
        public void Dispose() { }

        public void SampleThreads(long activeThreads, long inUseThreads) { }
        public void SampleWorkItems(long workItemsQueued, long workItemsProcessed) { }
        public void SampleWorkItemsWaitTime(TimeSpan workItemWaitTime) { }
        public void SampleWorkItemsProcessTime(TimeSpan workItemProcessTime) { }
        public long InUseThreads
        {
            get { return 0; }
        }

        public long ActiveThreads
        {
            get { return 0; }
        }

        public long WorkItemsQueued
        {
            get { return 0; }
        }

        public long WorkItemsProcessed
        {
            get { return 0; }
        }
    }

    internal class LocalCTPInstancePerformanceCounters : ICTPInstancePerformanceCounters, ICTPPerformanceCountersReader
    {
        public void Close() { }
        public void Dispose() { }

        private long _activeThreads;
        private long _inUseThreads;
        private long _workItemsQueued;
        private long _workItemsProcessed;

        public long InUseThreads
        {
            get { return _inUseThreads; }
        }

        public long ActiveThreads
        {
            get { return _activeThreads; }
        }

        public long WorkItemsQueued
        {
            get { return _workItemsQueued; }
        }

        public long WorkItemsProcessed
        {
            get { return _workItemsProcessed; }
        }

        public void SampleThreads(long activeThreads, long inUseThreads)
        {
            _activeThreads = activeThreads;
            _inUseThreads = inUseThreads;
        }

        public void SampleWorkItems(long workItemsQueued, long workItemsProcessed)
        {
            _workItemsQueued = workItemsQueued;
            _workItemsProcessed = workItemsProcessed;
        }

        public void SampleWorkItemsWaitTime(TimeSpan workItemWaitTime)
        {
            // Not supported
        }

        public void SampleWorkItemsProcessTime(TimeSpan workItemProcessTime)
        {
            // Not supported
        }
    }
}
