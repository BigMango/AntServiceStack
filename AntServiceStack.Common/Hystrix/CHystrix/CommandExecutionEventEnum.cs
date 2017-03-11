namespace CHystrix
{
    using System;

    internal enum CommandExecutionEventEnum
    {
        Success,
        BadRequest,
        Failed,
        Rejected,
        Timeout,
        ShortCircuited,
        FallbackSuccess,
        FallbackFailed,
        FallbackRejected,
        ExceptionThrown
    }
}

