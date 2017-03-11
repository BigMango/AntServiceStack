namespace CHystrix.Metrics
{
    using CHystrix;
    using CHystrix.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Timers;

    internal static class MetricsReporter
    {
        private const string HystrixAppInstanceMetricsName = "chystrix.app.instance";
        private const string HystrixErrorPercentageMetricsName = "chystrix.error.percentage";
        private const string HystrixEventDistributionMetricsName = "chystrix.execution.event.distribution";
        private const string HystrixExecutionConcurrentCountMetricsName = "chystrix.execution.concurrent_count";
        private const string HystrixExecutionLatencyPercentileMetricsName = "chystrix.execution.latency.percentile";
        private const string HystrixResourceUtilizationMetricsName = "chystrix.resource.utilization";
        private const string HystrixTotalExecutionLatencyPercentileMetricsName = "chystrix.execution.total_latency.percentile";
        private static Dictionary<double, string> LatencyPercentileValues;
        //private static IMetric Metrics = MetricManager.GetMetricLogger();
        public const int SendMetricsIntervalMilliseconds = 0xea60;
        private static System.Timers.Timer Timer;

        static MetricsReporter()
        {
            Dictionary<double, string> dictionary = new Dictionary<double, string>();
            dictionary.Add(0.0, "0");
            dictionary.Add(25.0, "25");
            dictionary.Add(50.0, "50");
            dictionary.Add(75.0, "75");
            dictionary.Add(90.0, "90");
            dictionary.Add(95.0, "95");
            dictionary.Add(99.0, "99");
            dictionary.Add(99.5, "99.5");
            dictionary.Add(100.0, "100");
            LatencyPercentileValues = dictionary;
        }

        private static void Report()
        {
            Dictionary<string, string> tags = new Dictionary<string, string>();
            tags["app"] = HystrixCommandBase.HystrixAppName.ToLower();
            tags["version"] = HystrixCommandBase.HystrixVersion;
            string key = string.Empty;
            DateTime now = DateTime.Now;
            //Metrics.log("chystrix.app.instance", (long) 1L, tags, now);
            foreach (CommandComponents components in HystrixCommandBase.CommandComponentsCollection.ToDictionary<KeyValuePair<string, CommandComponents>, string, CommandComponents>(p => p.Key, p => p.Value).Values)
            {
                tags["instancekey"] = components.CommandInfo.InstanceKey;
                tags["commandkey"] = components.CommandInfo.CommandKey;
                tags["groupkey"] = components.CommandInfo.GroupKey;
                tags["domain"] = components.CommandInfo.Domain;
                tags["isolationmode"] = components.CommandInfo.Type;
                key = "event";
                foreach (KeyValuePair<CommandExecutionEventEnum, int> pair in components.Metrics.GetExecutionEventDistribution())
                {
                    if ((pair.Value > 0) && (((CommandExecutionEventEnum) pair.Key) != CommandExecutionEventEnum.ExceptionThrown))
                    {
                        tags[key] = pair.Key.ToString();
                        //Metrics.log("chystrix.execution.event.distribution", (long) pair.Value, tags, now);
                    }
                }
                if (tags.ContainsKey(key))
                {
                    tags.Remove(key);
                }
                CommandExecutionHealthSnapshot executionHealthSnapshot = components.Metrics.GetExecutionHealthSnapshot();
                if (executionHealthSnapshot.ErrorPercentage > 0)
                {
                    //Metrics.log("chystrix.error.percentage", (long) executionHealthSnapshot.ErrorPercentage, tags, now);
                }
                key = "percentile";
                foreach (KeyValuePair<double, string> pair2 in LatencyPercentileValues)
                {
                    tags[key] = pair2.Value;
                    long totalExecutionLatencyPencentile = components.Metrics.GetTotalExecutionLatencyPencentile(pair2.Key);
                    if (totalExecutionLatencyPencentile > 0L)
                    {
                        //Metrics.log("chystrix.execution.total_latency.percentile", totalExecutionLatencyPencentile, tags, now);
                    }
                    if (components.IsolationMode == IsolationModeEnum.ThreadIsolation)
                    {
                        totalExecutionLatencyPencentile = components.Metrics.GetExecutionLatencyPencentile(pair2.Key);
                        if (totalExecutionLatencyPencentile > 0L)
                        {
                            //Metrics.log("chystrix.execution.latency.percentile", totalExecutionLatencyPencentile, tags, now);
                        }
                    }
                }
                if (tags.ContainsKey(key))
                {
                    tags.Remove(key);
                }
                int currentConcurrentExecutionCount = components.Metrics.CurrentConcurrentExecutionCount;
                if (currentConcurrentExecutionCount > 0)
                {
                    //Metrics.log("chystrix.execution.concurrent_count", (long) currentConcurrentExecutionCount, tags, now);
                }
                int num3 = 0;
                int commandMaxConcurrentCount = components.ConfigSet.CommandMaxConcurrentCount;
                if (commandMaxConcurrentCount > 0)
                {
                    num3 = (int) ((((double) currentConcurrentExecutionCount) / ((double) commandMaxConcurrentCount)) * 100.0);
                }
                else
                {
                    num3 = components.Metrics.CurrentConcurrentExecutionCount * 100;
                }
                if (num3 > 0)
                {
                    //Metrics.log("chystrix.resource.utilization", (long) num3, tags, now);
                }
            }
        }

        public static void Reset()
        {
            try
            {
                if (Timer != null)
                {
                    System.Timers.Timer timer = Timer;
                    Timer = null;
                    using (timer)
                    {
                        timer.Stop();
                    }
                }
            }
            catch
            {
            }
        }

        private static void SendMetrics(object sender, ElapsedEventArgs arg)
        {
            try
            {
                Report();
            }
            catch (Exception exception)
            {
                CommonUtils.Log.Log(LogLevelEnum.Warning, "Failed to send metrics.", exception, new Dictionary<string, string>().AddLogTagData("FXD303015"));
            }
        }

        public static void Start()
        {
            if (Timer == null)
            {
                System.Timers.Timer timer = new System.Timers.Timer {
                    Interval = 60000.0,
                    AutoReset = true,
                    Enabled = true
                };
                Timer = timer;
                Timer.Elapsed += new ElapsedEventHandler(MetricsReporter.SendMetrics);
            }
        }
    }
}

