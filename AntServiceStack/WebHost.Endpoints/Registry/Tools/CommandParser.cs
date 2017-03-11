using System;
using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Collections.Generic;

namespace AntServiceStack.WebHost.Endpoints.Registry.Tools
{
	public class CommandParser
	{
		private const int SCREENWIDTH = 80;

		public static bool AssignUnknowns(ArgumentDictionary arguments, AssignmentRule[] rules)
		{
			for (int i = 0; i < rules.Length; i++)
			{
				AssignmentRule assignmentRule = rules[i];
				if (assignmentRule.AssignDefaults(arguments))
				{
					return true;
				}
			}
			return false;
		}

		public static void CheckRules(ArgumentDictionary arguments, CheckingRule[] rules)
		{
			for (int i = 0; i < rules.Length; i++)
			{
				CheckingRule checkingRule = rules[i];
				checkingRule.CheckRule(arguments);
			}
		}

        public static ArgumentDictionary ParseCommand(string cmdLine, CommandSwitch[] switches)
        {
            cmdLine = cmdLine.Trim();
            if (string.IsNullOrWhiteSpace(cmdLine))
                return new ArgumentDictionary();

            List<string> cmd = new List<string>();
            StringBuilder builder = new StringBuilder();
            bool hasQuote = false;
            for (int i = 0; i < cmdLine.Length; i++)
            {
                char current = cmdLine[i];
                if (current == ' ' && !hasQuote)
                {
                    cmd.Add(builder.ToString());
                    builder.Clear();
                    hasQuote = false;
                }
                else
                {
                    if (current == '"')
                        hasQuote = !hasQuote;
                    builder.Append(current);
                }
            }
            cmd.Add(builder.ToString());
            return ParseCommand(cmd.ToArray(), switches);
        }

		public static ArgumentDictionary ParseCommand(string[] cmd, CommandSwitch[] switches)
		{
			return CommandParser.ParseCommand(cmd, switches, null);
		}

		public static ArgumentDictionary ParseCommand(string[] cmd, CommandSwitch[] switches, ResourceManager resources)
		{
			ArgumentDictionary argumentDictionary = new ArgumentDictionary(cmd.Length);
			for (int i = 0; i < cmd.Length; i++)
			{
				string text = cmd[i];
				string text2 = text;
				if (text2[0] != '/' && text2[0] != '-')
				{
					argumentDictionary.Add(string.Empty, text2);
				}
				else
				{
					if (text2.Length == 1)
					{
						throw new CommandLineArgumentException(CommandParser.GetLocalizedText(resources, "CommandArgumentsMissingSwitch", "Invalid argument: switch missing."));
					}
					text2 = text2.Substring(1);
					int num = text2.IndexOfAny(new char[]
					{
						':',
						'='
					});
					if (num == 0)
					{
						throw new CommandLineArgumentException(CommandParser.GetLocalizedText(resources, "CommandArgumentsSyntax", "Invalid argument: delimeter (':' or '=') may not start switch."));
					}
					string value;
					if (num == -1)
					{
						value = string.Empty;
					}
					else
					{
						value = text2.Substring(num + 1);
						text2 = text2.Substring(0, num);
					}
					CommandSwitch commandSwitch = CommandSwitch.FindSwitch(text2.ToLower(CultureInfo.InvariantCulture), switches);
					if (commandSwitch == null)
					{
						string text3 = text2.ToLower(CultureInfo.InvariantCulture);
						throw new CommandLineArgumentException(CommandParser.GetLocalizedText(resources, "CommandArgumentsUnknownSwitch", new string[]
						{
							text3
						}, string.Format("Switch /{0} is an unknown switch", new string[]
						{
							text3
						})));
					}
					argumentDictionary.Add(commandSwitch.Name, value);
				}
			}
			return argumentDictionary;
		}

		public static string PrintHelp(CheckingRule[] rules, CommandSwitch[] switches, bool print)
		{
			StringBuilder stringBuilder = new StringBuilder("\nMore help on command-line options:\n\n");
			Hashtable hashtable = new Hashtable();
			for (int i = 0; i < rules.Length; i++)
			{
				CheckingRule checkingRule = rules[i];
				if (!hashtable.Contains(checkingRule.Cswitch))
				{
					IList list = new ArrayList();
					list.Add(checkingRule);
					hashtable.Add(checkingRule.Cswitch, list);
				}
				else
				{
					IList list2 = (IList)hashtable[checkingRule.Cswitch];
					list2.Add(checkingRule);
				}
			}
			for (int j = 0; j < switches.Length; j++)
			{
				CommandSwitch commandSwitch = switches[j];
				StringBuilder stringBuilder2 = new StringBuilder("/");
				stringBuilder2.Append(commandSwitch.Name);
				string value = string.Empty;
				if (commandSwitch.ValueName != string.Empty)
				{
					value = ":<" + commandSwitch.ValueName + ">";
					stringBuilder2.Append(value);
				}
				StringBuilder stringBuilder3 = new StringBuilder();
				if (commandSwitch.Name != commandSwitch.Abbreviation)
				{
					stringBuilder3.Append(" [Short form: /");
					stringBuilder3.Append(commandSwitch.Abbreviation);
					stringBuilder3.Append(value);
					stringBuilder3.Append(']');
				}
				string value2 = CommandParser.WrapLine(commandSwitch.Description, 80);
				char value3 = (stringBuilder2.Length + stringBuilder3.Length + 1 > 80) ? '\n' : ' ';
				stringBuilder2.Append(value3);
				stringBuilder2.Append(stringBuilder3.ToString());
				stringBuilder2.Append('\n');
				stringBuilder2.Append(value2);
				stringBuilder2.Append('\n');
				stringBuilder.Append(stringBuilder2.ToString());
				stringBuilder.Append('\n');
			}
			if (print)
			{
				Console.WriteLine(stringBuilder.ToString());
			}
			return stringBuilder.ToString();
		}

		internal static string GetLocalizedText(ResourceManager resources, string name, string english)
		{
			return CommandParser.GetLocalizedText(resources, name, new string[0], english);
		}

		internal static string GetLocalizedText(ResourceManager resources, string name, string[] args, string english)
		{
			if (resources == null)
			{
				return english;
			}
			string @string = resources.GetString(name);
			if (@string == null)
			{
				return english;
			}
			return string.Format(@string, args);
		}

		internal static string WrapLine(string text, int screenWidth)
		{
			string[] array = text.Split(null);
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string text2 = array2[i];
				int length = text2.Length;
				if (num + length + 1 >= 80)
				{
					num = length + 1;
					stringBuilder.Append("\n" + text2);
				}
				else
				{
					num += length + 1;
					stringBuilder.Append(text2);
				}
				stringBuilder.Append(' ');
			}
			return stringBuilder.ToString();
		}
	}
}
