namespace CHystrix
{
    using System;

    public enum FailureTypeEnum
    {
        ExecutionFailed,
        ExecutionTimeout,
        ShortCircuited,
        ThreadIsolationRejected,
        SemaphoreIsolationRejected,
        FallbackRejected,
        FallbackExexecutionFailed
    }
}

