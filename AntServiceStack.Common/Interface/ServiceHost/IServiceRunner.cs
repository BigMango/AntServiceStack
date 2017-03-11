using System;

namespace AntServiceStack.ServiceHost
{
    public interface IServiceRunner
    {
        object Process(IRequestContext requestContext, object instance, object request);
    }

    public interface IServiceRunner<TRequest> : IServiceRunner
    {
        void OnBeforeExecute(IRequestContext requestContext, TRequest request);
        object OnAfterExecute(IRequestContext requestContext, object response);
        object HandleException(IRequestContext requestContext, TRequest request, Exception ex);

        object Execute(IRequestContext requestContext, object instance, TRequest request);
        object ExecuteOneWay(IRequestContext requestContext, object instance, TRequest request);
    }
}