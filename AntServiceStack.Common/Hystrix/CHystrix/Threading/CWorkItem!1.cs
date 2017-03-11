namespace CHystrix.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class CWorkItem<T> : ICWorkItem
    {
        private int _exeMilliseconds;
        private int _isMarkTimeoutStatus;
        private int _realExeMilliseconds;
        private T _result;
        private CTaskStatus _Status;
        internal EventHandler<StatusChangeEventArgs> StatusChange;

        public bool CanDo(int timeout)
        {
            if (this.IsCompleted || this.IsCanceled)
            {
                return false;
            }
            return ((timeout <= 0) || (timeout > this.ExeuteMilliseconds));
        }

        public void Do()
        {
            this.WorkerThreadID = Thread.CurrentThread.ManagedThreadId;
            this.RealStartTime = DateTime.Now;
            this.Status = CTaskStatus.Running;
            CTaskStatus ranToCompletion = this.Status;
            try
            {
                this._result = this.Action();
                ranToCompletion = CTaskStatus.RanToCompletion;
            }
            catch (System.Exception exception)
            {
                ranToCompletion = CTaskStatus.Faulted;
                this.IsFaulted = true;
                this.Exception = exception;
            }
            finally
            {
                this.IsCompleted = true;
                this.EndTime = DateTime.Now;
                this.Status = ranToCompletion;
            }
        }

        public bool MarkTimeout()
        {
            if (Interlocked.CompareExchange(ref this._isMarkTimeoutStatus, 1, 0) == 1)
            {
                return false;
            }
            this.Status = CTaskStatus.Canceled;
            this.IsCanceled = true;
            this.EndTime = DateTime.Now;
            return true;
        }

        public bool Wait(int waitMilliseconds = -1)
        {
            int num = 0;
            int millisecondsTimeout = 100;
            try
            {
                while ((num < waitMilliseconds) || (waitMilliseconds == -1))
                {
                    if (this.IsCompleted || this.IsCanceled)
                    {
                        return true;
                    }
                    Thread.Sleep(millisecondsTimeout);
                    if (millisecondsTimeout < 0x3e8)
                    {
                        millisecondsTimeout += 100;
                    }
                    num += millisecondsTimeout;
                }
            }
            catch (System.Exception exception)
            {
                this.IsCompleted = true;
                this.Status = CTaskStatus.Faulted;
                this.IsFaulted = true;
                this.Exception = exception;
                return false;
            }
            this.Status = CTaskStatus.Canceled;
            this.IsCanceled = true;
            this.EndTime = DateTime.Now;
            return false;
        }

        public Func<T> Action { get; set; }

        internal DateTime EndTime { get; set; }

        public System.Exception Exception { get; internal set; }

        public int ExeuteMilliseconds
        {
            get
            {
                if (this.IsCompleted || this.IsCanceled)
                {
                    if (this._exeMilliseconds == 0)
                    {
                        TimeSpan span = (TimeSpan) (this.EndTime - this.StartTime);
                        this._exeMilliseconds = (int) span.TotalMilliseconds;
                    }
                    return this._exeMilliseconds;
                }
                TimeSpan span2 = (TimeSpan) (DateTime.Now - this.StartTime);
                return (int) span2.TotalMilliseconds;
            }
        }

        public bool IsCanceled { get; private set; }

        public bool IsCompleted { get; private set; }

        public bool IsFaulted { get; private set; }

        public int RealExecuteMilliseconds
        {
            get
            {
                if (this.IsCompleted || this.IsCanceled)
                {
                    if (this._realExeMilliseconds == 0)
                    {
                        TimeSpan span = (TimeSpan) (this.EndTime - this.RealStartTime);
                        this._realExeMilliseconds = (int) span.TotalMilliseconds;
                    }
                    return this._realExeMilliseconds;
                }
                TimeSpan span2 = (TimeSpan) (DateTime.Now - this.StartTime);
                return (int) span2.TotalMilliseconds;
            }
        }

        internal DateTime RealStartTime { get; set; }

        public T Result
        {
            get
            {
                if (!this.IsCompleted && !this.IsCanceled)
                {
                    this.Wait(-1);
                }
                return this._result;
            }
        }

        internal DateTime StartTime { get; set; }

        public CTaskStatus Status
        {
            get
            {
                return this._Status;
            }
            internal set
            {
                this._Status = value;
                if (this.StatusChange != null)
                {
                    this.StatusChange(this, new StatusChangeEventArgs(value));
                }
            }
        }

        public int WorkerThreadID { get; internal set; }
    }
}

