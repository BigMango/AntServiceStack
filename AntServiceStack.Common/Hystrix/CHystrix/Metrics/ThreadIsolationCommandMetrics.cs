namespace CHystrix.Metrics
{
    using CHystrix;
    using CHystrix.Utils;
    using System;

    internal class ThreadIsolationCommandMetrics : CommandMetrics
    {
        private readonly string Key;

        public ThreadIsolationCommandMetrics(ICommandConfigSet configSet, string key) : base(configSet)
        {
            this.Key = key;
        }

        public override int CurrentConcurrentExecutionCount
        {
            get
            {
                return CThreadPoolFactory.GetPoolByKey(this.Key).NowRunningWorkCount;
            }
        }

        public override int CurrentWaitCount
        {
            get
            {
                return CThreadPoolFactory.GetPoolByKey(this.Key).NowWaitingWorkCount;
            }
        }
    }
}

