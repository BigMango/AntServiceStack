using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AntServiceStack.Common.Configuration;
using AntServiceStack.Common.Hystrix;
using AntServiceStack.Common.Hystrix.Strategy.Properties;

namespace AntServiceStack
{
    internal static class HystrixCommandHelper
    {
        const string FxConfigWebServiceSOAServiceCircuitBreakerTimeoutSettingKey = "SOA_ServiceCircuitBreaker_Timeout";
        public static readonly TimeSpan MinGlobalDefaultCircuitBreakerTimeoutSetting = TimeSpan.FromMilliseconds(5000);
        public static TimeSpan? GlobalDefaultCircuitBreakerTimeoutSetting;

        public static TimeSpan GetExecutionTimeout(this HystrixCommand command)
        {
            if (command.Properties.ExecutionIsolationThreadTimeout.Get().HasValue)
                return command.Properties.ExecutionIsolationThreadTimeout.Get().Value;

            if (GlobalDefaultCircuitBreakerTimeoutSetting.HasValue)
                return GlobalDefaultCircuitBreakerTimeoutSetting.Value;

            return HystrixPropertiesCommandDefault.DefaultExecutionIsolationThreadTimeout;
        }

        public static void SyncGlobalSetting()
        {
            try
            {
                string value = AntFxConfigWebServiceUtils.GetConfigItemValue(FxConfigWebServiceSOAServiceCircuitBreakerTimeoutSettingKey);
                int timeout = 0;
                int.TryParse(value, out timeout);
                TimeSpan globalTimeout = TimeSpan.FromMilliseconds(timeout);
                if (globalTimeout >= MinGlobalDefaultCircuitBreakerTimeoutSetting)
                    GlobalDefaultCircuitBreakerTimeoutSetting = globalTimeout;
            }
            catch
            {
            }
        }
    }
}
