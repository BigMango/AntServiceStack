#if !NO_RUNTIME

namespace AntServiceStack.ProtoBuf.Serializers
{
    interface ISerializerProxy
    {
        IProtoSerializer Serializer { get; }
    }
}
#endif