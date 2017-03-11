using System;
using System.IO;

namespace AntServiceStack.Common.ServiceClient
{
    public interface IClientCallFormat
    {
        string Format { get; }

        string ContentType { get; }

        ClientStreamDeserializerDelegate StreamDeserializer { get; }

        ClientStreamSerializerDelegate StreamSerializer { get; }
    }

    public delegate object ClientStreamDeserializerDelegate (Type type, Stream fromStream);

    public delegate void ClientStreamSerializerDelegate(object dto, Stream outputStream);
}
