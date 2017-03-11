using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace AntServiceStack.WebHost.Endpoints.Registry.Tools
{
	public class CheckingRule : CommandRule
	{
		private bool multipleValuesAllowed;

		private bool switchRequired;

		public bool MultipleValuesAllowed
		{
			get
			{
				return this.multipleValuesAllowed;
			}
			set
			{
				this.multipleValuesAllowed = value;
			}
		}

		public bool SwitchRequired
		{
			get
			{
				return this.switchRequired;
			}
			set
			{
				this.switchRequired = value;
			}
		}

		public CheckingRule() : this(null, string.Empty, true, false)
		{
		}

		public CheckingRule(CommandSwitch cswitch) : this(cswitch, string.Empty, true, false)
		{
		}

		public CheckingRule(Regex pattern) : this(null, pattern, true, false)
		{
		}

		public CheckingRule(string patternString) : this(null, patternString, true, false)
		{
		}

		public CheckingRule(CommandSwitch cswitch, Regex pattern) : this(cswitch, pattern, true, false)
		{
		}

		public CheckingRule(CommandSwitch cswitch, string patternString) : this(cswitch, patternString, true, false)
		{
		}

		public CheckingRule(CommandSwitch cswitch, Regex pattern, bool switchRequired) : this(cswitch, pattern, switchRequired, false)
		{
		}

		public CheckingRule(CommandSwitch cswitch, string patternString, bool switchRequired) : this(cswitch, patternString, switchRequired, false)
		{
		}

		public CheckingRule(CommandSwitch cswitch, Regex pattern, bool switchRequired, bool multipleValuesAllowed) : base(cswitch, pattern)
		{
			this.switchRequired = switchRequired;
			this.multipleValuesAllowed = multipleValuesAllowed;
		}

		public CheckingRule(CommandSwitch cswitch, string patternString, bool switchRequired, bool multipleValuesAllowed) : base(cswitch, patternString)
		{
			this.switchRequired = switchRequired;
			this.multipleValuesAllowed = multipleValuesAllowed;
		}

		public void CheckRule(ArgumentDictionary options)
		{
			base.VerifyRule();
			string name = base.Cswitch.Name;
			if (!options.Contains(name))
			{
				if (this.switchRequired)
				{
					throw new CommandLineException(this, "Switch \"" + name + "\" required but not provided");
				}
				return;
			}
			else
			{
				StringCollection arguments = options.GetArguments(name);
				if (base.Pattern.ToString() != ".*" && arguments.Count == 0)
				{
					throw new CommandLineException(this, "Value[s] required for switch \"" + name + "\"");
				}
				if (arguments.Count > 1 && !this.multipleValuesAllowed)
				{
					throw new CommandLineException(this, "Switch \"" + name + "\" may not be assigned multiple values");
				}
				foreach (string current in arguments)
				{
					if (!base.Pattern.IsMatch(current))
					{
						throw new CommandLineException(this, string.Concat(new string[]
						{
							"Value \"",
							current,
							"\" for switch \"",
							name,
							"\" was invalid"
						}));
					}
				}
				return;
			}
		}
	}
}
