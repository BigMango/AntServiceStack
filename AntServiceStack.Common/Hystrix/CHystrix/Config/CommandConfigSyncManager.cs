namespace CHystrix.Config
{
    using CHystrix;
    using CHystrix.Utils;
    using CHystrix.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Timers;

    internal static class CommandConfigSyncManager
    {
        private const string ConfigServiceOperationName = "GetApplicationConfig";
        public const int SyncConfigIntervalMilliseconds = 0x7530;
        private static System.Timers.Timer Timer;

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

        public static void Start()
        {
            if (Timer == null)
            {
                SyncConfig(null, null);
                System.Timers.Timer timer = new System.Timers.Timer {
                    Interval = 30000.0,
                    AutoReset = true,
                    Enabled = true
                };
                Timer = timer;
                Timer.Elapsed += new ElapsedEventHandler(CommandConfigSyncManager.SyncConfig);
            }
        }

        private static void SyncConfig(object sender, ElapsedEventArgs arg)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(HystrixCommandBase.ConfigServiceUrl))
                {
                    CommonUtils.Log.Log(LogLevelEnum.Warning, " Config Service Url is empty. so can not to SyncConfig from remote server", new Dictionary<string, string>().AddLogTagData("FXD303011"));
                }
                else
                {
                    string url = HystrixCommandBase.ConfigServiceUrl.WithTrailingSlash() + "GetApplicationConfig.json";
                    GetApplicationConfigRequestType type = new GetApplicationConfigRequestType {
                        AppName = HystrixCommandBase.HystrixAppName.ToLower()
                    };
                    string str2 = url.PostJsonToUrl(type.ToJson(), null, null);
                    if (string.IsNullOrWhiteSpace(str2))
                    {
                        CommonUtils.Log.Log(LogLevelEnum.Warning, "Got null response from config service: " + HystrixCommandBase.ConfigServiceUrl, new Dictionary<string, string>().AddLogTagData("FXD303012"));
                    }
                    else
                    {
                        GetApplicationConfigResponseType type2 = str2.FromJson<GetApplicationConfigResponseType>();
                        if (type2 != null)
                        {
                            if (type2.DefaultConfig != null)
                            {
                                if ((type2.DefaultConfig.CircuitBreakerErrorThresholdPercentage.HasValue && (type2.DefaultConfig.CircuitBreakerErrorThresholdPercentage.Value >= 20)) && (type2.DefaultConfig.CircuitBreakerErrorThresholdPercentage.Value <= 100))
                                {
                                    ComponentFactory.GlobalDefaultCircuitBreakerErrorThresholdPercentage = type2.DefaultConfig.CircuitBreakerErrorThresholdPercentage;
                                }
                                if (type2.DefaultConfig.CircuitBreakerForceClosed.HasValue)
                                {
                                    ComponentFactory.GlobalDefaultCircuitBreakerForceClosed = type2.DefaultConfig.CircuitBreakerForceClosed;
                                }
                                if (type2.DefaultConfig.CircuitBreakerRequestCountThreshold.HasValue && (type2.DefaultConfig.CircuitBreakerRequestCountThreshold.Value >= 10))
                                {
                                    ComponentFactory.GlobalDefaultCircuitBreakerRequestCountThreshold = type2.DefaultConfig.CircuitBreakerRequestCountThreshold;
                                }
                                if (type2.DefaultConfig.CommandMaxConcurrentCount.HasValue && (type2.DefaultConfig.CommandMaxConcurrentCount.Value >= 50))
                                {
                                    ComponentFactory.GlobalDefaultCommandMaxConcurrentCount = type2.DefaultConfig.CommandMaxConcurrentCount;
                                }
                                if (type2.DefaultConfig.CommandTimeoutInMilliseconds.HasValue && (type2.DefaultConfig.CommandTimeoutInMilliseconds.Value >= 0x1388))
                                {
                                    ComponentFactory.GlobalDefaultCommandTimeoutInMilliseconds = type2.DefaultConfig.CommandTimeoutInMilliseconds;
                                }
                                if (type2.DefaultConfig.FallbackMaxConcurrentCount.HasValue && (type2.DefaultConfig.FallbackMaxConcurrentCount.Value >= 50))
                                {
                                    ComponentFactory.GlobalDefaultFallbackMaxConcurrentCount = type2.DefaultConfig.FallbackMaxConcurrentCount;
                                }
                            }
                            bool flag = ((type2.Application == null) || (type2.Application.Commands == null)) || (type2.Application.Commands.Count == 0);
                            using (Dictionary<string, CommandComponents>.Enumerator enumerator = HystrixCommandBase.CommandComponentsCollection.ToDictionary<KeyValuePair<string, CommandComponents>, string, CommandComponents>(p => p.Key, p => p.Value).GetEnumerator())
                            {
                                Func<CHystrixCommand, bool> predicate = null;
                                KeyValuePair<string, CommandComponents> pair;
                                while (enumerator.MoveNext())
                                {
                                    pair = enumerator.Current;
                                    ICommandConfigSet configSet = pair.Value.ConfigSet;
                                    if (flag)
                                    {
                                        configSet.RaiseConfigChangeEvent();
                                    }
                                    else
                                    {
                                        if (predicate == null)
                                        {
                                            predicate = c => string.Compare(c.Key, pair.Value.CommandInfo.CommandKey, true) == 0;
                                        }
                                        CHystrixCommand command = type2.Application.Commands.Where<CHystrixCommand>(predicate).FirstOrDefault<CHystrixCommand>();
                                        if ((command == null) || (command.Config == null))
                                        {
                                            configSet.RaiseConfigChangeEvent();
                                            continue;
                                        }
                                        if ((command.Config.CircuitBreakerErrorThresholdPercentage.HasValue && (command.Config.CircuitBreakerErrorThresholdPercentage.Value > 0)) && (command.Config.CircuitBreakerErrorThresholdPercentage.Value <= 100))
                                        {
                                            configSet.CircuitBreakerErrorThresholdPercentage = command.Config.CircuitBreakerErrorThresholdPercentage.Value;
                                        }
                                        if (command.Config.CircuitBreakerForceClosed.HasValue)
                                        {
                                            configSet.CircuitBreakerForceClosed = command.Config.CircuitBreakerForceClosed.Value;
                                        }
                                        if (command.Config.CircuitBreakerForceOpen.HasValue)
                                        {
                                            configSet.CircuitBreakerForceOpen = command.Config.CircuitBreakerForceOpen.Value;
                                        }
                                        if (command.Config.CircuitBreakerRequestCountThreshold.HasValue && (command.Config.CircuitBreakerRequestCountThreshold.Value > 0))
                                        {
                                            configSet.CircuitBreakerRequestCountThreshold = command.Config.CircuitBreakerRequestCountThreshold.Value;
                                        }
                                        if (command.Config.CommandMaxConcurrentCount.HasValue && (command.Config.CommandMaxConcurrentCount.Value > 0))
                                        {
                                            configSet.CommandMaxConcurrentCount = command.Config.CommandMaxConcurrentCount.Value;
                                        }
                                        if (command.Config.CommandTimeoutInMilliseconds.HasValue && (command.Config.CommandTimeoutInMilliseconds.Value > 0))
                                        {
                                            configSet.CommandTimeoutInMilliseconds = command.Config.CommandTimeoutInMilliseconds.Value;
                                        }
                                        if (command.Config.DegradeLogLevel.HasValue)
                                        {
                                            configSet.DegradeLogLevel = command.Config.DegradeLogLevel.Value;
                                        }
                                        if (command.Config.LogExecutionError.HasValue)
                                        {
                                            configSet.LogExecutionError = command.Config.LogExecutionError.Value;
                                        }
                                        if (command.Config.FallbackMaxConcurrentCount.HasValue && (command.Config.FallbackMaxConcurrentCount.Value > 0))
                                        {
                                            configSet.FallbackMaxConcurrentCount = command.Config.FallbackMaxConcurrentCount.Value;
                                        }
                                        if ((command.Config.MaxAsyncCommandExceedPercentage.HasValue && (command.Config.MaxAsyncCommandExceedPercentage.Value >= 0)) && (command.Config.MaxAsyncCommandExceedPercentage.Value <= 100))
                                        {
                                            configSet.MaxAsyncCommandExceedPercentage = command.Config.MaxAsyncCommandExceedPercentage.Value;
                                        }
                                        configSet.RaiseConfigChangeEvent();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                CommonUtils.Log.Log(LogLevelEnum.Warning, "Failed to sync config from config service: " + HystrixCommandBase.ConfigServiceUrl, exception, new Dictionary<string, string>().AddLogTagData("FXD303014"));
            }
        }
    }
}

