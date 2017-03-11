namespace CHystrix.Threading
{
    using System;
    using System.Runtime.InteropServices;

    internal interface ICThreadPool
    {
        CWorkItem<T> QueueWorkItem<T>(Func<T> act, EventHandler<StatusChangeEventArgs> onStatusChange = null);

        int MaxConcurrentCount { get; set; }

        int WorkItemTimeoutMiliseconds { get; set; }
    }
}

