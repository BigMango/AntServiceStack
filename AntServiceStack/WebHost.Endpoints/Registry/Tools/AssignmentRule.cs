using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace AntServiceStack.WebHost.Endpoints.Registry.Tools
{
	public class AssignmentRule : CommandRule
	{
		public AssignmentRule()
		{
		}

		public AssignmentRule(CommandSwitch cswitch) : base(cswitch)
		{
		}

		public AssignmentRule(Regex pattern) : base(pattern)
		{
		}

		public AssignmentRule(string patternString) : base(patternString)
		{
		}

		public AssignmentRule(CommandSwitch cswitch, Regex pattern) : base(cswitch, pattern)
		{
		}

		public AssignmentRule(CommandSwitch cswitch, string patternString) : base(cswitch, patternString)
		{
		}

		public bool AssignDefaults(ArgumentDictionary options)
		{
			base.VerifyRule();
			bool flag = false;
			StringCollection arguments = options.GetArguments(string.Empty);
			StringCollection stringCollection = new StringCollection();
			if (arguments == null)
			{
				return true;
			}
			foreach (string current in arguments)
			{
				if (base.Pattern.IsMatch(current))
				{
					options.Add(base.Cswitch.Name, current);
					stringCollection.Add(current);
				}
				else
				{
					flag = true;
				}
			}
			foreach (string current2 in stringCollection)
			{
				arguments.Remove(current2);
			}
			return !flag;
		}
	}
}
