namespace CHystrix
{
    using System;

    public enum CommandStatusEnum
    {
        NotStarted,
        Started,
        Success,
        Failed,
        Timeout,
        Rejected,
        ShortCircuited,
        FallbackSuccess,
        FallbackFailed,
        FallbackRejected
    }
}

