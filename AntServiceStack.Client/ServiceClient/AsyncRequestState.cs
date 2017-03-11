using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;

namespace AntServiceStack.ServiceClient
{
    internal class AsyncRequestState : IIOCPAsyncState
    {
        public HttpWebRequest Request { get; private set; }

        public bool TimeoutEnabled { get; private set; }

        protected Timer Timer;

        protected readonly object Lock = new object();

        protected bool _isCompleted;

        public AsyncRequestState()
        { }

        public void Initialize(HttpWebRequest request, bool timtoutEnabled)
        {
            Request = request;
            TimeoutEnabled = timtoutEnabled;
            
            if (TimeoutEnabled)
                Timer = new Timer(OnTimeout, this, Request.Timeout, Timeout.Infinite);
        }

        public void Complete()
        {
            if (!TimeoutEnabled)
                return;

            if (!_isCompleted && Monitor.TryEnter(Lock))
            {
                if (!_isCompleted)
                {
                    _isCompleted = true;
                    StopTimer();
                }

                Monitor.Exit(Lock);
            }
        }

        protected internal void OnTimeout(object state)
        {
            if (!TimeoutEnabled)
                return;

            if (!_isCompleted && Monitor.TryEnter(Lock))
            {
                if (!_isCompleted)
                {
                    _isCompleted = true;
                    StopTimer();
                    try
                    {
                        Request.Abort();
                    }
                    catch
                    {
                    }
                }

                Monitor.Exit(Lock);
            }
        }

        protected internal void StopTimer()
        {
            if (Timer == null)
                return;

            try
            {
                using (Timer)
                {
                    Timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
            catch
            {
            }
        }
    }
}
