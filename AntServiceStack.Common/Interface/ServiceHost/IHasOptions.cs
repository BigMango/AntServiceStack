using System.Collections.Generic;

namespace AntServiceStack.ServiceHost
{
	public interface IHasOptions
	{
		IDictionary<string, string> Options { get; }
	}
}