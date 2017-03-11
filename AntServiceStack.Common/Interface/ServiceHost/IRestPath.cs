using System;
using System.Collections.Generic;
using System.Reflection;

namespace AntServiceStack.ServiceHost
{
	public interface IRestPath
	{
        bool IsWildCardPath { get; }

		MethodInfo ServiceMethod { get; }

        Type RequestType { get; }

        string OperationName { get; }

		object CreateRequestObject(string pathInfo, Dictionary<string, string> queryStringAndFormData, object fromInstance);
	}
}