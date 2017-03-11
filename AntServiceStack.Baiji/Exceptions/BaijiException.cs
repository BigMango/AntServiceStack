#if !(SILVERLIGHT || WINDOWS_PHONE)
using System.Runtime.Serialization;
#endif

namespace AntServiceStack.Baiji.Exceptions
{
    public class BaijiException : System.Exception
    {
        public BaijiException(string message)
            : base(message)
        {
        }

        public BaijiException(string message, System.Exception inner)
            : base(message, inner)
        {
        }

#if !(SILVERLIGHT || WINDOWS_PHONE)
        protected BaijiException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}