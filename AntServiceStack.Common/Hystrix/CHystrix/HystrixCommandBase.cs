namespace CHystrix
{
    using CHystrix.Config;
    using CHystrix.Metrics;
    using CHystrix.Registration;
    using CHystrix.Utils;
    using CHystrix.Utils.Atomic;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    public abstract class HystrixCommandBase : ICommand
    {
        private string _commandKey;
        private Action<ICommandConfigSet> _config;
        private string _domain;
        private string _groupKey;
        private string _instanceKey;
        private string _key;
        internal static readonly ConcurrentDictionary<string, CommandComponents> CommandComponentsCollection;
        internal static readonly AtomicInteger CommandCount;
        internal const string DefaultAppName = "HystrixApp";
        internal const string DefaultGroupKey = "DefaultGroup";
        internal const int DefaultMaxCommandCount = 0x2710;
        internal readonly object ExecutionLock;
        internal static readonly ConcurrentDictionary<string, IsolationSemaphore> ExecutionSemaphores;
        internal static readonly ConcurrentDictionary<string, IsolationSemaphore> FallbackExecutionSemaphores;
        internal const string HystrixAppNameSettingKey = "CHystrix.AppName";
        internal const string HystrixConfigServiceUrlSettingKey = "CHystrix.ConfigServiceUrl";
        internal const string HystrixMaxCommandCountSettingKey = "CHystrix.MaxCommandCount";
        internal const string HystrixRegistryServiceUrlSettingKey = "CHystrix.RegistryServiceUrl";
        internal static readonly ConcurrentDictionary<Type, string> TypePredefinedKeyMappings;

        static HystrixCommandBase()
        {
            int num;
            CommandComponentsCollection = new ConcurrentDictionary<string, CommandComponents>(StringComparer.InvariantCultureIgnoreCase);
            CommandCount = new AtomicInteger();
            ExecutionSemaphores = new ConcurrentDictionary<string, IsolationSemaphore>(StringComparer.InvariantCultureIgnoreCase);
            FallbackExecutionSemaphores = new ConcurrentDictionary<string, IsolationSemaphore>(StringComparer.InvariantCultureIgnoreCase);
            TypePredefinedKeyMappings = new ConcurrentDictionary<Type, string>();
            HystrixAppName = ConfigurationManager.AppSettings["CHystrix.AppName"];
            if (string.IsNullOrWhiteSpace(HystrixAppName))
            {
                if (string.IsNullOrWhiteSpace(CommonUtils.AppId))
                {
                    string message = "Either CHystrix.AppName Or AppId must be configured.";
                    CommonUtils.Log.Log(LogLevelEnum.Fatal, message, new Dictionary<string, string>().AddLogTagData("FXD303000"));
                    HystrixAppName = "HystrixApp";
                }
                else
                {
                    HystrixAppName = CommonUtils.AppId;
                }
            }
            else
            {
                HystrixAppName = HystrixAppName.Trim();
                if (!HystrixAppName.IsValidHystrixName())
                {
                    string str2 = "HystrixAppName setting is invalid: " + HystrixAppName + @". Name pattern is: ^[a-zA-Z0-9][a-zA-Z0-9\-_.]*[a-zA-Z0-9]$";
                    CommonUtils.Log.Log(LogLevelEnum.Fatal, str2, new Dictionary<string, string>().AddLogTagData("FXD303001"));
                    throw new ArgumentException(str2);
                }
                if (CommonUtils.AppId != null)
                {
                    HystrixAppName = CommonUtils.AppId + "-" + HystrixAppName;
                }
            }
            HystrixVersion = typeof(HystrixCommandBase).Assembly.GetName().Version.ToString();
            if (!int.TryParse(ConfigurationManager.AppSettings["CHystrix.MaxCommandCount"], out num) || (num <= 0))
            {
                num = 0x2710;
            }
            MaxCommandCount = num;
            ConfigServiceUrl = ConfigurationManager.AppSettings["CHystrix.ConfigServiceUrl"];
            if (string.IsNullOrWhiteSpace(ConfigServiceUrl))
            {
                ConfigServiceUrl = null;
            }
            else
            {
                ConfigServiceUrl = ConfigServiceUrl.Trim();
            }
            RegistryServiceUrl = ConfigurationManager.AppSettings["CHystrix.RegistryServiceUrl"];
            if (string.IsNullOrWhiteSpace(RegistryServiceUrl))
            {
                RegistryServiceUrl = null;
            }
            else
            {
                RegistryServiceUrl = RegistryServiceUrl.Trim();
            }
            if (!string.IsNullOrWhiteSpace(HystrixAppName))
            {
                HystrixConfigSyncManager.Start();
                CommandConfigSyncManager.Start();
                MetricsReporter.Start();
                SelfRegistrationManager.Start();
            }
        }

        internal HystrixCommandBase() : this(null, null, null, null)
        {
        }

        internal HystrixCommandBase(string commandKey, string groupKey, string domain, Action<ICommandConfigSet> config) : this(null, commandKey, groupKey, domain, config)
        {
        }

        internal HystrixCommandBase(string instanceKey, string commandKey, string groupKey, string domain, Action<ICommandConfigSet> config)
        {
            this.ExecutionLock = new object();
            this._instanceKey = string.IsNullOrWhiteSpace(instanceKey) ? null : instanceKey.Trim();
            this._commandKey = string.IsNullOrWhiteSpace(commandKey) ? null : commandKey.Trim();
            this._groupKey = string.IsNullOrWhiteSpace(groupKey) ? "DefaultGroup" : groupKey.Trim();
            this._domain = string.IsNullOrWhiteSpace(domain) ? "Ant" : domain.Trim();
            this._config = config;
            Type type = base.GetType();
            if ((string.IsNullOrWhiteSpace(this.Key) && string.IsNullOrWhiteSpace(this.CommandKey)) && string.IsNullOrWhiteSpace(this._commandKey))
            {
                if (type.IsGenericType)
                {
                    throw new ArgumentNullException("CommandKey cannot be null.");
                }
                this._commandKey = TypePredefinedKeyMappings.GetOrAdd(type, t => CommonUtils.GenerateTypeKey(t));
            }
            this._key = CommonUtils.GenerateKey(this.InstanceKey, this.CommandKey);
            CommandComponents orAdd = CommandComponentsCollection.GetOrAdd(this.Key, key => CreateCommandComponents(key, this.InstanceKey, this.CommandKey, this.GroupKey, this.Domain, this.IsolationMode, new Action<ICommandConfigSet>(this.Config), type));
            if (orAdd.IsolationMode != this.IsolationMode)
            {
                string message = string.Concat(new object[] { "The key ", this.Key, " has been used for ", orAdd.IsolationMode, ". Now it cannot be used for ", this.IsolationMode, "." });
                CommonUtils.Log.Log(LogLevelEnum.Fatal, message, new Dictionary<string, string>().AddLogTagData("FXD303009"));
                throw new ArgumentException(message);
            }
            this.CircuitBreaker = orAdd.CircuitBreaker;
            this.Log = orAdd.Log;
            this.ConfigSet = orAdd.ConfigSet;
            this.Metrics = orAdd.Metrics;
            this._groupKey = orAdd.CommandInfo.GroupKey;
            this._commandKey = orAdd.CommandInfo.CommandKey;
            this._instanceKey = orAdd.CommandInfo.InstanceKey;
            this._domain = orAdd.CommandInfo.Domain;
        }

        protected virtual void Config(ICommandConfigSet configSet)
        {
            if (this._config != null)
            {
                this._config(configSet);
            }
        }

        public static void ConfigAsyncCommand<T>(string commandKey, string domain)
        {
            ConfigAsyncCommand<T>(commandKey, null, domain);
        }

        public static void ConfigAsyncCommand<T>(string commandKey, Action<ICommandConfigSet> configCommand)
        {
            ConfigAsyncCommand<T>(commandKey, null, configCommand);
        }

        public static void ConfigAsyncCommand<T>(string commandKey, string domain, Action<ICommandConfigSet> configCommand)
        {
            ConfigAsyncCommand<T>(commandKey, null, domain, configCommand);
        }

        public static void ConfigAsyncCommand<T>(string commandKey, string groupKey, string domain)
        {
            ConfigAsyncCommand<T>(commandKey, groupKey, domain, (Action<ICommandConfigSet>) null);
        }

        public static void ConfigAsyncCommand<T>(string commandKey, string groupKey, string domain, Action<ICommandConfigSet> configCommand)
        {
            if (string.IsNullOrWhiteSpace(commandKey))
            {
                string message = "HystrixCommand Key cannot be null.";
                CommonUtils.Log.Log(LogLevelEnum.Fatal, message, new Dictionary<string, string>().AddLogTagData("FXD303003"));
                throw new ArgumentNullException(message);
            }
            new GenericThreadIsolationCommand<T>(commandKey, groupKey, domain, () => default(T), null, configCommand);
        }

        public static void ConfigAsyncCommand<T>(string commandKey, string groupKey, string domain, int maxConcurrentCount)
        {
            int? fallbackMaxConcurrentCount = new int?(maxConcurrentCount);
            ConfigAsyncCommand<T>(commandKey, groupKey, domain, new int?(maxConcurrentCount), null, null, null, fallbackMaxConcurrentCount, null);
        }

        public static void ConfigAsyncCommand<T>(string commandKey, string groupKey, string domain, int maxConcurrentCount, int timeoutInMilliseconds)
        {
            int? fallbackMaxConcurrentCount = new int?(maxConcurrentCount);
            ConfigAsyncCommand<T>(commandKey, groupKey, domain, new int?(maxConcurrentCount), new int?(timeoutInMilliseconds), null, null, fallbackMaxConcurrentCount, null);
        }

        internal static void ConfigAsyncCommand<T>(string commandKey, string groupKey, string domain, int? maxConcurrentCount = new int?(), int? timeoutInMilliseconds = new int?(), int? circuitBreakerRequestCountThreshold = new int?(), int? circuitBreakerErrorThresholdPercentage = new int?(), int? fallbackMaxConcurrentCount = new int?(), int? maxAsyncCommandExceedPercentage = new int?())
        {
            ConfigAsyncCommand<T>(commandKey, groupKey, domain, delegate (ICommandConfigSet configSet) {
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
                if (maxAsyncCommandExceedPercentage.HasValue)
                {
                    configSet.MaxAsyncCommandExceedPercentage = maxAsyncCommandExceedPercentage.Value;
                }
            });
        }

        public static void ConfigCommand<T>(string commandKey, Action<ICommandConfigSet> configCommand)
        {
            ConfigCommand<T>(commandKey, null, configCommand);
        }

        public static void ConfigCommand<T>(string commandKey, string domain)
        {
            ConfigCommand<T>(commandKey, null, domain);
        }

        public static void ConfigCommand<T>(string commandKey, string domain, Action<ICommandConfigSet> configCommand)
        {
            ConfigCommand<T>(commandKey, null, domain, configCommand);
        }

        public static void ConfigCommand<T>(string commandKey, string groupKey, string domain)
        {
            ConfigCommand<T>(null, commandKey, groupKey, domain);
        }

        public static void ConfigCommand<T>(string commandKey, string groupKey, string domain, int maxConcurrentCount)
        {
            ConfigCommand<T>(null, commandKey, groupKey, domain, maxConcurrentCount);
        }

        public static void ConfigCommand<T>(string instanceKey, string commandKey, string groupKey, string domain)
        {
            ConfigCommand<T>(instanceKey, commandKey, groupKey, domain, (Action<ICommandConfigSet>) null);
        }

        public static void ConfigCommand<T>(string commandKey, string groupKey, string domain, Action<ICommandConfigSet> configCommand)
        {
            ConfigCommand<T>(null, commandKey, groupKey, domain, configCommand);
        }

        public static void ConfigCommand<T>(string commandKey, string groupKey, string domain, int maxConcurrentCount, int timeoutInMilliseconds)
        {
            ConfigCommand<T>(null, commandKey, groupKey, domain, maxConcurrentCount, timeoutInMilliseconds);
        }

        public static void ConfigCommand<T>(string instanceKey, string commandKey, string groupKey, string domain, Action<ICommandConfigSet> configCommand)
        {
            if (string.IsNullOrWhiteSpace(commandKey))
            {
                string message = "HystrixCommand Key cannot be null.";
                CommonUtils.Log.Log(LogLevelEnum.Fatal, message, new Dictionary<string, string>().AddLogTagData("FXD303002"));
                throw new ArgumentNullException(message);
            }
            new GenericSemaphoreIsolationCommand<T>(instanceKey, commandKey, groupKey, domain, () => default(T), null, configCommand);
        }

        public static void ConfigCommand<T>(string instanceKey, string commandKey, string groupKey, string domain, int maxConcurrentCount)
        {
            int? fallbackMaxConcurrentCount = new int?(maxConcurrentCount);
            ConfigCommand<T>(instanceKey, commandKey, groupKey, domain, new int?(maxConcurrentCount), null, null, null, fallbackMaxConcurrentCount);
        }

        public static void ConfigCommand<T>(string instanceKey, string commandKey, string groupKey, string domain, int maxConcurrentCount, int timeoutInMilliseconds)
        {
            int? fallbackMaxConcurrentCount = new int?(maxConcurrentCount);
            ConfigCommand<T>(instanceKey, commandKey, groupKey, domain, new int?(maxConcurrentCount), new int?(timeoutInMilliseconds), null, null, fallbackMaxConcurrentCount);
        }

        internal static void ConfigCommand<T>(string commandKey, string groupKey, string domain, int? maxConcurrentCount = new int?(), int? timeoutInMilliseconds = new int?(), int? circuitBreakerRequestCountThreshold = new int?(), int? circuitBreakerErrorThresholdPercentage = new int?(), int? fallbackMaxConcurrentCount = new int?())
        {
            ConfigCommand<T>(null, commandKey, groupKey, domain, maxConcurrentCount, timeoutInMilliseconds, circuitBreakerRequestCountThreshold, circuitBreakerErrorThresholdPercentage, fallbackMaxConcurrentCount);
        }

        internal static void ConfigCommand<T>(string instanceKey, string commandKey, string groupKey, string domain, int? maxConcurrentCount = new int?(), int? timeoutInMilliseconds = new int?(), int? circuitBreakerRequestCountThreshold = new int?(), int? circuitBreakerErrorThresholdPercentage = new int?(), int? fallbackMaxConcurrentCount = new int?())
        {
            ConfigCommand<T>(instanceKey, commandKey, groupKey, domain, delegate (ICommandConfigSet configSet) {
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

        internal static CommandComponents CreateCommandComponents(string key, string instanceKey, string commandKey, string groupKey, string domain, IsolationModeEnum isolationMode, Action<ICommandConfigSet> config, Type type)
        {
            if (!key.IsValidHystrixName())
            {
                string message = "Hystrix command key has invalid char: " + key + @". Name pattern is: ^[a-zA-Z0-9][a-zA-Z0-9\-_.]*[a-zA-Z0-9]$";
                CommonUtils.Log.Log(LogLevelEnum.Fatal, message, new Dictionary<string, string>().AddLogTagData("FXD303004"));
                throw new ArgumentException(message);
            }
            if (!string.IsNullOrWhiteSpace(instanceKey) && !instanceKey.IsValidHystrixName())
            {
                string str2 = "Hystrix command instanceKey has invalid char: " + instanceKey + @". Name pattern is: ^[a-zA-Z0-9][a-zA-Z0-9\-_.]*[a-zA-Z0-9]$";
                CommonUtils.Log.Log(LogLevelEnum.Fatal, str2, new Dictionary<string, string>().AddLogTagData("FXD303004"));
                throw new ArgumentException(str2);
            }
            if (!commandKey.IsValidHystrixName())
            {
                string str3 = "Hystrix command commandKey has invalid char: " + commandKey + @". Name pattern is: ^[a-zA-Z0-9][a-zA-Z0-9\-_.]*[a-zA-Z0-9]$";
                CommonUtils.Log.Log(LogLevelEnum.Fatal, str3, new Dictionary<string, string>().AddLogTagData("FXD303004"));
                throw new ArgumentException(str3);
            }
            if (!groupKey.IsValidHystrixName())
            {
                string str4 = "Hystrix command group commandKey has invalid char: " + groupKey + @". Name pattern is: ^[a-zA-Z0-9][a-zA-Z0-9\-_.]*[a-zA-Z0-9]$";
                CommonUtils.Log.Log(LogLevelEnum.Fatal, str4, new Dictionary<string, string>().AddLogTagData("FXD303005"));
                throw new ArgumentException(str4);
            }
            if (!domain.IsValidHystrixName())
            {
                string str5 = "Hystrix domain has invalid char: " + domain + @". Name pattern is: ^[a-zA-Z0-9][a-zA-Z0-9\-_.]*[a-zA-Z0-9]$";
                CommonUtils.Log.Log(LogLevelEnum.Fatal, str5, new Dictionary<string, string>().AddLogTagData("FXD303006"));
                throw new ArgumentException(str5);
            }
            if (CommandCount >= MaxCommandCount)
            {
                string str6 = "Hystrix command count has reached the limit: " + MaxCommandCount;
                CommonUtils.Log.Log(LogLevelEnum.Fatal, str6, new Dictionary<string, string>().AddLogTagData("FXD303007"));
                throw new ArgumentException(str6);
            }
            CommandCount.IncrementAndGet();
            ICommandConfigSet configSet = ComponentFactory.CreateCommandConfigSet(isolationMode);
            configSet.SubcribeConfigChangeEvent(delegate (ICommandConfigSet c) {
                OnConfigChanged(key, c);
            });
            try
            {
                if (config != null)
                {
                    config(configSet);
                }
            }
            catch (Exception exception)
            {
                CommonUtils.Log.Log(LogLevelEnum.Fatal, "Failed to config command: " + key, exception, new Dictionary<string, string>().AddLogTagData("FXD303008"));
            }
            ICommandMetrics metrics = ComponentFactory.CreateCommandMetrics(configSet, key, isolationMode);
            CommandComponents components = new CommandComponents {
                ConfigSet = configSet,
                Metrics = metrics,
                CircuitBreaker = ComponentFactory.CreateCircuitBreaker(configSet, metrics)
            };
            CommandInfo info = new CommandInfo {
                Domain = domain.ToLower(),
                GroupKey = groupKey.ToLower(),
                CommandKey = commandKey.ToLower(),
                InstanceKey = (instanceKey == null) ? null : instanceKey.ToLower(),
                Key = key.ToLower(),
                Type = isolationMode.ToString()
            };
            components.CommandInfo = info;
            components.Log = ComponentFactory.CreateLog(configSet, type);
            components.IsolationMode = isolationMode;
            return components;
        }

        internal Dictionary<string, string> GetLogTagInfo()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("Domain", this.Domain);
            dictionary.Add("Type", this.IsolationMode.ToString());
            dictionary.Add("GroupKey", this.GroupKey);
            dictionary.Add("CommandKey", this.CommandKey);
            dictionary.Add("InstanceKey", this.InstanceKey);
            dictionary.Add("Key", this.Key);
            return dictionary;
        }

        private static void OnConfigChanged(string key, ICommandConfigSet configSet)
        {
            if (!string.IsNullOrWhiteSpace(key) && (configSet != null))
            {
                IsolationSemaphore semaphore;
                IsolationSemaphore semaphore2;
                if (((configSet.CommandMaxConcurrentCount > 0) && ExecutionSemaphores.TryGetValue(key, out semaphore)) && ((semaphore != null) && (semaphore.Count != configSet.CommandMaxConcurrentCount)))
                {
                    semaphore.Count = configSet.CommandMaxConcurrentCount;
                }
                if (((configSet.FallbackMaxConcurrentCount > 0) && FallbackExecutionSemaphores.TryGetValue(key, out semaphore2)) && ((semaphore2 != null) && (semaphore2.Count != configSet.FallbackMaxConcurrentCount)))
                {
                    semaphore2.Count = configSet.FallbackMaxConcurrentCount;
                }
            }
        }

        public static void RegisterCustomBadRequestExceptionChecker(string name, Func<Exception, bool> isBadRequestExceptionDelegate)
        {
            if (string.IsNullOrWhiteSpace(name) || (isBadRequestExceptionDelegate == null))
            {
                throw new ArgumentNullException("name or delegate is null.");
            }
            CustomBadRequestExceptionChecker.BadRequestExceptionCheckers.GetOrAdd(name, isBadRequestExceptionDelegate);
        }

        internal static void Reset()
        {
            CommandComponentsCollection.Clear();
            ExecutionSemaphores.Clear();
            FallbackExecutionSemaphores.Clear();
            CommandCount.GetAndSet(0);
            CustomBadRequestExceptionChecker.BadRequestExceptionCheckers.Clear();
        }

        public static T RunCommand<T>(string commandKey, Func<T> execute)
        {
            return RunCommand<T>(null, commandKey, execute);
        }

        public static T RunCommand<T>(string instanceKey, string commandKey, Func<T> execute)
        {
            return RunCommand<T>(instanceKey, commandKey, execute, null);
        }

        public static T RunCommand<T>(string commandKey, Func<T> execute, Func<T> getFallback)
        {
            return RunCommand<T>(null, commandKey, execute, getFallback);
        }

        public static T RunCommand<T>(string instanceKey, string commandKey, Func<T> execute, Func<T> getFallback)
        {
            return RunCommand<T>(instanceKey, commandKey, null, null, execute, getFallback, null);
        }

        internal static T RunCommand<T>(string commandKey, string groupKey, string domain, Func<T> execute, Func<T> getFallback, Action<ICommandConfigSet> configCommand)
        {
            return RunCommand<T>(null, commandKey, groupKey, domain, execute, getFallback, configCommand);
        }

        internal static T RunCommand<T>(string instanceKey, string commandKey, string groupKey, string domain, Func<T> execute, Func<T> getFallback, Action<ICommandConfigSet> configCommand)
        {
            SemaphoreIsolationCommand<T> command = new GenericSemaphoreIsolationCommand<T>(instanceKey, commandKey, groupKey, domain, execute, getFallback, configCommand);
            return command.Run();
        }

        public static Task<T> RunCommandAsync<T>(string commandKey, Func<T> execute)
        {
            return RunCommandAsync<T>(commandKey, execute, null);
        }

        public static Task<T> RunCommandAsync<T>(string commandKey, Func<T> execute, Func<T> getFallback)
        {
            return RunCommandAsync<T>(commandKey, null, null, execute, getFallback, null);
        }

        internal static Task<T> RunCommandAsync<T>(string commandKey, string groupKey, string domain, Func<T> execute, Func<T> getFallback, Action<ICommandConfigSet> configCommand)
        {
            GenericThreadIsolationCommand<T> command = new GenericThreadIsolationCommand<T>(commandKey, groupKey, domain, execute, getFallback, configCommand);
            return command.RunAsync();
        }

        internal static string ApplicationPath
        {
            get;  set;
        }

        internal ICircuitBreaker CircuitBreaker { get; private set; }

        public virtual string CommandKey
        {
            get
            {
                return this._commandKey;
            }
        }

        internal static string ConfigServiceUrl
        {
            get; set;
        }
        internal ICommandConfigSet ConfigSet { get; private set; }

        internal CommandConfigSet ConfigSetForTest
        {
            get
            {
                return (this.ConfigSet as CommandConfigSet);
            }
        }

        public virtual string Domain
        {
            get
            {
                return this._domain;
            }
        }

        public virtual string GroupKey
        {
            get
            {
                return this._groupKey;
            }
        }

        internal static string HystrixAppName
        {
            get; set;
        }

        internal static string HystrixVersion
        {
            get; set;
        }

        public virtual string InstanceKey
        {
            get
            {
                return this._instanceKey;
            }
        }

        internal abstract IsolationModeEnum IsolationMode { get; }

        public virtual string Key
        {
            get
            {
                return this._key;
            }
        }

        internal ILog Log { get; private set; }

        internal static int MaxCommandCount
        {
            get; set;
        }

        internal ICommandMetrics Metrics { get; private set; }

        internal static string RegistryServiceUrl
        {
            get; set;
        }

        public virtual CommandStatusEnum Status { get; internal set; }
    }
}

