#if !SILVERLIGHT && !MONOTOUCH && !XBOX && !ANDROIDINDIE
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace AntServiceStack.Interface
{
    [ServiceContract(Namespace = "http://services.AntServiceStack.net/")]
    public interface ISyncReply
    {
        [OperationContract(Action = "*", ReplyAction = "*")]
        Message Send(Message requestMsg);
    }
}
#endif