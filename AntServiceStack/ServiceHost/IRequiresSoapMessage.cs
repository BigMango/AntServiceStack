using System.ServiceModel.Channels;

namespace AntServiceStack.ServiceHost
{
    public interface IRequiresSoapMessage
    {
        Message Message { get; set; }
    }
}