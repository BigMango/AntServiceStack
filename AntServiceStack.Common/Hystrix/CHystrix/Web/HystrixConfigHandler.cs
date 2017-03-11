namespace CHystrix.Web
{
    using CHystrix;
    using CHystrix.Config;
    using CHystrix.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    internal class HystrixConfigHandler : IHttpHandler
    {
        public const string OperationName = "_config";

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                Dictionary<string, CommandComponents> dictionary = HystrixCommandBase.CommandComponentsCollection.ToDictionary<KeyValuePair<string, CommandComponents>, string, CommandComponents>(p => p.Key, p => p.Value);
                CHystrixConfigInfo info = new CHystrixConfigInfo {
                    ApplicationPath = HystrixCommandBase.ApplicationPath,
                    CHystrixAppName = HystrixCommandBase.HystrixAppName,
                    CHystrixVersion = HystrixCommandBase.HystrixVersion,
                    MaxCommandCount = HystrixCommandBase.MaxCommandCount,
                    CommandCount = dictionary.Count,
                    ConfigWebServiceUrl = HystrixConfigSyncManager.ConfigWebServiceUrl,
                    SOARegistryServiceUrl = HystrixConfigSyncManager.SOARegistryServiceUrl,
                    CHystrixConfigServiceUrl = HystrixCommandBase.ConfigServiceUrl,
                    CHystrixRegistryServiceUrl = HystrixCommandBase.RegistryServiceUrl,
                    SyncCHystrixConfigIntervalMilliseconds = 0x927c0,
                    SyncCommandConfigIntervalMilliseconds = 0x7530,
                    SelfRegistrationIntervalMilliseconds = 0x124f80,
                    FrameworkDefaultCircuitBreakerRequestCountThreshold = 20,
                    FrameworkDefaultCircuitBreakerErrorThresholdPercentage = 50,
                    FrameworkDefaultCircuitBreakerForceClosed = false,
                    FrameworkDefaultCommandTimeoutInMilliseconds = 0x7530,
                    FrameworkDefaultSemaphoreIsolationMaxConcurrentCount = 100,
                    FrameworkDefaultThreadIsolationMaxConcurrentCount = 20,
                    MinGlobalDefaultCircuitBreakerRequestCountThreshold = 10,
                    MinGlobalDefaultCircuitBreakerErrorThresholdPercentage = 20,
                    MinGlobalDefaultCommandTimeoutInMilliseconds = 0x1388,
                    MinGlobalDefaultCommandMaxConcurrentCount = 50,
                    MinGlobalDefaultFallbackMaxConcurrentCount = 50,
                    GlobalDefaultCircuitBreakerRequestCountThreshold = ComponentFactory.GlobalDefaultCircuitBreakerRequestCountThreshold,
                    GlobalDefaultCircuitBreakerErrorThresholdPercentage = ComponentFactory.GlobalDefaultCircuitBreakerErrorThresholdPercentage,
                    GlobalDefaultCircuitBreakerForceClosed = ComponentFactory.GlobalDefaultCircuitBreakerForceClosed,
                    GlobalDefaultCommandTimeoutInMilliseconds = ComponentFactory.GlobalDefaultCommandTimeoutInMilliseconds,
                    GlobalDefaultCommandMaxConcurrentCount = ComponentFactory.GlobalDefaultCommandMaxConcurrentCount,
                    GlobalDefaultFallbackMaxConcurrentCount = ComponentFactory.GlobalDefaultFallbackMaxConcurrentCount,
                    DefaultCircuitBreakerRequestCountThreshold = ComponentFactory.DefaultCircuitBreakerRequestCountThreshold,
                    DefaultCircuitBreakerErrorThresholdPercentage = ComponentFactory.DefaultCircuitBreakerErrorThresholdPercentage,
                    DefaultCircuitBreakerForceClosed = ComponentFactory.DefaultCircuitBreakerForceClosed,
                    DefaultCommandTimeoutInMilliseconds = ComponentFactory.DefaultCommandTimeoutInMilliseconds,
                    DefaultSemaphoreIsolationMaxConcurrentCount = ComponentFactory.DefaultSemaphoreIsolationMaxConcurrentCount,
                    DefaultThreadIsolationMaxConcurrentCount = ComponentFactory.DefaultThreadIsolationMaxConcurrentCount,
                    CommandConfigInfoList = new List<CommandConfigInfo>()
                };
                foreach (CommandComponents components in dictionary.Values)
                {
                    CommandConfigInfo item = new CommandConfigInfo {
                        CommandKey = components.CommandInfo.CommandKey,
                        GroupKey = components.CommandInfo.GroupKey,
                        Domain = components.CommandInfo.Domain,
                        Type = components.CommandInfo.Type,
                        ConfigSet = components.ConfigSet as CommandConfigSet
                    };
                    info.CommandConfigInfoList.Add(item);
                }
                info.CommandConfigInfoList = info.CommandConfigInfoList.Distinct<CommandConfigInfo>().ToList<CommandConfigInfo>();
                context.Response.ContentType = "application/json";
                context.Response.Write(info.ToJson());
            }
            catch (Exception exception)
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write(exception.Message);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}

