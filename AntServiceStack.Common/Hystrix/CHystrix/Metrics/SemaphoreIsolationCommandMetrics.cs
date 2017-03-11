namespace CHystrix.Metrics
{
    using CHystrix;
    using CHystrix.Utils;
    using System;

    internal class SemaphoreIsolationCommandMetrics : CommandMetrics
    {
        private readonly string Key;

        public SemaphoreIsolationCommandMetrics(ICommandConfigSet configSet, string key) : base(configSet)
        {
            this.Key = key;
        }

        public override int CurrentConcurrentExecutionCount
        {
            get
            {
                IsolationSemaphore semaphore;
                HystrixCommandBase.ExecutionSemaphores.TryGetValue(this.Key, out semaphore);
                if (semaphore == null)
                {
                    return 0;
                }
                return (semaphore.Count - semaphore.CurrentCount);
            }
        }

        public override int CurrentWaitCount
        {
            get
            {
                return 0;
            }
        }
    }
}

