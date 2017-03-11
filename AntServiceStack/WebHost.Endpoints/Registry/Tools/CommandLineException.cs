using System;

namespace AntServiceStack.WebHost.Endpoints.Registry.Tools
{
	public class CommandLineException : CommandLineArgumentException
	{
		private CommandRule offendingRule;

		public CommandRule OffendingRule
		{
			get
			{
				return this.offendingRule;
			}
		}

		public CommandLineException(CommandRule violated, string why) : base(why)
		{
			this.offendingRule = violated;
		}

		public CommandLineException(CommandRule violated, string why, Exception innerException) : base(why, innerException)
		{
			this.offendingRule = violated;
		}
	}
}
