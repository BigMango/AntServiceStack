using System;
using System.Runtime.Serialization;

namespace AntServiceStack.WebHost.Endpoints
{
    public class RequestBindingException : SerializationException
    {
        public RequestBindingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
