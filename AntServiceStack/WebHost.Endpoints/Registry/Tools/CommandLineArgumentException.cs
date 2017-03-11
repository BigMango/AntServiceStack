using System;

namespace AntServiceStack.WebHost.Endpoints.Registry.Tools
{
	public class CommandLineArgumentException : ArgumentException
	{
		public CommandLineArgumentException(string why) : base(why)
		{
		}

		public CommandLineArgumentException(string why, Exception innerException) : base(why, innerException)
		{
		}
	}
}
