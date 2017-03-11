namespace CHystrix
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal interface ICommandMetrics
    {
        long GetAverageExecutionLatency();
        long GetAverageTotalExecutionLatency();
        Dictionary<CommandExecutionEventEnum, int> GetExecutionEventDistribution();
        CommandExecutionHealthSnapshot GetExecutionHealthSnapshot();
        void GetExecutionLatencyAuditData(out int count, out long sum, out long min, out long max);
        long GetExecutionLatencyPencentile(double percentage);
        void GetTotalExecutionLatencyAuditData(out int count, out long sum, out long min, out long max);
        long GetTotalExecutionLatencyPencentile(double percentage);
        void MarkExecutionEvent(CommandExecutionEventEnum executionEvent);
        void MarkExecutionLatency(long milliseconds);
        void MarkTotalExecutionLatency(long milliseconds);
        void Reset();

        int CurrentConcurrentExecutionCount { get; }

        int CurrentWaitCount { get; }
    }
}

