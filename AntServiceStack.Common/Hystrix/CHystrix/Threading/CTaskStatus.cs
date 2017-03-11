namespace CHystrix.Threading
{
    using System;

    internal enum CTaskStatus
    {
        WaitingToRun,
        RanToCompletion,
        Running,
        Faulted,
        Canceled
    }
}

