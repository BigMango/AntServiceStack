#if !(SILVERLIGHT || WINDOWS_PHONE)
using System.Runtime.Serialization;
#endif
using AntServiceStack.Baiji.Exceptions;

namespace AntServiceStack.Baiji.Schema
{
    public class SchemaParseException : BaijiException
    {
        public SchemaParseException(string message)
            : base(message)
        {
        }

#if !(SILVERLIGHT || WINDOWS_PHONE)
        protected SchemaParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}