using System;
using System.Text;
using System.Text.RegularExpressions;

namespace AntServiceStack.WebHost.Endpoints.Registry.Tools
{
	public abstract class CommandRule
	{
		public const string ValueOptional = ".*";

		public const string ValueRequired = ".+";

		private CommandSwitch cswitch;

		private Regex pattern;

		public CommandSwitch Cswitch
		{
			get
			{
				return this.cswitch;
			}
			set
			{
				this.cswitch = value;
			}
		}

		public Regex Pattern
		{
			get
			{
				return this.pattern;
			}
			set
			{
				this.pattern = ((value == null) ? new Regex(".*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) : value);
			}
		}

		public CommandRule() : this(null, string.Empty)
		{
		}

		public CommandRule(CommandSwitch cswitch) : this(cswitch, string.Empty)
		{
		}

		public CommandRule(Regex pattern) : this(null, pattern)
		{
		}

		public CommandRule(string patternString) : this(null, patternString)
		{
		}

		public CommandRule(CommandSwitch cswitch, string patternString)
		{
			this.cswitch = cswitch;
			string text;
			if (patternString == null || patternString == string.Empty)
			{
				text = ".*";
			}
			else
			{
				text = patternString;
			}
			this.pattern = new Regex(text, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		}

		public CommandRule(CommandSwitch cswitch, Regex pattern)
		{
			this.cswitch = cswitch;
			this.pattern = ((pattern == null) ? new Regex(".*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) : pattern);
		}

		public static string AnyOf(string[] validValues)
		{
			if (validValues == null || validValues.Length == 0)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder("^(");
			for (int i = 0; i < validValues.Length - 1; i++)
			{
				stringBuilder.Append(validValues[i]);
				stringBuilder.Append('|');
			}
			stringBuilder.Append(validValues[validValues.Length - 1]);
			stringBuilder.Append(")$");
			return stringBuilder.ToString();
		}

		internal void VerifyRule()
		{
			if (this.cswitch == null)
			{
				throw new ArgumentException(base.GetType() + " missing a CommandSwitch on the rule");
			}
		}
	}
}
