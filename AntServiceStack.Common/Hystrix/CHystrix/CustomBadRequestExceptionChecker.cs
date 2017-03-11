namespace CHystrix
{
    using CHystrix.Utils;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal static class CustomBadRequestExceptionChecker
    {
        static CustomBadRequestExceptionChecker()
        {
            BadRequestExceptionCheckers = new ConcurrentDictionary<string, Func<Exception, bool>>(StringComparer.InvariantCultureIgnoreCase);
        }

        public static bool IsBadRequestException(Exception ex)
        {
            foreach (KeyValuePair<string, Func<Exception, bool>> pair in BadRequestExceptionCheckers)
            {
                try
                {
                    if (pair.Value(ex))
                    {
                        return true;
                    }
                }
                catch (Exception exception)
                {
                    CommonUtils.Log.Log(LogLevelEnum.Warning, "Failed to check bad request exception by custom delegate: " + pair.Key, exception, new Dictionary<string, string>().AddLogTagData("FXD303047"));
                }
            }
            return false;
        }

        public static ConcurrentDictionary<string, Func<Exception, bool>> BadRequestExceptionCheckers { get; set; }
    }
}

