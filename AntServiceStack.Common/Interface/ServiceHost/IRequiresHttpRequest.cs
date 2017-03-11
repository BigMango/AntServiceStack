using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.ServiceHost
{
	public interface IRequiresHttpRequest
	{
		IHttpRequest HttpRequest { get; set; }
	}
}
