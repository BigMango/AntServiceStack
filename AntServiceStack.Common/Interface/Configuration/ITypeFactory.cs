using System;

namespace AntServiceStack.Configuration
{
	public interface ITypeFactory
	{
		object CreateInstance(Type type);
	}
}