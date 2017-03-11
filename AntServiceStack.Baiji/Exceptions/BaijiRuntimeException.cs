#if !(SILVERLIGHT || WINDOWS_PHONE)
using System.Runtime.Serialization;
#endif

namespace AntServiceStack.Baiji.Exceptions
{
    public class BaijiRuntimeException : BaijiException
    {
        public BaijiRuntimeException(string message)
            : base(message)
        {
        }

        public BaijiRuntimeException(string message, System.Exception inner)
            : base(message, inner)
        {
        }

#if !(SILVERLIGHT || WINDOWS_PHONE)
        protected BaijiRuntimeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}