using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

using AntServiceStack.Common.Hystrix.Atomic;

namespace AntServiceStack.WebHost.Endpoints.Utils
{
    internal class ConnectionRequestCounterCache
    {
        public const int CacheLimit = 10 * 1000;
        public const int OverDueMilliseconds = 5 * 60 * 1000;

        private static ConnectionRequestCounterCache _instance = new ConnectionRequestCounterCache();
        public static ConnectionRequestCounterCache Instance
        {
            get
            {
                return _instance;
            }
        }

        private ConcurrentDictionary<string, CounterData> _connectionRequestCountCounter;

        private AtomicBoolean _clearOverDueDataStarted;

        private ConnectionRequestCounterCache()
        {
            _connectionRequestCountCounter = new ConcurrentDictionary<string, CounterData>();
            _clearOverDueDataStarted = new AtomicBoolean();
        }

        private string GenerateKey(string ip, string port)
        {
            return ip + ":" + port;
        }

        public int IncrementAndGet(string ip, string port)
        {
            string key = GenerateKey(ip, port);
            CounterData counterData;
            _connectionRequestCountCounter.TryGetValue(key, out counterData);
            if (counterData == null)
            {
                if (_connectionRequestCountCounter.Count > CacheLimit)
                {
                    TryClearOverDueData();
                    return 0;
                }
                counterData = _connectionRequestCountCounter.GetOrAdd(key, _ => new CounterData());
            }

            return counterData.Counter.IncrementAndGet();
        }

        public void Reset(string ip, string port)
        {
            string key = GenerateKey(ip, port);
            CounterData counterData;
            _connectionRequestCountCounter.TryRemove(key, out counterData);
        }

        private void TryClearOverDueData()
        {
            if (!_clearOverDueDataStarted.CompareAndSet(false, true))
                return;

            DateTime now = DateTime.Now;
            List<string> overDueKeys = new List<string>();
            foreach (string key in _connectionRequestCountCounter.Keys)
            {
                if ((now - _connectionRequestCountCounter[key].StartTime).TotalMilliseconds > OverDueMilliseconds)
                    overDueKeys.Add(key);
            }

            foreach (string key in overDueKeys)
            {
                CounterData counterData;
                _connectionRequestCountCounter.TryRemove(key, out counterData);
            }

            _clearOverDueDataStarted.GetAndSet(false);
        }

        private class CounterData
        {
            public AtomicInteger Counter { get; private set; }
            public DateTime StartTime { get; private set; }

            public CounterData()
            {
                Counter = new AtomicInteger();
                StartTime = DateTime.Now;
            }
        }
    }
}
