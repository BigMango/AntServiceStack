using AntServiceStack.ServiceHost;

namespace AntServiceStack.Plugins.SimpleAuth
{
    public interface ISimpleAuthProvider
    {
        /// <summary>
        /// <para>简单认证接口</para>
        /// <para>如果验证成功,则返回 true; 否则返回 false (此时框架将返回 Forbidden).</para>
        /// </summary>
        /// <param name="request">请求对象</param>
        /// <param name="requestDto">请求包含的DTO对象</param>
        /// <param name="operationName">请求的操作名称</param>
        bool Authenticate(IHttpRequest request, object requestDto, string operationName);
    }
}
