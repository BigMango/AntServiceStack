namespace CHystrix.Metrics
{
    using CHystrix;
    using CHystrix.Utils;
    using CHystrix.Utils.Buffer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    internal abstract class CommandMetrics : ICommandMetrics
    {
        protected Dictionary<CommandExecutionEventEnum, int> _executionEventDistributionSnapshot;
        protected CommandExecutionHealthSnapshot _executionEventHealthSnapshot;
        protected long _lastGetLatencyBufferSnapshotTimeInMilliseconds;
        protected long _lastGetTotalLatencyBufferSnapshotTimeInMilliseconds;
        protected long _lastUpdateExecutionEventSnapshotTimeInMilliseconds;
        protected List<long> _latencyBufferSnapshot;
        protected List<long> _totalLatencyBufferSnapshot;
        protected readonly ICommandConfigSet ConfigSet;
        protected CounterBuffer<CommandExecutionEventEnum> ExecutionEventBuffer;
        protected IntegerPercentileBuffer ExecutionLatencyBuffer;
        protected IntegerPercentileBuffer TotalExecutionLatencyBuffer;

        public CommandMetrics(ICommandConfigSet configSet)
        {
            this.ConfigSet = configSet;
            this.Reset();
        }

        public long GetAverageExecutionLatency()
        {
            this.UpdateExecutionLatencyBufferSnapshot();
            List<long> list = this._latencyBufferSnapshot;
            if (list.Count == 0)
            {
                return 0L;
            }
            return (long) ((IEnumerable<long>) list).Average();
        }

        public long GetAverageTotalExecutionLatency()
        {
            this.UpdateTotalExecutionLatencyBufferSnapshot();
            List<long> list = this._totalLatencyBufferSnapshot;
            if (list.Count == 0)
            {
                return 0L;
            }
            return (long) ((IEnumerable<long>) list).Average();
        }

        public Dictionary<CommandExecutionEventEnum, int> GetExecutionEventDistribution()
        {
            this.UpdateExecutionEventSnapshot();
            return this._executionEventDistributionSnapshot;
        }

        public CommandExecutionHealthSnapshot GetExecutionHealthSnapshot()
        {
            this.UpdateExecutionEventSnapshot();
            return this._executionEventHealthSnapshot;
        }

        public void GetExecutionLatencyAuditData(out int count, out long sum, out long min, out long max)
        {
            this.UpdateExecutionLatencyBufferSnapshot();
            this._latencyBufferSnapshot.GetAuditData(out count, out sum, out min, out max);
        }

        public long GetExecutionLatencyPencentile(double percentage)
        {
            this.UpdateExecutionLatencyBufferSnapshot();
            return this._latencyBufferSnapshot.GetPercentile(percentage, true);
        }

        public void GetTotalExecutionLatencyAuditData(out int count, out long sum, out long min, out long max)
        {
            this.UpdateTotalExecutionLatencyBufferSnapshot();
            this._totalLatencyBufferSnapshot.GetAuditData(out count, out sum, out min, out max);
        }

        public long GetTotalExecutionLatencyPencentile(double percentage)
        {
            this.UpdateTotalExecutionLatencyBufferSnapshot();
            return this._totalLatencyBufferSnapshot.GetPercentile(percentage, true);
        }

        public void MarkExecutionEvent(CommandExecutionEventEnum executionEvent)
        {
            this.ExecutionEventBuffer.IncreaseCount(executionEvent);
        }

        public void MarkExecutionLatency(long milliseconds)
        {
            this.ExecutionLatencyBuffer.Add(milliseconds);
        }

        public void MarkTotalExecutionLatency(long milliseconds)
        {
            this.TotalExecutionLatencyBuffer.Add(milliseconds);
        }

        public void Reset()
        {
            this.ExecutionEventBuffer = new CounterBuffer<CommandExecutionEventEnum>(this.ConfigSet.MetricsRollingStatisticalWindowInMilliseconds, this.ConfigSet.MetricsRollingStatisticalWindowBuckets);
            this.ExecutionLatencyBuffer = new IntegerPercentileBuffer(this.ConfigSet.MetricsRollingPercentileWindowInMilliseconds, this.ConfigSet.MetricsRollingPercentileWindowBuckets, this.ConfigSet.MetricsRollingPercentileBucketSize);
            this.TotalExecutionLatencyBuffer = new IntegerPercentileBuffer(this.ConfigSet.MetricsRollingPercentileWindowInMilliseconds, this.ConfigSet.MetricsRollingPercentileWindowBuckets, this.ConfigSet.MetricsRollingPercentileBucketSize);
            this._lastUpdateExecutionEventSnapshotTimeInMilliseconds = 0L;
            this._executionEventDistributionSnapshot = new Dictionary<CommandExecutionEventEnum, int>();
            this._executionEventHealthSnapshot = new CommandExecutionHealthSnapshot(0, 0);
            this._lastGetLatencyBufferSnapshotTimeInMilliseconds = 0L;
            this._latencyBufferSnapshot = new List<long>();
            this._lastGetTotalLatencyBufferSnapshotTimeInMilliseconds = 0L;
            this._totalLatencyBufferSnapshot = new List<long>();
        }

        private void UpdateExecutionEventSnapshot()
        {
            long currentTimeInMiliseconds = CommonUtils.CurrentTimeInMiliseconds;
            if ((this._lastUpdateExecutionEventSnapshotTimeInMilliseconds == 0L) || ((this._lastUpdateExecutionEventSnapshotTimeInMilliseconds + this.ConfigSet.MetricsHealthSnapshotIntervalInMilliseconds) <= currentTimeInMiliseconds))
            {
                Dictionary<CommandExecutionEventEnum, int> executionEventDistribution = new Dictionary<CommandExecutionEventEnum, int>();
                foreach (CommandExecutionEventEnum enum2 in CommonUtils.CommandExecutionEvents)
                {
                    executionEventDistribution[enum2] = this.ExecutionEventBuffer.GetCount(enum2);
                }
                this._executionEventHealthSnapshot = executionEventDistribution.GetHealthSnapshot();
                this._executionEventDistributionSnapshot = executionEventDistribution;
                this._lastUpdateExecutionEventSnapshotTimeInMilliseconds = currentTimeInMiliseconds;
            }
        }

        private void UpdateExecutionLatencyBufferSnapshot()
        {
            long currentTimeInMiliseconds = CommonUtils.CurrentTimeInMiliseconds;
            if ((this._lastGetLatencyBufferSnapshotTimeInMilliseconds == 0L) || ((this._lastGetLatencyBufferSnapshotTimeInMilliseconds + this.ConfigSet.MetricsHealthSnapshotIntervalInMilliseconds) <= currentTimeInMiliseconds))
            {
                List<long> snapShot = this.ExecutionLatencyBuffer.GetSnapShot();
                snapShot.Sort();
                this._latencyBufferSnapshot = snapShot;
                this._lastGetLatencyBufferSnapshotTimeInMilliseconds = currentTimeInMiliseconds;
            }
        }

        private void UpdateTotalExecutionLatencyBufferSnapshot()
        {
            long currentTimeInMiliseconds = CommonUtils.CurrentTimeInMiliseconds;
            if ((this._lastGetTotalLatencyBufferSnapshotTimeInMilliseconds == 0L) || ((this._lastGetTotalLatencyBufferSnapshotTimeInMilliseconds + this.ConfigSet.MetricsHealthSnapshotIntervalInMilliseconds) <= currentTimeInMiliseconds))
            {
                List<long> snapShot = this.TotalExecutionLatencyBuffer.GetSnapShot();
                snapShot.Sort();
                this._totalLatencyBufferSnapshot = snapShot;
                this._lastGetTotalLatencyBufferSnapshotTimeInMilliseconds = currentTimeInMiliseconds;
            }
        }

        public abstract int CurrentConcurrentExecutionCount { get; }

        public abstract int CurrentWaitCount { get; }
    }
}

