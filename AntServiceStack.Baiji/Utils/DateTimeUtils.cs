
using System;
using System.Globalization;

namespace AntServiceStack.Baiji.Utils
{
    public static class DateTimeUtils
    {
        private const string Prefix = "/Date(";
        private const string Suffix = ")/";
        private const string UnspecifiedOffset = "-0000";
        private const long minValueUnixTime = -62135596800000;
        private const long maxValueUnixTime = 253402300799000;

        private static readonly char[] TimeZoneChars = new[] { '+', '-' };
        private static readonly System.DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

        public static long GetTimeInterval(DateTime value)
        {
            if (value == DateTime.MinValue)
                return minValueUnixTime;
            if (value == DateTime.MaxValue)
                return maxValueUnixTime;
            return (long)value.ToStableTime().ToUniversalTime().Subtract(EPOCH).TotalMilliseconds;
        }

        public static long GetTimeIntervalLongValue(DateTime value)
        {
            if (value == DateTime.MinValue)
                return minValueUnixTime;
            if (value == DateTime.MaxValue)
                return maxValueUnixTime;
            return (long)value.ToLocalStableTime().ToUniversalTime().Subtract(EPOCH).TotalMilliseconds;
        }

        public static DateTime GetDateFromTimeInterval(long value, long offsetTotalMinutes)
        {
            TimeSpan offset = new TimeSpan((int)offsetTotalMinutes / 60, (int)offsetTotalMinutes % 60, 0);
            var utcDate = GetUtcDateFromTimeIntervalLongValue(value);
            if (offset.Ticks == TimeZoneInfo.Local.GetUtcOffset(utcDate).Ticks)
            {
                return utcDate.ToLocalTime();
            }
            return new DateTimeOffset(utcDate.Add(offset).Ticks, offset).DateTime;
        }

        public static string GetTimeIntervalString(DateTime value)
        {
            string utcValue = GetTimeIntervalLongValue(value).ToString();
            string offset = string.Empty;
            switch (value.Kind)
            {
                case DateTimeKind.Unspecified:
                    offset = UnspecifiedOffset; break;
                case DateTimeKind.Local:
                    offset = TimeZoneInfo.Local.GetUtcOffset(value).ToTimeOffsetString(); break;
            }
            return Prefix + utcValue + offset + Suffix;
        }

        public static DateTime GetDateFromTimeIntervalString(string value)
        {
            var suffixPos = value.IndexOf(Suffix);
            var timeString = value.Substring(Prefix.Length, suffixPos - Prefix.Length);

            var timeZonePos = timeString.LastIndexOfAny(TimeZoneChars);
            var timeZone = timeZonePos <= 0 ? string.Empty : timeString.Substring(timeZonePos);
            var unixTime = long.Parse(timeString.Substring(0, timeString.Length - timeZone.Length));
            var utcDate = GetUtcDateFromTimeIntervalLongValue(unixTime);
            if (timeZone == string.Empty)
            {
                return utcDate.ToLocalTime();
            }
            if (timeZone.Equals(UnspecifiedOffset))
            {
                if (minValueUnixTime == unixTime)
                    return DateTime.MinValue;
                if (maxValueUnixTime == unixTime)
                    return DateTime.MaxValue;
            }
            var offset = timeZone.FromTimeOffsetString();
            if (offset.Ticks == TimeZoneInfo.Local.GetUtcOffset(utcDate).Ticks)
            {
                return utcDate.ToLocalTime();
            }
            utcDate = new DateTime(utcDate.Add(offset).Ticks, DateTimeKind.Utc);
            return new DateTime(utcDate.ToLocalTime().Ticks, DateTimeKind.Unspecified);
        }

        public static long GetTimeOffsetTotalMinutes(DateTime value)
        {
            switch (value.Kind)
            {
                case DateTimeKind.Utc:
                case DateTimeKind.Unspecified:
                    return 0;
            }
            var offset = new DateTimeOffset(value).Offset;
            return (long)offset.TotalMinutes;
        }

        public static DateTime ToStableTime(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
                return new DateTime(dateTime.Ticks, DateTimeKind.Utc);
            return dateTime;
        }

        public static DateTime ToLocalStableTime(this DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Local)
                return new DateTime(dateTime.Ticks, DateTimeKind.Local);
            return dateTime;
        }

        public static string ToTimeOffsetString(this TimeSpan offset, string seperator = "")
        {
            var hours = Math.Abs(offset.Hours).ToString(CultureInfo.InvariantCulture);
            var minutes = Math.Abs(offset.Minutes).ToString(CultureInfo.InvariantCulture);
            return (offset < TimeSpan.Zero ? "-" : "+")
                + (hours.Length == 1 ? "0" + hours : hours)
                + seperator
                + (minutes.Length == 1 ? "0" + minutes : minutes);
        }

        public static TimeSpan FromTimeOffsetString(this string offsetString)
        {
            if (!offsetString.Contains(":"))
                offsetString = offsetString.Insert(offsetString.Length - 2, ":");

            offsetString = offsetString.TrimStart('+');

            return TimeSpan.Parse(offsetString);
        }

        private static DateTime GetUtcDateFromTimeIntervalLongValue(long value)
        {
            if (minValueUnixTime == value)
                return DateTime.MinValue;
            if (maxValueUnixTime == value)
                return DateTime.MaxValue;
            return EPOCH.AddMilliseconds(value);
        }
    }
}