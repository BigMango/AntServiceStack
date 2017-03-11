using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Freeway.Logging;

namespace AntServiceStack.Common.CAT
{
    internal static class CatFakeTransaction
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CatFakeTransaction));

        private static readonly long BaseLine = new DateTime(1970, 1, 1, 0, 0, 0).Ticks;

        private static long DurationInMillisecond(DateTime startTime)
        {
            return (DateTime.Now.Ticks - startTime.ToLocalTime().Ticks) / TimeSpan.TicksPerMillisecond;
        }

        private static long ToUnixMillisecond(DateTime startTime)
        {
            return (startTime.ToLocalTime().Ticks - BaseLine) / TimeSpan.TicksPerMillisecond;
        }

        public static void InsertFakeTransaction(string type, string name, DateTime startTime)
        {
            InsertFakeTransaction(type, name, startTime, null);
        }

        public static void InsertFakeTransaction(string type, string name, DateTime startTime, Exception ex)
        {
            try
            {
               
            }
            catch (Exception e)
            {
                _logger.Warn(e);
            }
        }
    }
}
