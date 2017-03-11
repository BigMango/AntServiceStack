namespace CHystrix.Registration
{
    using CHystrix;
    using CHystrix.Utils;
    using CHystrix.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Timers;

    internal class SelfRegistrationManager
    {
        private static object _lock = new object();
        private static System.Timers.Timer _timer;
        public const int RegistrationIntervalMilliseconds = 0x124f80;
        private const string RegistryServiceOperationName = "RegisterApp";

        private static List<RegisterCommandInfo> getRegisterCommandInfos()
        {
            Dictionary<string, RegisterCommandInfo> dictionary = new Dictionary<string, RegisterCommandInfo>(StringComparer.InvariantCultureIgnoreCase);
            foreach (CommandComponents components in HystrixCommandBase.CommandComponentsCollection.Values)
            {
                RegisterCommandInfo info2;
                CommandInfo commandInfo = components.CommandInfo;
                dictionary.TryGetValue(commandInfo.CommandKey, out info2);
                if (info2 == null)
                {
                    info2 = new RegisterCommandInfo {
                        Key = commandInfo.CommandKey,
                        Domain = commandInfo.Domain,
                        GroupKey = commandInfo.GroupKey,
                        InstanceKeys = new List<string>(),
                        Type = commandInfo.Type
                    };
                    dictionary[commandInfo.CommandKey] = info2;
                }
                if (commandInfo.InstanceKey != null)
                {
                    info2.InstanceKeys.Add(commandInfo.InstanceKey);
                }
            }
            return dictionary.Values.ToList<RegisterCommandInfo>();
        }

        private static void RegisterData(object sender, ElapsedEventArgs arg)
        {
            try
            {
                if (Monitor.TryEnter(_lock))
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(HystrixCommandBase.RegistryServiceUrl))
                        {
                            CommonUtils.Log.Log(LogLevelEnum.Warning, "Hystrix Registry Url is empty.", new Dictionary<string, string>().AddLogTagData("FXD303043"));
                        }
                        else
                        {
                            string url = HystrixCommandBase.RegistryServiceUrl.WithTrailingSlash() + "RegisterApp.json";
                            RegisterAppRequest request = new RegisterAppRequest {
                                ApplicationPath = HystrixCommandBase.ApplicationPath,
                                AppName = HystrixCommandBase.HystrixAppName,
                                HostIP = CommonUtils.HostIP,
                                HystrixVersion = HystrixCommandBase.HystrixVersion,
                                HystrixCommands = getRegisterCommandInfos()
                            };
                            url.PostJsonToUrl(request.ToJson(), null, null);
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_lock);
                    }
                }
            }
            catch (Exception exception)
            {
                CommonUtils.Log.Log(LogLevelEnum.Warning, "Failed to register data to Hystrix Registry Service: " + HystrixCommandBase.RegistryServiceUrl, exception, new Dictionary<string, string>().AddLogTagData("FXD303044"));
            }
        }

        public static void Start()
        {
            System.Timers.Timer timer = new System.Timers.Timer {
                Interval = 1200000.0,
                AutoReset = true,
                Enabled = true
            };
            _timer = timer;
            _timer.Elapsed += new ElapsedEventHandler(SelfRegistrationManager.RegisterData);
        }

        public static void Stop()
        {
            try
            {
                if (_timer != null)
                {
                    using (_timer)
                    {
                        _timer.Stop();
                    }
                }
            }
            catch
            {
            }
        }
    }
}

