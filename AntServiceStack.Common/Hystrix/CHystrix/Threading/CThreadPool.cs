namespace CHystrix.Threading
{
    using CHystrix;
    using CHystrix.Utils;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class CThreadPool : ICThreadPool
    {
        private int _finishedCount;
        private ConcurrentQueue<CThread> _idleThreads;
        private volatile int _largestPoolSize;
        private volatile int _MaxConcurrentCount;
        private const int _maxTryPopFailTimes = 3;
        private int _nowRunningCount;
        private ConcurrentDictionary<int, CThread> _threads;
        private CWorkItemCompleteCallback _ThreadWorkCompleteCallback;
        private int _timeoutCount;
        private ConcurrentQueue<ICWorkItem> _waitingTasks;

        public CThreadPool(int maxConcurrentCount)
        {
            this._waitingTasks = new ConcurrentQueue<ICWorkItem>();
            this._threads = new ConcurrentDictionary<int, CThread>();
            this._MaxConcurrentCount = maxConcurrentCount;
            this._idleThreads = new ConcurrentQueue<CThread>();
            this._largestPoolSize = maxConcurrentCount;
        }

        public CThreadPool(int maxConcurrentCount, int workItemTimeoutMiliseconds) : this(maxConcurrentCount)
        {
            this.WorkItemTimeoutMiliseconds = workItemTimeoutMiliseconds;
        }

        public CThreadPool(int maxConcurrentCount, int workItemTimeoutMiliseconds, CWorkItemCompleteCallback workCompleteCallback) : this(maxConcurrentCount, workItemTimeoutMiliseconds)
        {
            this._ThreadWorkCompleteCallback = workCompleteCallback;
        }

        ~CThreadPool()
        {
            this.Reset();
        }

        private void NotifyThreadPoolOfPendingWork()
        {
            if ((this._idleThreads.Count != 0) || (this._nowRunningCount < this.MaxConcurrentCount))
            {
                int num = 0;
                while (((this._waitingTasks.Count > 0) && (this._idleThreads.Count > 0)) && (num < 3))
                {
                    CThread thread;
                    if (!this._idleThreads.TryDequeue(out thread))
                    {
                        Thread.Sleep(10);
                        num++;
                        continue;
                    }
                    if ((this._nowRunningCount + this._idleThreads.Count) > this.MaxConcurrentCount)
                    {
                        thread.Shutdown();
                        continue;
                    }
                    if (!thread.IsShutdown)
                    {
                        ICWorkItem item;
                        bool flag = false;
                        while (this._waitingTasks.TryDequeue(out item))
                        {
                            if (item.CanDo(this.WorkItemTimeoutMiliseconds))
                            {
                                thread.DoWork(item);
                                flag = true;
                                Interlocked.Increment(ref this._nowRunningCount);
                                this._threads.TryAdd(thread.ThreadID, thread);
                                break;
                            }
                            if (item.MarkTimeout())
                            {
                                Interlocked.Increment(ref this._timeoutCount);
                            }
                        }
                        if (!flag)
                        {
                            this._idleThreads.Enqueue(thread);
                            break;
                        }
                    }
                }
                while ((this._waitingTasks.Count > 0) && ((this._nowRunningCount + this._idleThreads.Count) < this.MaxConcurrentCount))
                {
                    ICWorkItem item2;
                    if (this._waitingTasks.TryDequeue(out item2))
                    {
                        if (item2.CanDo(this.WorkItemTimeoutMiliseconds))
                        {
                            CThread thread2 = new CThread(new CThreadWorkCompleteCallback(this.OnWorkComplete));
                            thread2.DoWork(item2);
                            Interlocked.Increment(ref this._nowRunningCount);
                            this._threads.TryAdd(thread2.ThreadID, thread2);
                        }
                        else if (item2.MarkTimeout())
                        {
                            Interlocked.Increment(ref this._timeoutCount);
                        }
                    }
                }
            }
        }

        private void OnWorkComplete(CThread thread, ICWorkItem task)
        {
            CThread thread2;
            Interlocked.Increment(ref this._finishedCount);
            bool flag = false;
            try
            {
                if (this._ThreadWorkCompleteCallback != null)
                {
                    this._ThreadWorkCompleteCallback(task);
                }
            }
            catch (Exception exception)
            {
                CommonUtils.Log.Log(LogLevelEnum.Error, exception.Message, exception);
            }
            if ((this._nowRunningCount + this._idleThreads.Count) > this._MaxConcurrentCount)
            {
                thread.Shutdown();
            }
            if (!thread.IsShutdown)
            {
                ICWorkItem item;
                while (this._waitingTasks.TryDequeue(out item))
                {
                    if (item.CanDo(this.WorkItemTimeoutMiliseconds))
                    {
                        thread.DoWork(item);
                        flag = true;
                        break;
                    }
                    if (item.MarkTimeout())
                    {
                        Interlocked.Increment(ref this._timeoutCount);
                    }
                }
            }
            if (!flag && this._threads.TryRemove(thread.ThreadID, out thread2))
            {
                Interlocked.Decrement(ref this._nowRunningCount);
                this._idleThreads.Enqueue(thread);
            }
        }

        public CWorkItem<T> QueueWorkItem<T>(Func<T> act)
        {
            return this.QueueWorkItem<T>(act, null);
        }

        public CWorkItem<T> QueueWorkItem<T>(Func<T> act, EventHandler<StatusChangeEventArgs> onStatusChange = null)
        {
            CWorkItem<T> item = new CWorkItem<T> {
                StartTime = DateTime.Now,
                Action = act,
                Status = CTaskStatus.WaitingToRun
            };
            item.StatusChange = onStatusChange;
            this._waitingTasks.Enqueue(item);
            this.NotifyThreadPoolOfPendingWork();
            if (this._nowRunningCount > this._largestPoolSize)
            {
                this._largestPoolSize = this._nowRunningCount;
            }
            return item;
        }

        internal void Reset()
        {
            CThread thread2;
            foreach (CThread thread in this._threads.Values)
            {
                thread.Shutdown();
            }
            while (this._idleThreads.TryDequeue(out thread2))
            {
                thread2.Shutdown();
            }
            this._threads.Clear();
            this._waitingTasks = new ConcurrentQueue<ICWorkItem>();
            this._nowRunningCount = 0;
            this._finishedCount = 0;
            this._timeoutCount = 0;
        }

        public static void SetCThreadIdleTimeout(int seconds)
        {
            CThread.ThreadMaxIdleTime = TimeSpan.FromSeconds((double) seconds);
        }

        public void SetTimeoutWorkStatus()
        {
            if (this.WorkItemTimeoutMiliseconds > 0)
            {
                foreach (ICWorkItem item in (from x in (from x in this._threads.Values select x.NowTask).Concat<ICWorkItem>(this._waitingTasks.ToArray())
                    where x != null
                    select x).ToArray<ICWorkItem>())
                {
                    if (!item.CanDo(this.WorkItemTimeoutMiliseconds) && item.MarkTimeout())
                    {
                        Interlocked.Increment(ref this._timeoutCount);
                    }
                }
            }
        }

        public int CurrentPoolSize
        {
            get
            {
                return (this.PoolThreadCount + this.IdleThreadCount);
            }
        }

        public int CurrentTaskCount
        {
            get
            {
                return ((this.NowRunningWorkCount + this.NowWaitingWorkCount) + this.FinishedWorkCount);
            }
        }

        public int FinishedWorkCount
        {
            get
            {
                return this._finishedCount;
            }
        }

        public int IdleThreadCount
        {
            get
            {
                return this._idleThreads.Count<CThread>(x => !x.IsShutdown);
            }
        }

        public int LargestPoolSize
        {
            get
            {
                return this._largestPoolSize;
            }
        }

        public int MaxConcurrentCount
        {
            get
            {
                return this._MaxConcurrentCount;
            }
            set
            {
                if (this._MaxConcurrentCount != value)
                {
                    bool flag = this._MaxConcurrentCount < value;
                    this._MaxConcurrentCount = value;
                    if (flag)
                    {
                        this.NotifyThreadPoolOfPendingWork();
                    }
                }
            }
        }

        public int NowRunningWorkCount
        {
            get
            {
                return this._nowRunningCount;
            }
        }

        public int NowWaitingWorkCount
        {
            get
            {
                return this._waitingTasks.Count;
            }
        }

        public int PoolThreadCount
        {
            get
            {
                return this._threads.Count;
            }
        }

        public CWorkItemCompleteCallback ThreadWorkCompleteCallback
        {
            set
            {
                this._ThreadWorkCompleteCallback = value;
            }
        }

        public int TimeoutWorkCount
        {
            get
            {
                return this._timeoutCount;
            }
        }

        public int WorkItemTimeoutMiliseconds { get; set; }
    }
}

