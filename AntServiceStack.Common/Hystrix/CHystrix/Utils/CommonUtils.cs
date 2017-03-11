namespace CHystrix.Utils
{
    using CHystrix;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    internal static class CommonUtils
    {
        public const string HystrixNamePattern = @"^[a-zA-Z0-9][a-zA-Z0-9\-_.]*[a-zA-Z0-9]$";
        public const long UnixEpoch = 0x89f7ff5f7b58000L;
        public static readonly DateTime UnixEpochDateTimeUtc = new DateTime(0x89f7ff5f7b58000L, DateTimeKind.Utc);

        static CommonUtils()
        {
            CommandExecutionEvents = (CommandExecutionEventEnum[]) System.Enum.GetValues(typeof(CommandExecutionEventEnum));
            CommandExecutionEventEnum[] enumArray = new CommandExecutionEventEnum[6];
            enumArray[1] = CommandExecutionEventEnum.Failed;
            enumArray[2] = CommandExecutionEventEnum.Timeout;
            enumArray[3] = CommandExecutionEventEnum.ShortCircuited;
            enumArray[4] = CommandExecutionEventEnum.Rejected;
            enumArray[5] = CommandExecutionEventEnum.BadRequest;
            CoreCommandExecutionEvents = enumArray;
            CommandExecutionEventEnum[] enumArray2 = new CommandExecutionEventEnum[4];
            enumArray2[1] = CommandExecutionEventEnum.Failed;
            enumArray2[2] = CommandExecutionEventEnum.Timeout;
            enumArray2[3] = CommandExecutionEventEnum.ShortCircuited;
            ValuableCommandExecutionEvents = enumArray2;
            CoreFailedCommandExecutionEvents = new CommandExecutionEventEnum[] { CommandExecutionEventEnum.Failed, CommandExecutionEventEnum.Timeout, CommandExecutionEventEnum.ShortCircuited, CommandExecutionEventEnum.Rejected, CommandExecutionEventEnum.BadRequest };
            AppId = ConfigurationManager.AppSettings["AppId"];
            if (string.IsNullOrWhiteSpace(AppId))
            {
                AppId = null;
            }
            else
            {
                AppId = AppId.Trim();
            }
            Log = ComponentFactory.CreateLog(typeof(CommonUtils));
            try
            {
                HostIP = (from c in Dns.GetHostAddresses(Dns.GetHostName())
                    where c.AddressFamily == AddressFamily.InterNetwork
                    select c.ToString()).FirstOrDefault<string>();
            }
            catch (Exception exception)
            {
                Log.Log(LogLevelEnum.Fatal, "Failed to get host IP.", exception);
            }
        }

        public static Dictionary<string, string> AddLogTagData(this Dictionary<string, string> tagData, string errorCode)
        {
            if (tagData == null)
            {
                tagData = new Dictionary<string, string>();
            }
            tagData["HystrixAppName"] = HystrixCommandBase.HystrixAppName;
            tagData["ErrorCode"] = errorCode;
            return tagData;
        }

        public static string GenerateKey(string instanceKey, string commandKey)
        {
            if (string.IsNullOrWhiteSpace(commandKey))
            {
                return null;
            }
            commandKey = commandKey.Trim();
            if (!string.IsNullOrWhiteSpace(instanceKey))
            {
                return (commandKey + "." + instanceKey.Trim());
            }
            return commandKey;
        }

        public static string GenerateTypeKey(Type type)
        {
            return (type.FullName + "__" + type.Assembly.GetName().Name);
        }

        public static void GetAuditData(this List<long> list, out int count, out long sum, out long min, out long max)
        {
            if (list == null)
            {
                list = new List<long>();
            }
            int num = 0;
            long num2 = 0L;
            long num3 = 0x7fffffffffffffffL;
            long num4 = -9223372036854775808L;
            foreach (long num5 in list)
            {
                num++;
                num2 += num5;
                if (num5 < num3)
                {
                    num3 = num5;
                }
                if (num5 > num4)
                {
                    num4 = num5;
                }
            }
            if (num == 0)
            {
                num3 = 0L;
                num4 = 0L;
            }
            count = num;
            sum = num2;
            min = num3;
            max = num4;
        }

        public static CommandExecutionHealthSnapshot GetHealthSnapshot(this Dictionary<CommandExecutionEventEnum, int> executionEventDistribution)
        {
            int totalCount = 0;
            int failedCount = 0;
            foreach (KeyValuePair<CommandExecutionEventEnum, int> pair in executionEventDistribution)
            {
                if (ValuableCommandExecutionEvents.Contains<CommandExecutionEventEnum>(pair.Key))
                {
                    totalCount += pair.Value;
                    if (((CommandExecutionEventEnum) pair.Key) != CommandExecutionEventEnum.Success)
                    {
                        failedCount += pair.Value;
                    }
                }
            }
            return new CommandExecutionHealthSnapshot(totalCount, failedCount);
        }

        public static long GetPercentile(this List<long> list, double percent)
        {
            return list.GetPercentile(percent, false);
        }

        public static long GetPercentile(this List<long> list, double percent, bool sorted)
        {
            if (list == null)
            {
                return 0L;
            }
            if (list.Count <= 0)
            {
                return 0L;
            }
            if (!sorted)
            {
                list.Sort();
            }
            if (percent <= 0.0)
            {
                return list[0];
            }
            if (percent >= 100.0)
            {
                return list[list.Count - 1];
            }
            int num = (int) ((percent * (list.Count - 1)) / 100.0);
            return list[num];
        }

        public static bool IsBadRequestException(this Exception ex)
        {
            if (ex == null)
            {
                return false;
            }
            return ((ex is BadRequestException) || CustomBadRequestExceptionChecker.IsBadRequestException(ex));
        }

        public static bool IsValidHystrixName(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }
            return Regex.IsMatch(name, @"^[a-zA-Z0-9][a-zA-Z0-9\-_.]*[a-zA-Z0-9]$");
        }

        public static void RaiseConfigChangeEvent(this ICommandConfigSet configSet)
        {
            IConfigChangeEvent event2 = configSet as IConfigChangeEvent;
            if (event2 != null)
            {
                event2.RaiseConfigChangeEvent();
            }
        }

        public static void SubcribeConfigChangeEvent(this ICommandConfigSet configSet, HandleConfigChangeDelegate handleConfigChange)
        {
            IConfigChangeEvent event2 = configSet as IConfigChangeEvent;
            if (event2 != null)
            {
                event2.OnConfigChanged += handleConfigChange;
            }
        }

        public static string AppId
        {
            get; set;
        }

        public static CommandExecutionEventEnum[] CommandExecutionEvents
        {
            get; set;
        }

        public static CommandExecutionEventEnum[] CoreCommandExecutionEvents
        {
            get; set;
        }

        public static CommandExecutionEventEnum[] CoreFailedCommandExecutionEvents
        {
            get; set;
        }

        public static long CurrentTimeInMiliseconds
        {
            get
            {
                return (DateTime.Now.Ticks / 0x2710L);
            }
        }

        public static long CurrentUnixTimeInMilliseconds
        {
            get
            {
                DateTime now = DateTime.Now;
                DateTime time2 = DateTime.Now;
                if (time2.Kind != DateTimeKind.Utc)
                {
                    time2 = ((now.Kind == DateTimeKind.Unspecified) && (now > DateTime.MinValue)) ? DateTime.SpecifyKind(now.Subtract(TimeZoneInfo.Local.GetUtcOffset(now)), DateTimeKind.Utc) : TimeZoneInfo.ConvertTimeToUtc(now);
                }
                return (long) time2.Subtract(UnixEpochDateTimeUtc).TotalMilliseconds;
            }
        }

        public static string HostIP
        {
            get; set;
        }

        public static ILog Log
        {
            get; set;
        }

        public static CommandExecutionEventEnum[] ValuableCommandExecutionEvents
        {
            get; set;
        }
    }
}

