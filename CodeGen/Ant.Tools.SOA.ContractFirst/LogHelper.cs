using System;
using System.IO;
using System.Text;

namespace CTrip.Tools.SOA.ContractFirst
{
	/// <summary>
	/// Summary description for LogHelper.
	/// </summary>
	public class LogHelper
	{
		public static void LogToFile(string filename, string message)
		{
			StreamWriter sw = new StreamWriter(filename + "_" + DateTime.Now.ToLongTimeString().Replace(":", ".") + ".log", true, Encoding.UTF8);
			sw.WriteLine(message);
			sw.Flush();
			sw.Close();
		}
	}
}
