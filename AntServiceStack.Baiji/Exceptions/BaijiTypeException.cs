#if !(SILVERLIGHT || WINDOWS_PHONE)
using System.Runtime.Serialization;
#endif

namespace AntServiceStack.Baiji.Exceptions
{
    public class BaijiTypeException : BaijiException
    {
        public BaijiTypeException(string message)
            : base(message)
        {
        }

#if !(SILVERLIGHT || WINDOWS_PHONE)
        protected BaijiTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}