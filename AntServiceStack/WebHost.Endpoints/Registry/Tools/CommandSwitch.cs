using System;
using System.Globalization;

namespace AntServiceStack.WebHost.Endpoints.Registry.Tools
{
	public class CommandSwitch
	{
		private string name;

		private string abbreviation;

		private string description;

		private string valueName;

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public string Abbreviation
		{
			get
			{
				return this.abbreviation;
			}
		}

		public string Description
		{
			get
			{
				return this.description;
			}
			set
			{
				this.description = ((value == null) ? string.Empty : value);
			}
		}

		public string ValueName
		{
			get
			{
				return this.valueName;
			}
			set
			{
				this.valueName = ((value == null) ? string.Empty : value);
			}
		}

		public CommandSwitch(string name, string abbreviation) : this(name, abbreviation, string.Empty, string.Empty)
		{
		}

		public CommandSwitch(string name, string abbreviation, string description) : this(name, abbreviation, description, string.Empty)
		{
		}

		public CommandSwitch(string name, string abbreviation, string description, string valueName)
		{
			if (name[0] == '/' || name[0] == '-')
			{
				this.name = name.Substring(1).ToLower(CultureInfo.InvariantCulture);
			}
			else
			{
				this.name = name.ToLower(CultureInfo.InvariantCulture);
			}
			if (abbreviation[0] == '/' || abbreviation[0] == '-')
			{
				this.abbreviation = abbreviation.Substring(1).ToLower(CultureInfo.InvariantCulture);
			}
			else
			{
				this.abbreviation = abbreviation.ToLower(CultureInfo.InvariantCulture);
			}
			this.description = ((description == null) ? string.Empty : description);
			this.valueName = ((valueName == null) ? string.Empty : valueName);
		}

		public bool Equals(string compare)
		{
			string value;
			if (compare[0] == '/' || compare[0] == '-')
			{
				value = compare.Substring(1).ToLower(CultureInfo.InvariantCulture);
			}
			else
			{
				value = compare.ToLower(CultureInfo.InvariantCulture);
			}
			return this.name.Equals(value) || this.abbreviation.Equals(value);
		}

		public string FancyName()
		{
			string result;
			if (this.name.Length != this.abbreviation.Length)
			{
				result = string.Concat(new string[]
				{
					"/",
					this.abbreviation,
					"[",
					this.name.Substring(this.abbreviation.Length),
					"]"
				});
			}
			else
			{
				result = "/" + this.name;
			}
			return result;
		}

		public static CommandSwitch FindSwitch(string name, CommandSwitch[] switches)
		{
			for (int i = 0; i < switches.Length; i++)
			{
				CommandSwitch commandSwitch = switches[i];
				if (commandSwitch.Equals(name))
				{
					return commandSwitch;
				}
			}
			return null;
		}
	}
}
