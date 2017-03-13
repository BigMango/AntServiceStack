

using System.Collections.Generic;
using System.Text;
using AntServiceStack.Text.Common;

namespace AntServiceStack.Text
{
	public static class MapExtensions
	{
		public static string Join<K, V>(this Dictionary<K, V> values)
		{
			return Join(values, JsWriter.ItemSeperatorString, JsWriter.MapKeySeperatorString);
		}

		public static string Join<K, V>(this Dictionary<K, V> values, string itemSeperator, string keySeperator)
		{
			var sb = new StringBuilder();
			foreach (var entry in values)
			{
				if (sb.Length > 0)
					sb.Append(itemSeperator);

				sb.Append(entry.Key).Append(keySeperator).Append(entry.Value);
			}
			return sb.ToString();
		}
	}
}