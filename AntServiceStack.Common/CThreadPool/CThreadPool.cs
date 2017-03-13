#region Release History

// Adapted from Smart Thread Pool
// http://www.codeproject.com/Articles/7933/Smart-Thread-Pool

#endregion

using System;
using System.Security;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using AntServiceStack.Threading.Internal;

namespace AntServiceStack.Threading
{
    #region CThreadPool class
    /// <summary>
    ///  thread pool class.
    /// </summary>
    public partial class CThreadPool : WorkItemsGroupBase, IDisposable
    {
        #region Public Default Constants

        /// <summary>
        /// Default minimum number of threads the thread pool contains. (0)
        /// </summary>
        public const int DefaultMinWorkerThreads = 0;

        /// <summary>
        /// Default maximum number of threads the thread pool contains. (25)
        /// </summary>
        public const int DefaultMaxWorkerThreads = 25;

        /// <summary>
        /// Default idle timeout in milliseconds. (One minute)
        /// </summary>
        public const int DefaultIdleTimeout = 60 * 1000; // One minute

        /// <summary>
        /// Indicate to copy the security context of the caller and then use it in the call. (false)
        /// </summary>
        public const bool DefaultUseCallerCallContext = false;

        /// <summary>
        /// Indicate to copy the HTTP context of the caller and then use it in the call. (false)
        /// </summary>
        public const bool DefaultUseCallerHttpContext = false;

        /// <summary>
        /// Indicate to dispose of the state objects if they support the IDispose interface. (false)
        /// </summary>
        public const bool DefaultDisposeOfStateObjects = false;

        /// <summary>
        /// The default post execute method to run. (None)
        /// When null it means not to call it.
        /// </summary>
        public static readonly PostExecuteWorkItemCallback DefaultPostExecuteWorkItemCallback;

        /// <summary>
        /// The default work item priority (WorkItemPriority.Normal)
        /// </summary>
        public const WorkItemPriority DefaultWorkItemPriority = WorkItemPriority.Normal;

        /// <summary>
        /// The default is to work on work items as soon as they arrive
        /// and not to wait for the start. (false)
        /// </summary>
        public const bool DefaultStartSuspended = false;

        /// <summary>
        /// The default name to use for the performance counters instance. (null)
        /// </summary>
        public static readonly string DefaultPerformanceCounterInstanceName;

        /// <summary>
        /// The default thread priority (ThreadPriority.Normal)
        /// </summary>
        public const ThreadPriority DefaultThreadPriority = ThreadPriority.Normal;

        /// <summary>
        /// The default thread pool name. (CThreadPool)
        /// </summary>
        public const string DefaultThreadPoolName = "CThreadPool";

        /// <summary>
        /// The default Max Stack Size. (CThreadPool)
        /// </summary>
        public static readonly int? DefaultMaxStackSize = null;

        /// <summary>
        /// The default fill state with params. (false)
        /// It is relevant only to QueueWorkItem of Action&lt;...&gt;/Func&lt;...&gt;
        /// </summary>
        public const bool DefaultFillStateWithArgs = false;

        /// <summary>
        /// The default thread backgroundness. (true)
        /// </summary>
        public const bool DefaultAreThreadsBackground = true;

        /// <summary>
        /// The default apartment state of a thread in the thread pool. 
        /// The default is ApartmentState.Unknown which means the CTP will not 
        /// set the apartment of the thread. It will use the .NET default.
        /// </summary>
        public const ApartmentState DefaultApartmentState = ApartmentState.Unknown;

        #endregion

        #region Member Variables

        /// <summary>
        /// Dictionary of all the threads in the thread pool.
        /// </summary>
        private readonly SynchronizedDictionary<Thread, ThreadEntry> _workerThreads = new SynchronizedDictionary<Thread, ThreadEntry>();

        /// <summary>
        /// Queue of work items.
        /// </summary>
        private readonly WorkItemsQueue _workItemsQueue = new WorkItemsQueue();

        /// <summary>
        /// Count the work items handled.
        /// Used by the performance counter.
        /// </summary>
        private int _workItemsProcessed;

        /// <summary>
        /// Number of threads that currently work (not idle).
        /// </summary>
        private int _inUseWorkerThreads;

        /// <summary>
        /// Max in use work threads ever reached
        /// </summary>
        private int _maxInUseWorkThreads;

        /// <summary>
        /// Max active threads ever reached
        /// </summary>
        private int _maxActiveThreads;

        /// <summary>
        /// Stores a copy of the original CTPStartInfo.
        /// It is used to change the MinThread and MaxThreads
        /// </summary>
        private CTPStartInfo _ctpStartInfo;

        /// <summary>
        /// Total number of work items that are stored in the work items queue 
        /// plus the work items that the threads in the pool are working on.
        /// </summary>
        private int _currentWorkItemsCount;

        /// <summary>
        /// Signaled when the thread pool is idle, i.e. no thread is busy
        /// and the work items queue is empty
        /// </summary>
        //private ManualResetEvent _isIdleWaitHandle = new ManualResetEvent(true);
        private ManualResetEvent _isIdleWaitHandle = EventWaitHandleFactory.CreateManualResetEvent(true);

        /// <summary>
        /// An event to signal all the threads to quit immediately.
        /// </summary>
        //private ManualResetEvent _shuttingDownEvent = new ManualResetEvent(false);
        private ManualResetEvent _shuttingDownEvent = EventWaitHandleFactory.CreateManualResetEvent(false);

        /// <summary>
        /// A flag to indicate if the  Thread Pool is now suspended.
        /// </summary>
        private bool _isSuspended;

        /// <summary>
        /// A flag to indicate the threads to quit.
        /// </summary>
        private bool _shutdown;

        /// <summary>
        /// Counts the threads created in the pool.
        /// It is used to name the threads.
        /// </summary>
        private int _threadCounter;

        /// <summary>
        /// Indicate that the CThreadPool has been disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Holds all the WorkItemsGroup instaces that have at least one 
        /// work item int the CThreadPool
        /// This variable is used in case of Shutdown
        /// </summary>
        private readonly SynchronizedDictionary<IWorkItemsGroup, IWorkItemsGroup> _workItemsGroups = new SynchronizedDictionary<IWorkItemsGroup, IWorkItemsGroup>();

        /// <summary>
        /// A common object for all the work items int the CTP
        /// so we can mark them to cancel in O(1)
        /// </summary>
        private CanceledWorkItemsGroup _canceledCThreadPool = new CanceledWorkItemsGroup();

        /// <summary>
        /// Windows CTP performance counters
        /// </summary>
        //private ICTPInstancePerformanceCounters _windowsPCs = NullCTPInstancePerformanceCounters.Instance;

        /// <summary>
        /// Local CTP performance counters
        /// </summary>
        private ICTPInstancePerformanceCounters _localPCs = NullCTPInstancePerformanceCounters.Instance;

        [ThreadStatic]
        private static ThreadEntry _threadEntry;

        /// <summary>
        /// An event to call after a thread is created, but before 
        /// it's first use.
        /// </summary>
        private event ThreadInitializationHandler _onThreadInitialization;

        /// <summary>
        /// An event to call when a thread is about to exit, after 
        /// it is no longer belong to the pool.
        /// </summary>
        private event ThreadTerminationHandler _onThreadTermination;

        #endregion

        #region Per thread properties

        /// <summary>
        /// A reference to the current work item a thread from the thread pool 
        /// is executing.
        /// </summary>
        internal static ThreadEntry CurrentThreadEntry
        {
            get
            {
                return _threadEntry;
            }
            set
            {
                _threadEntry = value;
            }
        }
        #endregion

        #region Construction and Finalization

        /// <summary>
        /// Constructor
        /// </summary>
        public CThreadPool()
        {
            _ctpStartInfo = new CTPStartInfo();
            Initialize();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="idleTimeout">Idle timeout in milliseconds</param>
        public CThreadPool(int idleTimeout)
        {
            _ctpStartInfo = new CTPStartInfo
            {
                IdleTimeout = idleTimeout,
            };
            Initialize();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="idleTimeout">Idle timeout in milliseconds</param>
        /// <param name="maxWorkerThreads">Upper limit of threads in the pool</param>
        public CThreadPool(
            int idleTimeout,
            int maxWorkerThreads)
        {
            _ctpStartInfo = new CTPStartInfo
            {
                IdleTimeout = idleTimeout,
                MaxWorkerThreads = maxWorkerThreads,
            };
            Initialize();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="idleTimeout">Idle timeout in milliseconds</param>
        /// <param name="maxWorkerThreads">Upper limit of threads in the pool</param>
        /// <param name="minWorkerThreads">Lower limit of threads in the pool</param>
        public CThreadPool(
            int idleTimeout,
            int maxWorkerThreads,
            int minWorkerThreads)
        {
            _ctpStartInfo = new CTPStartInfo
            {
                IdleTimeout = idleTimeout,
                MaxWorkerThreads = maxWorkerThreads,
                MinWorkerThreads = minWorkerThreads,
            };
            Initialize();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ctpStartInfo">A CThreadPool configuration that overrides the default behavior</param>
        public CThreadPool(CTPStartInfo ctpStartInfo)
        {
            _ctpStartInfo = new CTPStartInfo(ctpStartInfo);
            Initialize();
        }

        private void Initialize()
        {
            Name = _ctpStartInfo.ThreadPoolName;
            ValidateCTPStartInfo();

            // _ctpStartInfoRW stores a read/write copy of the CTPStartInfo.
            // Actually only MaxWorkerThreads and MinWorkerThreads are overwritten

            _isSuspended = _ctpStartInfo.StartSuspended;

            /*
            if (null != _ctpStartInfo.PerformanceCounterInstanceName)
            {
                try
                {
                    _windowsPCs = new CTPInstancePerformanceCounters(_ctpStartInfo.PerformanceCounterInstanceName);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Unable to create Performance Counters: " + e);
                    _windowsPCs = NullCTPInstancePerformanceCounters.Instance;
                }
            }
            */

            if (_ctpStartInfo.EnableLocalPerformanceCounters)
            {
                _localPCs = new LocalCTPInstancePerformanceCounters();
            }

            // If the CTP is not started suspended then start the threads.
            if (!_isSuspended)
            {
                StartOptimalNumberOfThreads();
            }
        }

        private void StartOptimalNumberOfThreads()
        {
            int threadsCount = Math.Max(_workItemsQueue.Count, _ctpStartInfo.MinWorkerThreads);
            threadsCount = Math.Min(threadsCount, _ctpStartInfo.MaxWorkerThreads);
            threadsCount -= _workerThreads.Count;
            if (threadsCount > 0)
            {
                StartThreads(threadsCount);
            }
        }

        private void ValidateCTPStartInfo()
        {
            if (_ctpStartInfo.MinWorkerThreads < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "MinWorkerThreads", "MinWorkerThreads cannot be negative");
            }

            if (_ctpStartInfo.MaxWorkerThreads <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "MaxWorkerThreads", "MaxWorkerThreads must be greater than zero");
            }

            if (_ctpStartInfo.MinWorkerThreads > _ctpStartInfo.MaxWorkerThreads)
            {
                throw new ArgumentOutOfRangeException(
                    "MinWorkerThreads, maxWorkerThreads",
                    "MaxWorkerThreads must be greater or equal to MinWorkerThreads");
            }
        }

        private static void ValidateCallback(Delegate callback)
        {
            if (callback.GetInvocationList().Length > 1)
            {
                throw new NotSupportedException("CThreadPool doesn't support delegates chains");
            }
        }

        #endregion

        #region Thread Processing

        /// <summary>
        /// Waits on the queue for a work item, shutdown, or timeout.
        /// </summary>
        /// <returns>
        /// Returns the WaitingCallback or null in case of timeout or shutdown.
        /// </returns>
        private WorkItem Dequeue()
        {
            WorkItem workItem =
                _workItemsQueue.DequeueWorkItem(_ctpStartInfo.IdleTimeout, _shuttingDownEvent);

            return workItem;
        }

        /// <summary>
        /// Put a new work item in the queue
        /// </summary>
        /// <param name="workItem">A work item to queue</param>
        internal override void Enqueue(WorkItem workItem)
        {
            // Make sure the workItem is not null
            Debug.Assert(null != workItem);

            IncrementWorkItemsCount();

            workItem.CanceledCThreadPool = _canceledCThreadPool;
            _workItemsQueue.EnqueueWorkItem(workItem);
            workItem.WorkItemIsQueued();

            // If all the threads are busy then try to create a new one
            if (_currentWorkItemsCount > _workerThreads.Count)
            {
                StartThreads(1);
            }
        }

        private void IncrementWorkItemsCount()
        {
            //_windowsPCs.SampleWorkItems(_workItemsQueue.Count, _workItemsProcessed);
            _localPCs.SampleWorkItems(_workItemsQueue.Count, _workItemsProcessed);

            int count = Interlocked.Increment(ref _currentWorkItemsCount);
            //Trace.WriteLine("WorkItemsCount = " + _currentWorkItemsCount.ToString());
            if (count == 1)
            {
                IsIdle = false;
                _isIdleWaitHandle.Reset();
            }
        }

        private void DecrementWorkItemsCount()
        {
            int count = Interlocked.Decrement(ref _currentWorkItemsCount);
            //Trace.WriteLine("WorkItemsCount = " + _currentWorkItemsCount.ToString());
            if (count == 0)
            {
                IsIdle = true;
                _isIdleWaitHandle.Set();
            }

            Interlocked.Increment(ref _workItemsProcessed);

            if (!_shutdown)
            {
                // The counter counts even if the work item was cancelled
                //_windowsPCs.SampleWorkItems(_workItemsQueue.Count, _workItemsProcessed);
                _localPCs.SampleWorkItems(_workItemsQueue.Count, _workItemsProcessed);
            }

        }

        internal void RegisterWorkItemsGroup(IWorkItemsGroup workItemsGroup)
        {
            _workItemsGroups[workItemsGroup] = workItemsGroup;
        }

        internal void UnregisterWorkItemsGroup(IWorkItemsGroup workItemsGroup)
        {
            if (_workItemsGroups.Contains(workItemsGroup))
            {
                _workItemsGroups.Remove(workItemsGroup);
            }
        }

        /// <summary>
        /// Inform that the current thread is about to quit or quiting.
        /// The same thread may call this method more than once.
        /// </summary>
        private void InformCompleted()
        {
            // There is no need to lock the two methods together 
            // since only the current thread removes itself
            // and the _workerThreads is a synchronized dictionary
            if (_workerThreads.Contains(Thread.CurrentThread))
            {
                _workerThreads.Remove(Thread.CurrentThread);
                //_windowsPCs.SampleThreads(_workerThreads.Count, _inUseWorkerThreads);
                _localPCs.SampleThreads(_workerThreads.Count, _inUseWorkerThreads);
            }
        }

        /// <summary>
        /// Starts new threads
        /// </summary>
        /// <param name="threadsCount">The number of threads to start</param>
        private void StartThreads(int threadsCount)
        {
            if (_isSuspended)
            {
                return;
            }

            lock (_workerThreads.SyncRoot)
            {
                // Don't start threads on shut down
                if (_shutdown)
                {
                    return;
                }

                for (int i = 0; i < threadsCount; ++i)
                {
                    // Don't create more threads then the upper limit
                    if (_workerThreads.Count >= _ctpStartInfo.MaxWorkerThreads)
                    {
                        return;
                    }

                    // Create a new thread

                    Thread workerThread =
                        _ctpStartInfo.MaxStackSize.HasValue
                        ? new Thread(ProcessQueuedItems, _ctpStartInfo.MaxStackSize.Value)
                        : new Thread(ProcessQueuedItems);
                    // Configure the new thread and start it
                    workerThread.Name = "CTP " + Name + " Thread #" + _threadCounter;
                    workerThread.IsBackground = _ctpStartInfo.AreThreadsBackground;

                    if (_ctpStartInfo.ApartmentState != ApartmentState.Unknown)
                    {
                        workerThread.SetApartmentState(_ctpStartInfo.ApartmentState);
                    }

                    workerThread.Priority = _ctpStartInfo.ThreadPriority;
                    workerThread.Start();
                    ++_threadCounter;

                    // Add it to the dictionary and update its creation time.
                    _workerThreads[workerThread] = new ThreadEntry(this);

                    //_windowsPCs.SampleThreads(_workerThreads.Count, _inUseWorkerThreads);
                    _localPCs.SampleThreads(_workerThreads.Count, _inUseWorkerThreads);
                    if (_workerThreads.Count > _maxActiveThreads)
                    {
                        _maxActiveThreads = _workerThreads.Count;
                    }
                }
            }
        }

        /// <summary>
        /// A worker thread method that processes work items from the work items queue.
        /// </summary>
        private void ProcessQueuedItems()
        {
            // Keep the entry of the dictionary as thread's variable to avoid the synchronization locks
            // of the dictionary.
            CurrentThreadEntry = _workerThreads[Thread.CurrentThread];

            FireOnThreadInitialization();

            try
            {
                bool bInUseWorkerThreadsWasIncremented = false;

                // Process until shutdown.
                while (!_shutdown)
                {
                    // Update the last time this thread was seen alive.
                    // It's good for debugging.
                    CurrentThreadEntry.IAmAlive();

                    // The following block handles the when the MaxWorkerThreads has been
                    // incremented by the user at run-time.
                    // Double lock for quit.
                    if (_workerThreads.Count > _ctpStartInfo.MaxWorkerThreads)
                    {
                        lock (_workerThreads.SyncRoot)
                        {
                            if (_workerThreads.Count > _ctpStartInfo.MaxWorkerThreads)
                            {
                                // Inform that the thread is quiting and then quit.
                                // This method must be called within this lock or else
                                // more threads will quit and the thread pool will go
                                // below the lower limit.
                                InformCompleted();
                                break;
                            }
                        }
                    }

                    // Wait for a work item, shutdown, or timeout
                    WorkItem workItem = Dequeue();

                    // Update the last time this thread was seen alive.
                    // It's good for debugging.
                    CurrentThreadEntry.IAmAlive();

                    // On timeout or shut down.
                    if (null == workItem)
                    {
                        // Double lock for quit.
                        if (_workerThreads.Count > _ctpStartInfo.MinWorkerThreads)
                        {
                            lock (_workerThreads.SyncRoot)
                            {
                                if (_workerThreads.Count > _ctpStartInfo.MinWorkerThreads)
                                {
                                    // Inform that the thread is quiting and then quit.
                                    // This method must be called within this lock or else
                                    // more threads will quit and the thread pool will go
                                    // below the lower limit.
                                    InformCompleted();
                                    break;
                                }
                            }
                        }
                    }

                    // If we didn't quit then skip to the next iteration.
                    if (null == workItem)
                    {
                        continue;
                    }

                    try
                    {
                        // Initialize the value to false
                        bInUseWorkerThreadsWasIncremented = false;

                        // Set the Current Work Item of the thread.
                        // Store the Current Work Item  before the workItem.StartingWorkItem() is called, 
                        // so WorkItem.Cancel can work when the work item is between InQueue and InProgress 
                        // states.
                        // If the work item has been cancelled BEFORE the workItem.StartingWorkItem() 
                        // (work item is in InQueue state) then workItem.StartingWorkItem() will return false.
                        // If the work item has been cancelled AFTER the workItem.StartingWorkItem() then
                        // (work item is in InProgress state) then the thread will be aborted
                        CurrentThreadEntry.CurrentWorkItem = workItem;

                        // Change the state of the work item to 'in progress' if possible.
                        // We do it here so if the work item has been canceled we won't 
                        // increment the _inUseWorkerThreads.
                        // The cancel mechanism doesn't delete items from the queue,  
                        // it marks the work item as canceled, and when the work item
                        // is dequeued, we just skip it.
                        if (!workItem.StartingWorkItem())
                        {
                            continue;
                        }

                        // Execute the callback.  Make sure to accurately
                        // record how many callbacks are currently executing.
                        int inUseWorkerThreads = Interlocked.Increment(ref _inUseWorkerThreads);
                        //_windowsPCs.SampleThreads(_workerThreads.Count, inUseWorkerThreads);
                        _localPCs.SampleThreads(_workerThreads.Count, inUseWorkerThreads);
                        if (inUseWorkerThreads > _maxInUseWorkThreads)
                        {
                            Interlocked.CompareExchange(ref _maxInUseWorkThreads, inUseWorkerThreads, _maxInUseWorkThreads);
                        }

                        // Mark that the _inUseWorkerThreads incremented, so in the finally{}
                        // statement we will decrement it correctly.
                        bInUseWorkerThreadsWasIncremented = true;

                        workItem.FireWorkItemStarted();

                        ExecuteWorkItem(workItem);
                    }
                    catch (Exception ex)
                    {
                        ex.GetHashCode();
                        // Do nothing
                    }
                    finally
                    {
                        workItem.DisposeOfState();

                        // Set the CurrentWorkItem to null, since we 
                        // no longer run user's code.
                        CurrentThreadEntry.CurrentWorkItem = null;

                        // Decrement the _inUseWorkerThreads only if we had 
                        // incremented it. Note the cancelled work items don't
                        // increment _inUseWorkerThreads.
                        if (bInUseWorkerThreadsWasIncremented)
                        {
                            int inUseWorkerThreads = Interlocked.Decrement(ref _inUseWorkerThreads);
                            //_windowsPCs.SampleThreads(_workerThreads.Count, inUseWorkerThreads);
                            _localPCs.SampleThreads(_workerThreads.Count, inUseWorkerThreads);
                        }

                        // Run Post Callback
                        workItem.PostExecute();

                        // Notify that the work item has been completed.
                        // WorkItemsGroup may enqueue their next work item.
                        workItem.FireWorkItemCompleted();

                        // Decrement the number of work items here so the idle 
                        // ManualResetEvent won't fluctuate.
                        DecrementWorkItemsCount();
                    }
                }
            }
            catch (ThreadAbortException tae)
            {
                tae.GetHashCode();
                // Handle the abort exception gracfully.
                Thread.ResetAbort();
            }
            catch (Exception e)
            {
                Debug.Assert(null != e);
            }
            finally
            {
                InformCompleted();
                FireOnThreadTermination();
            }
        }

        private void ExecuteWorkItem(WorkItem workItem)
        {
            //_windowsPCs.SampleWorkItemsWaitTime(workItem.WaitingTime);
            _localPCs.SampleWorkItemsWaitTime(workItem.WaitingTime);
            try
            {
                workItem.Execute();
            }
            finally
            {
                //_windowsPCs.SampleWorkItemsProcessTime(workItem.ProcessTime);
                _localPCs.SampleWorkItemsProcessTime(workItem.ProcessTime);
            }
        }


        #endregion

        #region Public Methods

        private void ValidateWaitForIdle()
        {
            if (null != CurrentThreadEntry && CurrentThreadEntry.AssociatedCThreadPool == this)
            {
                throw new NotSupportedException(
                    "WaitForIdle cannot be called from a thread on its CThreadPool, it causes a deadlock");
            }
        }

        internal static void ValidateWorkItemsGroupWaitForIdle(IWorkItemsGroup workItemsGroup)
        {
            if (null == CurrentThreadEntry)
            {
                return;
            }

            WorkItem workItem = CurrentThreadEntry.CurrentWorkItem;
            ValidateWorkItemsGroupWaitForIdleImpl(workItemsGroup, workItem);
            if ((null != workItemsGroup) &&
                (null != workItem) &&
                CurrentThreadEntry.CurrentWorkItem.WasQueuedBy(workItemsGroup))
            {
                throw new NotSupportedException("WaitForIdle cannot be called from a thread on its CThreadPool, it causes a deadlock");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ValidateWorkItemsGroupWaitForIdleImpl(IWorkItemsGroup workItemsGroup, WorkItem workItem)
        {
            if ((null != workItemsGroup) &&
                (null != workItem) &&
                workItem.WasQueuedBy(workItemsGroup))
            {
                throw new NotSupportedException("WaitForIdle cannot be called from a thread on its CThreadPool, it causes a deadlock");
            }
        }

        /// <summary>
        /// Force the CThreadPool to shutdown
        /// </summary>
        public void Shutdown()
        {
            Shutdown(true, 0);
        }

        /// <summary>
        /// Force the CThreadPool to shutdown with timeout
        /// </summary>
        public void Shutdown(bool forceAbort, TimeSpan timeout)
        {
            Shutdown(forceAbort, (int)timeout.TotalMilliseconds);
        }

        /// <summary>
        /// Empties the queue of work items and abort the threads in the pool.
        /// </summary>
        public void Shutdown(bool forceAbort, int millisecondsTimeout)
        {
            ValidateNotDisposed();

            
            /*
            ICTPInstancePerformanceCounters pcs = _windowsPCs;

            if (NullCTPInstancePerformanceCounters.Instance != _windowsPCs)
            {
                // Set the _pcs to "null" to stop updating the performance
                // counters
                _windowsPCs = NullCTPInstancePerformanceCounters.Instance;

                pcs.Dispose();
            }
            */

            Thread[] threads;
            lock (_workerThreads.SyncRoot)
            {
                // Shutdown the work items queue
                _workItemsQueue.Dispose();

                // Signal the threads to exit
                _shutdown = true;
                _shuttingDownEvent.Set();

                // Make a copy of the threads' references in the pool
                threads = new Thread[_workerThreads.Count];
                _workerThreads.Keys.CopyTo(threads, 0);
            }

            int millisecondsLeft = millisecondsTimeout;
            Stopwatch stopwatch = Stopwatch.StartNew();
            //DateTime start = DateTime.UtcNow;
            bool waitInfinitely = (Timeout.Infinite == millisecondsTimeout);
            bool timeout = false;

            // Each iteration we update the time left for the timeout.
            foreach (Thread thread in threads)
            {
                // Join don't work with negative numbers
                if (!waitInfinitely && (millisecondsLeft < 0))
                {
                    timeout = true;
                    break;
                }

                // Wait for the thread to terminate
                bool success = thread.Join(millisecondsLeft);
                if (!success)
                {
                    timeout = true;
                    break;
                }

                if (!waitInfinitely)
                {
                    // Update the time left to wait
                    //TimeSpan ts = DateTime.UtcNow - start;
                    millisecondsLeft = millisecondsTimeout - (int)stopwatch.ElapsedMilliseconds;
                }
            }

            if (timeout && forceAbort)
            {
                // Abort the threads in the pool
                foreach (Thread thread in threads)
                {

                    if ((thread != null) && thread.IsAlive)
                    {
                        try
                        {
                            thread.Abort(); // Shutdown
                        }
                        catch (SecurityException e)
                        {
                            e.GetHashCode();
                        }
                        catch (ThreadStateException ex)
                        {
                            ex.GetHashCode();
                            // In case the thread has been terminated 
                            // after the check if it is alive.
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Wait for all work items to complete
        /// </summary>
        /// <param name="waitableResults">Array of work item result objects</param>
        /// <returns>
        /// true when every work item in workItemResults has completed; otherwise false.
        /// </returns>
        public static bool WaitAll(
            IWaitableResult[] waitableResults)
        {
            return WaitAll(waitableResults, Timeout.Infinite, true);
        }

        /// <summary>
        /// Wait for all work items to complete
        /// </summary>
        /// <param name="waitableResults">Array of work item result objects</param>
        /// <param name="timeout">The number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely. </param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <returns>
        /// true when every work item in workItemResults has completed; otherwise false.
        /// </returns>
        public static bool WaitAll(
            IWaitableResult[] waitableResults,
            TimeSpan timeout,
            bool exitContext)
        {
            return WaitAll(waitableResults, (int)timeout.TotalMilliseconds, exitContext);
        }

        /// <summary>
        /// Wait for all work items to complete
        /// </summary>
        /// <param name="waitableResults">Array of work item result objects</param>
        /// <param name="timeout">The number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely. </param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <param name="cancelWaitHandle">A cancel wait handle to interrupt the wait if needed</param>
        /// <returns>
        /// true when every work item in workItemResults has completed; otherwise false.
        /// </returns>
        public static bool WaitAll(
            IWaitableResult[] waitableResults,
            TimeSpan timeout,
            bool exitContext,
            WaitHandle cancelWaitHandle)
        {
            return WaitAll(waitableResults, (int)timeout.TotalMilliseconds, exitContext, cancelWaitHandle);
        }

        /// <summary>
        /// Wait for all work items to complete
        /// </summary>
        /// <param name="waitableResults">Array of work item result objects</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <returns>
        /// true when every work item in workItemResults has completed; otherwise false.
        /// </returns>
        public static bool WaitAll(
            IWaitableResult[] waitableResults,
            int millisecondsTimeout,
            bool exitContext)
        {
            return WorkItem.WaitAll(waitableResults, millisecondsTimeout, exitContext, null);
        }

        /// <summary>
        /// Wait for all work items to complete
        /// </summary>
        /// <param name="waitableResults">Array of work item result objects</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <param name="cancelWaitHandle">A cancel wait handle to interrupt the wait if needed</param>
        /// <returns>
        /// true when every work item in workItemResults has completed; otherwise false.
        /// </returns>
        public static bool WaitAll(
            IWaitableResult[] waitableResults,
            int millisecondsTimeout,
            bool exitContext,
            WaitHandle cancelWaitHandle)
        {
            return WorkItem.WaitAll(waitableResults, millisecondsTimeout, exitContext, cancelWaitHandle);
        }


        /// <summary>
        /// Waits for any of the work items in the specified array to complete, cancel, or timeout
        /// </summary>
        /// <param name="waitableResults">Array of work item result objects</param>
        /// <returns>
        /// The array index of the work item result that satisfied the wait, or WaitTimeout if any of the work items has been canceled.
        /// </returns>
        public static int WaitAny(
            IWaitableResult[] waitableResults)
        {
            return WaitAny(waitableResults, Timeout.Infinite, true);
        }

        /// <summary>
        /// Waits for any of the work items in the specified array to complete, cancel, or timeout
        /// </summary>
        /// <param name="waitableResults">Array of work item result objects</param>
        /// <param name="timeout">The number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely. </param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <returns>
        /// The array index of the work item result that satisfied the wait, or WaitTimeout if no work item result satisfied the wait and a time interval equivalent to millisecondsTimeout has passed or the work item has been canceled.
        /// </returns>
        public static int WaitAny(
            IWaitableResult[] waitableResults,
            TimeSpan timeout,
            bool exitContext)
        {
            return WaitAny(waitableResults, (int)timeout.TotalMilliseconds, exitContext);
        }

        /// <summary>
        /// Waits for any of the work items in the specified array to complete, cancel, or timeout
        /// </summary>
        /// <param name="waitableResults">Array of work item result objects</param>
        /// <param name="timeout">The number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely. </param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <param name="cancelWaitHandle">A cancel wait handle to interrupt the wait if needed</param>
        /// <returns>
        /// The array index of the work item result that satisfied the wait, or WaitTimeout if no work item result satisfied the wait and a time interval equivalent to millisecondsTimeout has passed or the work item has been canceled.
        /// </returns>
        public static int WaitAny(
            IWaitableResult[] waitableResults,
            TimeSpan timeout,
            bool exitContext,
            WaitHandle cancelWaitHandle)
        {
            return WaitAny(waitableResults, (int)timeout.TotalMilliseconds, exitContext, cancelWaitHandle);
        }

        /// <summary>
        /// Waits for any of the work items in the specified array to complete, cancel, or timeout
        /// </summary>
        /// <param name="waitableResults">Array of work item result objects</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <returns>
        /// The array index of the work item result that satisfied the wait, or WaitTimeout if no work item result satisfied the wait and a time interval equivalent to millisecondsTimeout has passed or the work item has been canceled.
        /// </returns>
        public static int WaitAny(
            IWaitableResult[] waitableResults,
            int millisecondsTimeout,
            bool exitContext)
        {
            return WorkItem.WaitAny(waitableResults, millisecondsTimeout, exitContext, null);
        }

        /// <summary>
        /// Waits for any of the work items in the specified array to complete, cancel, or timeout
        /// </summary>
        /// <param name="waitableResults">Array of work item result objects</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <param name="cancelWaitHandle">A cancel wait handle to interrupt the wait if needed</param>
        /// <returns>
        /// The array index of the work item result that satisfied the wait, or WaitTimeout if no work item result satisfied the wait and a time interval equivalent to millisecondsTimeout has passed or the work item has been canceled.
        /// </returns>
        public static int WaitAny(
            IWaitableResult[] waitableResults,
            int millisecondsTimeout,
            bool exitContext,
            WaitHandle cancelWaitHandle)
        {
            return WorkItem.WaitAny(waitableResults, millisecondsTimeout, exitContext, cancelWaitHandle);
        }

        /// <summary>
        /// Creates a new WorkItemsGroup.
        /// </summary>
        /// <param name="concurrency">The number of work items that can be run concurrently</param>
        /// <returns>A reference to the WorkItemsGroup</returns>
        public IWorkItemsGroup CreateWorkItemsGroup(int concurrency)
        {
            IWorkItemsGroup workItemsGroup = new WorkItemsGroup(this, concurrency, _ctpStartInfo);
            return workItemsGroup;
        }

        /// <summary>
        /// Creates a new WorkItemsGroup.
        /// </summary>
        /// <param name="concurrency">The number of work items that can be run concurrently</param>
        /// <param name="wigStartInfo">A WorkItemsGroup configuration that overrides the default behavior</param>
        /// <returns>A reference to the WorkItemsGroup</returns>
        public IWorkItemsGroup CreateWorkItemsGroup(int concurrency, WIGStartInfo wigStartInfo)
        {
            IWorkItemsGroup workItemsGroup = new WorkItemsGroup(this, concurrency, wigStartInfo);
            return workItemsGroup;
        }

        #region Fire Thread's Events

        private void FireOnThreadInitialization()
        {
            if (null != _onThreadInitialization)
            {
                foreach (ThreadInitializationHandler tih in _onThreadInitialization.GetInvocationList())
                {
                    try
                    {
                        tih();
                    }
                    catch (Exception e)
                    {
                        e.GetHashCode();
                        Debug.Assert(false);
                        throw;
                    }
                }
            }
        }

        private void FireOnThreadTermination()
        {
            if (null != _onThreadTermination)
            {
                foreach (ThreadTerminationHandler tth in _onThreadTermination.GetInvocationList())
                {
                    try
                    {
                        tth();
                    }
                    catch (Exception e)
                    {
                        e.GetHashCode();
                        Debug.Assert(false);
                        throw;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// This event is fired when a thread is created.
        /// Use it to initialize a thread before the work items use it.
        /// </summary>
        public event ThreadInitializationHandler OnThreadInitialization
        {
            add { _onThreadInitialization += value; }
            remove { _onThreadInitialization -= value; }
        }

        /// <summary>
        /// This event is fired when a thread is terminating.
        /// Use it for cleanup.
        /// </summary>
        public event ThreadTerminationHandler OnThreadTermination
        {
            add { _onThreadTermination += value; }
            remove { _onThreadTermination -= value; }
        }


        internal void CancelAbortWorkItemsGroup(WorkItemsGroup wig)
        {
            foreach (ThreadEntry threadEntry in _workerThreads.Values)
            {
                WorkItem workItem = threadEntry.CurrentWorkItem;
                if (null != workItem &&
                    workItem.WasQueuedBy(wig) &&
                    !workItem.IsCanceled)
                {
                    threadEntry.CurrentWorkItem.GetWorkItemResult().Cancel(true);
                }
            }
        }



        #endregion

        #region Properties

        /// <summary>
        /// Get/Set the lower limit of threads in the pool.
        /// </summary>
        public int MinThreads
        {
            get
            {
                ValidateNotDisposed();
                return _ctpStartInfo.MinWorkerThreads;
            }
            set
            {
                Debug.Assert(value >= 0);
                Debug.Assert(value <= _ctpStartInfo.MaxWorkerThreads);
                if (_ctpStartInfo.MaxWorkerThreads < value)
                {
                    _ctpStartInfo.MaxWorkerThreads = value;
                }
                _ctpStartInfo.MinWorkerThreads = value;
                StartOptimalNumberOfThreads();
            }
        }

        /// <summary>
        /// Get/Set the upper limit of threads in the pool.
        /// </summary>
        public int MaxThreads
        {
            get
            {
                ValidateNotDisposed();
                return _ctpStartInfo.MaxWorkerThreads;
            }

            set
            {
                Debug.Assert(value > 0);
                Debug.Assert(value >= _ctpStartInfo.MinWorkerThreads);
                if (_ctpStartInfo.MinWorkerThreads > value)
                {
                    _ctpStartInfo.MinWorkerThreads = value;
                }
                _ctpStartInfo.MaxWorkerThreads = value;
                StartOptimalNumberOfThreads();
            }
        }
        /// <summary>
        /// Get the number of threads in the thread pool.
        /// Should be between the lower and the upper limits.
        /// </summary>
        public int ActiveThreads
        {
            get
            {
                ValidateNotDisposed();
                return _workerThreads.Count;
            }
        }

        /// <summary>
        /// Max number of active threads ever reached.
        /// </summary>
        public int MaxActiveThreads
        {
            get
            {
                return _maxActiveThreads;
            }
        }

        /// <summary>
        /// Get the number of busy (not idle) threads in the thread pool.
        /// </summary>
        public int InUseThreads
        {
            get
            {
                ValidateNotDisposed();
                return _inUseWorkerThreads;
            }
        }

        /// <summary>
        /// Max number of in use threads ever reached.
        /// </summary>
        public int MaxInUseThreads
        {
            get
            {
                return _maxInUseWorkThreads;
            }
        }

        /// <summary>
        /// Returns true if the current running work item has been cancelled.
        /// Must be used within the work item's callback method.
        /// The work item should sample this value in order to know if it
        /// needs to quit before its completion.
        /// </summary>
        public static bool IsWorkItemCanceled
        {
            get
            {
                return CurrentThreadEntry.CurrentWorkItem.IsCanceled;
            }
        }

        /// <summary>
        /// Checks if the work item has been cancelled, and if yes then abort the thread.
        /// Can be used with Cancel and timeout
        /// </summary>
        public static void AbortOnWorkItemCancel()
        {
            if (IsWorkItemCanceled)
            {
                Thread.CurrentThread.Abort();
            }
        }

        /// <summary>
        /// Thread Pool start information (readonly)
        /// </summary>
        public CTPStartInfo CTPStartInfo
        {
            get
            {
                return _ctpStartInfo.AsReadOnly();
            }
        }

        public bool IsShuttingdown
        {
            get { return _shutdown; }
        }

        /// <summary>
        /// Return the local calculated performance counters
        /// Available only if CTPStartInfo.EnableLocalPerformanceCounters is true.
        /// </summary>
        public ICTPPerformanceCountersReader PerformanceCountersReader
        {
            get { return (ICTPPerformanceCountersReader)_localPCs; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (!_isDisposed)
            {
                if (!_shutdown)
                {
                    Shutdown();
                }

                if (null != _shuttingDownEvent)
                {
                    _shuttingDownEvent.Close();
                    _shuttingDownEvent = null;
                }
                _workerThreads.Clear();

                if (null != _isIdleWaitHandle)
                {
                    _isIdleWaitHandle.Close();
                    _isIdleWaitHandle = null;
                }

                _isDisposed = true;
            }
        }

        private void ValidateNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().ToString(), "The CThreadPool has been shutdown");
            }
        }
        #endregion

        #region WorkItemsGroupBase Overrides

        /// <summary>
        /// Get/Set the maximum number of work items that execute cocurrency on the thread pool
        /// </summary>
        public override int Concurrency
        {
            get { return MaxThreads; }
            set { MaxThreads = value; }
        }

        /// <summary>
        /// Get the number of work items in the queue.
        /// </summary>
        public override int WaitingCallbacks
        {
            get
            {
                ValidateNotDisposed();
                return _workItemsQueue.Count;
            }
        }

        /// <summary>
        /// Get an array with all the state objects of the currently running items.
        /// The array represents a snap shot and impact performance.
        /// </summary>
        public override object[] GetStates()
        {
            object[] states = _workItemsQueue.GetStates();
            return states;
        }

        /// <summary>
        /// WorkItemsGroup start information (readonly)
        /// </summary>
        public override WIGStartInfo WIGStartInfo
        {
            get { return _ctpStartInfo.AsReadOnly(); }
        }

        /// <summary>
        /// Start the thread pool if it was started suspended.
        /// If it is already running, this method is ignored.
        /// </summary>
        public override void Start()
        {
            if (!_isSuspended)
            {
                return;
            }
            _isSuspended = false;

            ICollection workItemsGroups = _workItemsGroups.Values;
            foreach (WorkItemsGroup workItemsGroup in workItemsGroups)
            {
                workItemsGroup.OnCTPIsStarting();
            }

            StartOptimalNumberOfThreads();
        }

        /// <summary>
        /// Cancel all work items using thread abortion
        /// </summary>
        /// <param name="abortExecution">True to stop work items by raising ThreadAbortException</param>
        public override void Cancel(bool abortExecution)
        {
            _canceledCThreadPool.IsCanceled = true;
            _canceledCThreadPool = new CanceledWorkItemsGroup();

            ICollection workItemsGroups = _workItemsGroups.Values;
            foreach (WorkItemsGroup workItemsGroup in workItemsGroups)
            {
                workItemsGroup.Cancel(abortExecution);
            }

            if (abortExecution)
            {
                foreach (ThreadEntry threadEntry in _workerThreads.Values)
                {
                    WorkItem workItem = threadEntry.CurrentWorkItem;
                    if (null != workItem &&
                        threadEntry.AssociatedCThreadPool == this &&
                        !workItem.IsCanceled)
                    {
                        threadEntry.CurrentWorkItem.GetWorkItemResult().Cancel(true);
                    }
                }
            }
        }

        /// <summary>
        /// Wait for the thread pool to be idle
        /// </summary>
        public override bool WaitForIdle(int millisecondsTimeout)
        {
            ValidateWaitForIdle();
            return CTPEventWaitHandle.WaitOne(_isIdleWaitHandle, millisecondsTimeout, false);
        }

        /// <summary>
        /// This event is fired when all work items are completed.
        /// (When IsIdle changes to true)
        /// This event only work on WorkItemsGroup. On CThreadPool
        /// it throws the NotImplementedException.
        /// </summary>
        public override event WorkItemsGroupIdleHandler OnIdle
        {
            add
            {
                throw new NotImplementedException("This event is not implemented in the CThreadPool class. Please create a WorkItemsGroup in order to use this feature.");
                //_onIdle += value;
            }
            remove
            {
                throw new NotImplementedException("This event is not implemented in the CThreadPool class. Please create a WorkItemsGroup in order to use this feature.");
                //_onIdle -= value;
            }
        }

        internal override void PreQueueWorkItem()
        {
            ValidateNotDisposed();
        }

        #endregion

        #region Join, Choice, Pipe, etc.

        /// <summary>
        /// Executes all actions in parallel.
        /// Returns when they all finish.
        /// </summary>
        /// <param name="actions">Actions to execute</param>
        public void Join(IEnumerable<Action> actions)
        {
            WIGStartInfo wigStartInfo = new WIGStartInfo { StartSuspended = true };
            IWorkItemsGroup workItemsGroup = CreateWorkItemsGroup(int.MaxValue, wigStartInfo);
            foreach (Action action in actions)
            {
                workItemsGroup.QueueWorkItem(action);
            }
            workItemsGroup.Start();
            workItemsGroup.WaitForIdle();
        }

        /// <summary>
        /// Executes all actions in parallel.
        /// Returns when they all finish.
        /// </summary>
        /// <param name="actions">Actions to execute</param>
        public void Join(params Action[] actions)
        {
            Join((IEnumerable<Action>)actions);
        }

        private class ChoiceIndex
        {
            public int _index = -1;
        }

        /// <summary>
        /// Executes all actions in parallel
        /// Returns when the first one completes
        /// </summary>
        /// <param name="actions">Actions to execute</param>
        public int Choice(IEnumerable<Action> actions)
        {
            WIGStartInfo wigStartInfo = new WIGStartInfo { StartSuspended = true };
            IWorkItemsGroup workItemsGroup = CreateWorkItemsGroup(int.MaxValue, wigStartInfo);

            ManualResetEvent anActionCompleted = new ManualResetEvent(false);

            ChoiceIndex choiceIndex = new ChoiceIndex();

            int i = 0;
            foreach (Action action in actions)
            {
                Action act = action;
                int value = i;
                workItemsGroup.QueueWorkItem(() => { act(); Interlocked.CompareExchange(ref choiceIndex._index, value, -1); anActionCompleted.Set(); });
                ++i;
            }
            workItemsGroup.Start();
            anActionCompleted.WaitOne();

            return choiceIndex._index;
        }

        /// <summary>
        /// Executes all actions in parallel
        /// Returns when the first one completes
        /// </summary>
        /// <param name="actions">Actions to execute</param>
        public int Choice(params Action[] actions)
        {
            return Choice((IEnumerable<Action>)actions);
        }

        /// <summary>
        /// Executes actions in sequence asynchronously.
        /// Returns immediately.
        /// </summary>
        /// <param name="pipeState">A state context that passes </param>
        /// <param name="actions">Actions to execute in the order they should run</param>
        public void Pipe<T>(T pipeState, IEnumerable<Action<T>> actions)
        {
            WIGStartInfo wigStartInfo = new WIGStartInfo { StartSuspended = true };
            IWorkItemsGroup workItemsGroup = CreateWorkItemsGroup(1, wigStartInfo);
            foreach (Action<T> action in actions)
            {
                Action<T> act = action;
                workItemsGroup.QueueWorkItem(() => act(pipeState));
            }
            workItemsGroup.Start();
            workItemsGroup.WaitForIdle();
        }

        /// <summary>
        /// Executes actions in sequence asynchronously.
        /// Returns immediately.
        /// </summary>
        /// <param name="pipeState"></param>
        /// <param name="actions">Actions to execute in the order they should run</param>
        public void Pipe<T>(T pipeState, params Action<T>[] actions)
        {
            Pipe(pipeState, (IEnumerable<Action<T>>)actions);
        }
        #endregion
    }
    #endregion
}
