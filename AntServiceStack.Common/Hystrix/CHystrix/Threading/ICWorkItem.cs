namespace CHystrix.Threading
{
    using System;

    internal interface ICWorkItem
    {
        bool CanDo(int timeout);
        void Do();
        bool MarkTimeout();
        bool Wait(int waitMilliseconds);

        System.Exception Exception { get; }

        int ExeuteMilliseconds { get; }

        bool IsCanceled { get; }

        bool IsCompleted { get; }

        bool IsFaulted { get; }

        int RealExecuteMilliseconds { get; }

        CTaskStatus Status { get; }
    }
}

