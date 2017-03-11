//
// ServiceStack.Text: .NET C# POCO JSON, JSV and CSV Text Serializers.
//
// Authors:
//   William Yang (b.yang@ctrip.com)
//
// Copyright 2012 CTrip Ltd.
//
//

using System;

namespace AntServiceStack.Text
{
	public static class SystemTime
	{
		public static Func<DateTime> UtcDateTimeResolver;

		public static DateTime Now
		{
			get
			{
				var temp = UtcDateTimeResolver;
				return temp == null ? DateTime.Now : temp().ToLocalTime();
			}
		}

		public static DateTime UtcNow
		{
			get
			{
				var temp = UtcDateTimeResolver;
				return temp == null ? DateTime.UtcNow : temp();
			}
		}
	}
}
