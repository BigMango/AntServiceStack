using System;

namespace AntServiceStack.ServiceHost
{
	public interface IRequestAttributes
	{
		bool AcceptsGzip { get; }

		bool AcceptsDeflate { get; }
	}
}