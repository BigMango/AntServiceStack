using System;
using System.Collections.Generic;

namespace AntServiceStack.ServiceHost
{
	/// <summary>
	/// Responsible for executing the operation within the specified context.
	/// </summary>
	/// <value>The operation types.</value>
	public interface IServiceController
	{
		/// <summary>
		/// Returns the first matching RestPath
		/// </summary>
		/// <param name="httpMethod"></param>
		/// <param name="pathInfo"></param>
		/// <returns></returns>
		IRestPath GetRestPathForRequest(string httpMethod, string servicePath, string pathInfo);

		/// <summary>
		/// Executes the Service request under the supplied requestContext.
		/// </summary>
        /// <param name="operationName"></param>
		/// <param name="request"></param>
		/// <param name="requestContext"></param>
		/// <returns></returns>
		object Execute(string operationName, object request, IRequestContext requestContext);
	}
}