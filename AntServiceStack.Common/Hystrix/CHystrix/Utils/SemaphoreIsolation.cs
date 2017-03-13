namespace CHystrix.Utils
{
    using CHystrix;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class SemaphoreIsolation : IDisposable
    {
        private int _disposed;
        private bool _hasSemaphore;
        private int _markedResult;
        private int _started;
        private Stopwatch _stopwatch;
        private readonly CommandComponents Components;
        private readonly IsolationSemaphore ExecutionSemaphore;
        private readonly string Key;

        public SemaphoreIsolation(string commandKey) : this(commandKey, null)
        {
        }

        public SemaphoreIsolation(string commandKey, string groupKey) : this(commandKey, groupKey, null)
        {
        }

        public SemaphoreIsolation(string commandKey, string groupKey, string domain) : this(commandKey, groupKey, domain, null)
        {
        }

        public SemaphoreIsolation(string commandKey, string groupKey, string domain, Action<ICommandConfigSet> config) : this(null, commandKey, groupKey, domain, config)
        {
        }

        public SemaphoreIsolation(string instanceKey, string commandKey, string groupKey, string domain, Action<ICommandConfigSet> config)
        {
            Func<string, CommandComponents> valueFactory = null;
            Func<string, IsolationSemaphore> func2 = null;
            if (string.IsNullOrWhiteSpace(commandKey))
            {
                string message = "HystrixCommand Key cannot be null.";
                CommonUtils.Log.Log(LogLevelEnum.Fatal, message, new Dictionary<string, string>().AddLogTagData("FXD303002"));
                throw new ArgumentNullException(message);
            }
            this.Key = CommonUtils.GenerateKey(instanceKey, commandKey);
            instanceKey = string.IsNullOrWhiteSpace(instanceKey) ? null : instanceKey.Trim();
            commandKey = commandKey.Trim();
            groupKey = groupKey ?? "DefaultGroup";
            domain = domain ?? "Ant";
            if (valueFactory == null)
            {
                valueFactory = key => HystrixCommandBase.CreateCommandComponents(this.Key, instanceKey, commandKey, groupKey, domain, IsolationModeEnum.SemaphoreIsolation, config, typeof(SemaphoreIsolation));
            }
            this.Components = HystrixCommandBase.CommandComponentsCollection.GetOrAdd(this.Key, valueFactory);
            if (func2 == null)
            {
                func2 = key => new IsolationSemaphore(this.Components.ConfigSet.CommandMaxConcurrentCount);
            }
            this.ExecutionSemaphore = HystrixCommandBase.ExecutionSemaphores.GetOrAdd(this.Key, func2);
            this._stopwatch = new Stopwatch();
        }

        private bool CanEndExecution()
        {
            if (this._started == 0)
            {
                this.Components.Log.Log(LogLevelEnum.Warning, "The command has not been started.", this.GetLogTagInfo().AddLogTagData("FXD303041"));
                return false;
            }
            if ((this._disposed != 0) || (Interlocked.CompareExchange(ref this._disposed, 1, 0) != 0))
            {
                this.Components.Log.Log(LogLevelEnum.Warning, "The command has been ended. A command can be only ended once.", this.GetLogTagInfo().AddLogTagData("FXD303042"));
                return false;
            }
            if (this._stopwatch.IsRunning)
            {
                this._stopwatch.Stop();
            }
            return true;
        }

        private bool CanMarkResult()
        {
            if (this._started == 0)
            {
                this.Components.Log.Log(LogLevelEnum.Warning, "The command has not been started.", this.GetLogTagInfo().AddLogTagData("FXD303040"));
                return false;
            }
            if ((this._markedResult != 0) || (Interlocked.CompareExchange(ref this._markedResult, 1, 0) != 0))
            {
                return false;
            }
            if (this._stopwatch.IsRunning)
            {
                this._stopwatch.Stop();
            }
            return true;
        }

        private bool CanStartExecution()
        {
            if ((this._started == 0) && (Interlocked.CompareExchange(ref this._started, 1, 0) == 0))
            {
                return true;
            }
            this.Components.Log.Log(LogLevelEnum.Warning, "The command has been started. A command can be only started once.", this.GetLogTagInfo().AddLogTagData("FXD303018"));
            return false;
        }

        public static void Config(string commandKey, Action<ICommandConfigSet> config)
        {
            Config(commandKey, null, config);
        }

        public static void Config(string commandKey, string groupKey)
        {
            string domain = null;
            Config(commandKey, groupKey, domain);
        }

        public static void Config(string commandKey, string groupKey, string domain)
        {
            Config(null, commandKey, groupKey, domain);
        }

        public static void Config(string commandKey, string groupKey, Action<ICommandConfigSet> config)
        {
            Config(commandKey, groupKey, null, config);
        }

        public static void Config(string commandKey, string groupKey, string domain, int maxConcurrentCount)
        {
            Config(null, commandKey, groupKey, domain, maxConcurrentCount);
        }

        public static void Config(string instanceKey, string commandKey, string groupKey, string domain)
        {
            Config(instanceKey, commandKey, groupKey, domain, (Action<ICommandConfigSet>) null);
        }

        public static void Config(string commandKey, string groupKey, string domain, Action<ICommandConfigSet> config)
        {
            Config(null, commandKey, groupKey, domain, config);
        }

        public static void Config(string commandKey, string groupKey, string domain, int maxConcurrentCount, int timeoutInMilliseconds)
        {
            Config(null, commandKey, groupKey, domain, maxConcurrentCount, timeoutInMilliseconds);
        }

        public static void Config(string instanceKey, string commandKey, string groupKey, string domain, int maxConcurrentCount)
        {
            int? fallbackMaxConcurrentCount = new int?(maxConcurrentCount);
            Config(instanceKey, commandKey, groupKey, domain, new int?(maxConcurrentCount), null, null, null, fallbackMaxConcurrentCount);
        }

        public static void Config(string instanceKey, string commandKey, string groupKey, string domain, Action<ICommandConfigSet> config)
        {
            new SemaphoreIsolation(instanceKey, commandKey, groupKey, domain, config);
        }

        public static void Config(string instanceKey, string commandKey, string groupKey, string domain, int maxConcurrentCount, int timeoutInMilliseconds)
        {
            int? fallbackMaxConcurrentCount = new int?(maxConcurrentCount);
            Config(instanceKey, commandKey, groupKey, domain, new int?(maxConcurrentCount), new int?(timeoutInMilliseconds), null, null, fallbackMaxConcurrentCount);
        }

        internal static void Config(string commandKey, string groupKey, string domain, int? maxConcurrentCount = new int?(), int? timeoutInMilliseconds = new int?(), int? circuitBreakerRequestCountThreshold = new int?(), int? circuitBreakerErrorThresholdPercentage = new int?(), int? fallbackMaxConcurrentCount = new int?())
        {
            Config(null, commandKey, groupKey, domain, maxConcurrentCount, timeoutInMilliseconds, circuitBreakerRequestCountThreshold, circuitBreakerErrorThresholdPercentage, fallbackMaxConcurrentCount);
        }

        internal static void Config(string instanceKey, string commandKey, string groupKey, string domain, int? maxConcurrentCount = new int?(), int? timeoutInMilliseconds = new int?(), int? circuitBreakerRequestCountThreshold = new int?(), int? circuitBreakerErrorThresholdPercentage = new int?(), int? fallbackMaxConcurrentCount = new int?())
        {
            Config(instanceKey, commandKey, groupKey, domain, delegate (ICommandConfigSet configSet) {
                if (maxConcurrentCount.HasValue)
                {
                    configSet.CommandMaxConcurrentCount = maxConcurrentCount.Value;
                }
                if (timeoutInMilliseconds.HasValue)
                {
                    configSet.CommandTimeoutInMilliseconds = timeoutInMilliseconds.Value;
                }
                if (circuitBreakerRequestCountThreshold.HasValue)
                {
                    configSet.CircuitBreakerRequestCountThreshold = circuitBreakerRequestCountThreshold.Value;
                }
                if (circuitBreakerErrorThresholdPercentage.HasValue)
                {
                    configSet.CircuitBreakerErrorThresholdPercentage = circuitBreakerErrorThresholdPercentage.Value;
                }
                if (fallbackMaxConcurrentCount.HasValue)
                {
                    configSet.FallbackMaxConcurrentCount = fallbackMaxConcurrentCount.Value;
                }
            });
        }

        private static SemaphoreIsolation ConvertInstance(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            SemaphoreIsolation isolation = instance as SemaphoreIsolation;
            if (isolation == null)
            {
                throw new ArgumentException("instance should be of type SemaphoreIsolation!");
            }
            return isolation;
        }

        public static object CreateInstance(string commandKey)
        {
            return new SemaphoreIsolation(commandKey);
        }

        public static object CreateInstance(string instanceKey, string commandKey)
        {
            return new SemaphoreIsolation(instanceKey, commandKey, null, null, null);
        }

        public void Dispose()
        {
            this.EndExecution();
        }

        public void EndExecution()
        {
            if (this.CanEndExecution() && this._hasSemaphore)
            {
                this.ExecutionSemaphore.Release();
                this.Components.Metrics.MarkExecutionLatency(this._stopwatch.ElapsedMilliseconds);
                this.Components.Metrics.MarkTotalExecutionLatency(this._stopwatch.ElapsedMilliseconds);
            }
        }

        public static void EndExecution(object instance)
        {
            ConvertInstance(instance).EndExecution();
        }

        private Dictionary<string, string> GetLogTagInfo()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("HystrixAppName", HystrixCommandBase.HystrixAppName);
            dictionary.Add("Key", this.Key);
            dictionary.Add("InstanceKey", this.Components.CommandInfo.InstanceKey);
            dictionary.Add("CommandKey", this.Components.CommandInfo.CommandKey);
            dictionary.Add("GroupKey", this.Components.CommandInfo.GroupKey);
            dictionary.Add("Domain", this.Components.CommandInfo.Domain);
            dictionary.Add("IsolationMode", this.Components.CommandInfo.Type);
            return dictionary;
        }

        public void MarkBadRequest()
        {
            if (this.CanMarkResult())
            {
                this.Components.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.BadRequest);
                this.Components.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.ExceptionThrown);
                if (this.Components.ConfigSet.LogExecutionError)
                {
                    this.Components.Log.Log(LogLevelEnum.Error, "HystrixCommand request is bad.", this.GetLogTagInfo().AddLogTagData("FXD303021"));
                }
            }
        }

        public static void MarkBadRequest(object instance)
        {
            ConvertInstance(instance).MarkBadRequest();
        }

        public void MarkFailure()
        {
            if (this.CanMarkResult())
            {
                this.Components.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.Failed);
                this.Components.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.ExceptionThrown);
                if (this.Components.ConfigSet.LogExecutionError)
                {
                    this.Components.Log.Log(LogLevelEnum.Error, "HystrixCommand execution failed.", this.GetLogTagInfo().AddLogTagData("FXD303022"));
                }
            }
        }

        public static void MarkFailure(object instance)
        {
            ConvertInstance(instance).MarkFailure();
        }

        public void MarkSuccess()
        {
            if (this.CanMarkResult())
            {
                if (this._stopwatch.ElapsedMilliseconds > this.Components.ConfigSet.CommandTimeoutInMilliseconds)
                {
                    this.Components.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.Timeout);
                    this.Components.Log.Log(LogLevelEnum.Warning, string.Format("HystrixCommand execution timeout: {0}ms.", this._stopwatch.ElapsedMilliseconds), this.GetLogTagInfo().AddLogTagData("FXD303020"));
                }
                else
                {
                    this.Components.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.Success);
                    this.Components.CircuitBreaker.MarkSuccess();
                }
            }
        }

        public static void MarkSuccess(object instance)
        {
            ConvertInstance(instance).MarkSuccess();
        }

        public void StartExecution()
        {
            if (this.CanStartExecution())
            {
                this._stopwatch.Start();
                if (!this.Components.CircuitBreaker.AllowRequest())
                {
                    this.Components.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.ShortCircuited);
                    this.Components.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.ExceptionThrown);
                    Interlocked.CompareExchange(ref this._markedResult, 1, 0);
                    string message = "Circuit Breaker is open. Execution was short circuited.";
                    this.Components.Log.Log(LogLevelEnum.Error, message, this.GetLogTagInfo().AddLogTagData("FXD303019"));
                    throw new HystrixException(FailureTypeEnum.ShortCircuited, base.GetType(), this.Key, message);
                }
                this._hasSemaphore = this.ExecutionSemaphore.TryAcquire();
                if (!this._hasSemaphore)
                {
                    this.Components.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.Rejected);
                    this.Components.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.ExceptionThrown);
                    Interlocked.CompareExchange(ref this._markedResult, 1, 0);
                    string str2 = "HystrixCommand execution was rejected.";
                    this.Components.Log.Log(LogLevelEnum.Error, str2, this.GetLogTagInfo().AddLogTagData("FXD303023"));
                    throw new HystrixException(FailureTypeEnum.SemaphoreIsolationRejected, base.GetType(), this.Key, str2);
                }
            }
        }

        public static void StartExecution(object instance)
        {
            ConvertInstance(instance).StartExecution();
        }
    }
}

