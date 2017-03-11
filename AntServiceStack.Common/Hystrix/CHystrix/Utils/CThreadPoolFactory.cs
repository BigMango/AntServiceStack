namespace CHystrix.Utils
{
    using CHystrix;
    using CHystrix.Threading;
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal static class CThreadPoolFactory
    {
        private static ConcurrentDictionary<string, CThreadPool> _pools = new ConcurrentDictionary<string, CThreadPool>();
        private static Timer _timer = new Timer(delegate (object o) {
            foreach (CThreadPool pool in _pools.Values)
            {
                pool.SetTimeoutWorkStatus();
            }
        }, null, 0, 0x3e8);

        [CompilerGenerated]
        private static void SetTimeoutWorkStatus(object o)
        {
            foreach (CThreadPool pool in _pools.Values)
            {
                pool.SetTimeoutWorkStatus();
            }
        }

        internal static CThreadPool GetCommandPool(HystrixCommandBase command)
        {
            return _pools.GetOrAdd(command.Key.ToLower(), new CThreadPool(command.ConfigSet.CommandMaxConcurrentCount, command.ConfigSet.CommandTimeoutInMilliseconds));
        }

        internal static CThreadPool GetPoolByKey(string key)
        {
            CThreadPool pool = null;
            _pools.TryGetValue(key.ToLower(), out pool);
            return pool;
        }

        public static CWorkItem<T> QueueWorkItem<T>(this ThreadIsolationCommand<T> command, Func<T> func, EventHandler<StatusChangeEventArgs> onStatusChange = null)
        {
            CThreadPool commandPool = GetCommandPool(command);
            if (commandPool.NowWaitingWorkCount >= ((commandPool.MaxConcurrentCount * command.ConfigSet.MaxAsyncCommandExceedPercentage) / 100))
            {
                throw new HystrixException(FailureTypeEnum.ThreadIsolationRejected, command.GetType(), command.Key, "already exceed the max workitem, can't add any more.");
            }
            return commandPool.QueueWorkItem<T>(func, onStatusChange);
        }

        internal static void ResetThreadPoolByCommandKey(string key)
        {
            CThreadPool pool;
            if (_pools.TryGetValue(key, out pool))
            {
                pool.Reset();
            }
        }

        public static void UpdateCommandTimeoutInMilliseconds<T>(this ThreadIsolationCommand<T> command, int seconds)
        {
            GetCommandPool(command).WorkItemTimeoutMiliseconds = seconds;
        }

        public static void UpdateMaxConcurrentCount<T>(this ThreadIsolationCommand<T> command, int count)
        {
            GetCommandPool(command).MaxConcurrentCount = count;
        }

        internal static ConcurrentDictionary<string, CThreadPool> AllPools
        {
            get
            {
                return _pools;
            }
        }
    }
}

