namespace CHystrix.Threading
{
    using CHystrix;
    using CHystrix.Utils;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class CThread
    {
        private CThreadWorkCompleteCallback _completeCallback;
        private volatile bool _isIdle = true;
        private bool _isShutdown;
        private bool _isStart;
        private ManualResetEvent _newJobWaitHandle = new ManualResetEvent(false);
        private ICWorkItem _task;
        private Thread _thread;
        private int _threadID = -1;
        public static TimeSpan ThreadMaxIdleTime = TimeSpan.FromMinutes(5.0);

        public CThread(CThreadWorkCompleteCallback completeCallback)
        {
            this._completeCallback = completeCallback;
            this._thread = new Thread(new ThreadStart(this.DoTask));
            this._thread.IsBackground = true;
            this._threadID = this._thread.ManagedThreadId;
        }

        private void DoTask()
        {
            while (!this._isShutdown)
            {
                this._isIdle = false;
                try
                {
                    while (this._task != null)
                    {
                        ICWorkItem item;
                        lock (this._task)
                        {
                            item = this._task;
                            item.Do();
                        }
                        this._task = null;
                        this._isIdle = true;
                        this._completeCallback(this, item);
                    }
                }
                catch (Exception exception)
                {
                    CommonUtils.Log.Log(LogLevelEnum.Error, exception.Message, exception);
                }
                if (!this._isShutdown)
                {
                    this._newJobWaitHandle.Reset();
                    if (!this._newJobWaitHandle.WaitOne(ThreadMaxIdleTime) && (this._task == null))
                    {
                        this._isShutdown = true;
                        this._isIdle = false;
                        this._thread = null;
                    }
                }
            }
        }

        public bool DoWork(ICWorkItem task)
        {
            if (!this._isIdle || (this._task != null))
            {
                return false;
            }
            this._task = task;
            if (!this._isStart)
            {
                this._isStart = true;
                this._thread.Start();
            }
            else
            {
                this._newJobWaitHandle.Set();
            }
            return true;
        }

        public void Shutdown()
        {
            this._isShutdown = true;
            this._isIdle = false;
            this._thread = null;
            this._newJobWaitHandle.Set();
        }

        public bool IsIdle
        {
            get
            {
                return this._isIdle;
            }
        }

        public bool IsShutdown
        {
            get
            {
                return this._isShutdown;
            }
        }

        public ICWorkItem NowTask
        {
            get
            {
                return this._task;
            }
        }

        public int ThreadID
        {
            get
            {
                return this._threadID;
            }
        }
    }
}

