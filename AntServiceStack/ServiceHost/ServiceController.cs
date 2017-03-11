using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using AntServiceStack.Configuration;
using Freeway.Logging;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.Text;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Types;
using AntServiceStack.Common.Extensions;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.ServiceHost;
using System.Web.Hosting;
using System.Web;
using System.Threading.Tasks;
using AntServiceStack.WebHost.Endpoints.Utils;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.Common;

namespace AntServiceStack.ServiceHost
{
    public delegate object ServiceExecFn(IRequestContext requestContext, object request);
    
    public class ServiceController
           : IServiceController
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceController));

        public ServiceController(Func<IEnumerable<Type>> resolveServicesFn, Dictionary<string, ServiceMetadata> metadataMap)
        {
            this.MetadataMap = metadataMap;
            this.RequestTypeFactoryMap = new Dictionary<Type, Func<IHttpRequest, object>>();
            this.EnableAccessRestrictions = true;
            this.ResolveServicesFn = resolveServicesFn;
        }

        readonly Dictionary<string, Dictionary<string, ServiceExecFn>> operationExecMap = new Dictionary<string,Dictionary<string,ServiceExecFn>>();

        /// <summary>
        /// 查看是否有限制条件(比如是否限制只能localhost访问之类的)
        /// </summary>
        readonly Dictionary<string, RestrictAttribute> operationServiceAttrs
            = new Dictionary<string, RestrictAttribute>();

        public bool EnableAccessRestrictions { get; set; }

        /// <summary>
        /// 元数据集合
        /// </summary>
        public Dictionary<string, ServiceMetadata> MetadataMap { get; internal set; }

        public Dictionary<Type, Func<IHttpRequest, object>> RequestTypeFactoryMap { get; set; }

        public string DefaultOperationsNamespace { get; set; }

        private IResolver resolver;
        public IResolver Resolver
        {
            get { return resolver ?? EndpointHost.AppHost; }
            set { resolver = value; }
        }

        public Func<IEnumerable<Type>> ResolveServicesFn { get; set; }

        /// <summary>
        /// 遍历从Func方法里面拿到的Type 拿到继承实现了CServiceInterface接口的class
        /// </summary>
        /// <param name="serviceFactoryFn"></param>
        public void Register(ITypeFactory serviceFactoryFn)
        {
            foreach (var serviceType in ResolveServicesFn())
            {
                RegisterGService(serviceFactoryFn, serviceType);
            }
        }

        // Current For Testing
        public void Register(Type serviceType)
        {
            var handlerFactoryFn = Expression.Lambda<Func<Type, object>>
            (
                Expression.New(serviceType),
                Expression.Parameter(typeof(Type), "serviceType")
            ).Compile();

            Register(serviceType, handlerFactoryFn);
        }

        // Current For Testing
        public void Register(Type serviceType, Func<Type, object> handlerFactoryFn)
        {
            RegisterGService(new TypeFactoryWrapper(handlerFactoryFn), serviceType);
        }

        /// <summary>
        /// 拿到继承实现了CServiceInterface接口的class
        /// </summary>
        /// <param name="serviceFactoryFn"></param>
        /// <param name="serviceType"></param>
        public void RegisterGService(ITypeFactory serviceFactoryFn, Type serviceType)
        {
            if (serviceType.IsAbstract || serviceType.ContainsGenericParameters) return;

            //IService
            foreach (var serviceIntf in serviceType.GetInterfaces())
            {
                // Is this service supported by AntServiceStack
                if (!serviceIntf.HasAttribute<AntServiceInterfaceAttribute>()) continue;

                // Copy service name and namespace from CServiceInterface attribute 只拿第一个
                var cserviceAttribute = serviceIntf.AttributesOfType<AntServiceInterfaceAttribute>().First();

                //拿到服务名称 服务的命名空间 和 CodeGen的版本号
                ServiceMetadata serviceMetadata = new ServiceMetadata(
                    serviceType, cserviceAttribute.ServiceName, cserviceAttribute.ServiceNamespace, cserviceAttribute.CodeGeneratorVersion);

                if (MetadataMap.ContainsKey(serviceMetadata.ServicePath))
                    MetadataMap[serviceMetadata.ServicePath].MergeData(serviceMetadata);
                else
                    MetadataMap[serviceMetadata.ServicePath] = serviceMetadata;

                // reflect methods in service interface 拿到controller里面的所有的action
                foreach (MethodInfo methodInfoIntf in serviceIntf.GetMethods())
                {
                    //实现了接口的action
                    MethodInfo methodInfoImpl = serviceType.GetMethod(methodInfoIntf.Name, BindingFlags.Public | BindingFlags.Instance);

                    //注册action
                    RegisterGServiceMethodExecutor(serviceMetadata.ServicePath, serviceMetadata.ServiceName, serviceType, methodInfoImpl, serviceFactoryFn);

                    var responseType = methodInfoIntf.ReturnType;
                    bool isAsync = responseType.FullName.StartsWith("System.Threading.Tasks.Task`1");
                    if (isAsync)
                        responseType = responseType.GetGenericArguments()[0];
                    if (responseType == typeof(void))
                        responseType = null;

                    Type requestType = methodInfoIntf.GetParameters()[0].ParameterType;
                    RegisterCommon(serviceMetadata.ServicePath, serviceType, methodInfoImpl, requestType, responseType, isAsync);
                }
            }
        }

        public void RegisterCommon(string servicePath, Type serviceType, MethodInfo methodInfo, Type requestType, Type responseType, bool isAsync)
        {
            //路由注册
            RegisterRestPaths(methodInfo, servicePath);

            MetadataMap[servicePath].Add(serviceType, methodInfo, requestType, responseType, isAsync);

            if (typeof(IRequiresRequestStream).IsAssignableFrom(requestType))
            {
                this.RequestTypeFactoryMap[requestType] = httpReq =>
                {
                    var rawReq = (IRequiresRequestStream)requestType.CreateInstance();
                    rawReq.RequestStream = httpReq.InputStream;
                    return rawReq;
                };
            }

            Log.Debug(string.Format("Registering {0} service '{1}' with method '{2}'",
                (responseType != null ? "Reply" : "OneWay"), serviceType.Name, methodInfo.Name), 
                new Dictionary<string, string>() { { "ErrorCode", "FXD300042" } });
        }

        public readonly Dictionary<string, Dictionary<string, List<RestPath>>> RestPathMap = new Dictionary<string, Dictionary<string, List<RestPath>>>();

        /// <summary>
        /// 注册路由
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="servicePath"></param>
        public void RegisterRestPaths(MethodInfo methodInfo, string servicePath)
        {
            //获取到RouteAttribute
            List<RouteAttribute> attrs = methodInfo.GetCustomAttributes(typeof(RouteAttribute), false).OfType<RouteAttribute>().ToList();
            //默认的路由前面要加上 "/"
            string defaultOperationRestPath = ("/" + methodInfo.Name).ToLower();
            //如果没有打RouteAttribute 加入一个默认的路由
            if (attrs.Count(a => a.Path.ToLower() == defaultOperationRestPath) == 0)
            {
                attrs.Add(new RouteAttribute(defaultOperationRestPath));
            }

            foreach (RouteAttribute attr in attrs)
            {
                var restPath = new RestPath(methodInfo, attr.Path, attr.Verbs, attr.Summary, attr.Notes);

                var defaultAttr = attr as FallbackRouteAttribute;
                if (defaultAttr != null)
                {
                    if (EndpointHost.Config != null)
                    {
                        if (EndpointHost.Config.FallbackRestPaths.ContainsKey(servicePath))
                            throw new NotSupportedException(string.Format(
                                "FallbackRouteAttribute is already defined. Only 1 [FallbackRoute] is allowed."));

                        EndpointHost.Config.FallbackRestPaths[servicePath] = (httpMethod, theServicePath, pathInfo, filePath) =>
                        {
                            var pathInfoParts = RestPath.GetPathPartsForMatching(pathInfo);
                            return restPath.IsMatch(httpMethod, pathInfoParts) ? restPath : null;
                        };
                    }

                    continue;
                }

                if (!restPath.IsValid)
                    throw new NotSupportedException(string.Format(
                        "RestPath '{0}' on method '{1}' is not Valid", attr.Path, methodInfo.Name));
                //路由注册
                RegisterRestPath(restPath, servicePath);
            }
        }

        private static readonly char[] InvalidRouteChars = new[] { '?', '&' };

        public void RegisterRestPath(RestPath restPath, string servicePath)
        {
            if (!EndpointHostConfig.SkipRouteValidation)
            {
                if (!restPath.Path.StartsWith("/"))
                    throw new ArgumentException("Route '{0}' on '{1}' must start with a '/'".Fmt(restPath.Path, restPath.RequestType.Name));
                if (restPath.Path.IndexOfAny(InvalidRouteChars) != -1)
                    throw new ArgumentException(("Route '{0}' on '{1}' contains invalid chars. " +
                                                "See https://github.com/ServiceStack/ServiceStack/wiki/Routing for info on valid routes.").Fmt(restPath.Path, restPath.RequestType.Name));
            }

            if (!RestPathMap.ContainsKey(servicePath))
                RestPathMap[servicePath] = new Dictionary<string, List<RestPath>>();
            if (!RestPathMap[servicePath].ContainsKey(restPath.FirstMatchHashKey))
                RestPathMap[servicePath][restPath.FirstMatchHashKey] = new List<RestPath>();
            RestPathMap[servicePath][restPath.FirstMatchHashKey].Add(restPath);
        }

        public void AfterInit()
        {
            //Register any routes configured on MetadataMap.Routes
            foreach (var serviceMetadata in this.MetadataMap.Values)
            {
                foreach (var restPath in serviceMetadata.Routes.RestPaths)
                {
                    RegisterRestPath(restPath, serviceMetadata.ServicePath);
                }

                //Sync the RestPaths collections
                serviceMetadata.Routes.RestPaths.Clear();
                serviceMetadata.Routes.RestPaths.AddRange(RestPathMap[serviceMetadata.ServicePath].Values.SelectMany(x => x));

                serviceMetadata.AfterInit();
            }
        }

        public IRestPath GetRestPathForRequest(string httpMethod, string servicePath, string pathInfo)
        {
            Dictionary<string, List<RestPath>> restPaths = RestPathMap[servicePath];

            var matchUsingPathParts = RestPath.GetPathPartsForMatching(pathInfo);

            List<RestPath> firstMatches;

            var yieldedHashMatches = RestPath.GetFirstMatchHashKeys(matchUsingPathParts);

            foreach (var potentialHashMatch in yieldedHashMatches)
            {
                if (!restPaths.TryGetValue(potentialHashMatch, out firstMatches)) continue;

                var bestScore = -1;
                foreach (var restPath in firstMatches)
                {
                    var score = restPath.MatchScore(httpMethod, matchUsingPathParts);
                    if (score > bestScore) bestScore = score;
                }
                if (bestScore > 0)
                {
                    foreach (var restPath in firstMatches)
                    {
                        if (bestScore == restPath.MatchScore(httpMethod, matchUsingPathParts))
                            return restPath;
                    }
                }
            }

            var yieldedWildcardMatches = RestPath.GetFirstMatchWildCardHashKeys(matchUsingPathParts);
            foreach (var potentialHashMatch in yieldedWildcardMatches)
            {
                if (!restPaths.TryGetValue(potentialHashMatch, out firstMatches)) continue;

                var bestScore = -1;
                foreach (var restPath in firstMatches)
                {
                    var score = restPath.MatchScore(httpMethod, matchUsingPathParts);
                    if (score > bestScore) bestScore = score;
                }
                if (bestScore > 0)
                {
                    foreach (var restPath in firstMatches)
                    {
                        if (bestScore == restPath.MatchScore(httpMethod, matchUsingPathParts))
                            return restPath;
                    }
                }
            }

            return null;
        }

        internal class TypeFactoryWrapper : ITypeFactory
        {
            private readonly Func<Type, object> typeCreator;

            public TypeFactoryWrapper(Func<Type, object> typeCreator)
            {
                this.typeCreator = typeCreator;
            }

            public object CreateInstance(Type type)
            {
                return typeCreator(type);
            }
        }

        public void Register(string servicePath, string serviceName, Type serviceType, MethodInfo mi)
        {
            var handlerFactoryFn = Expression.Lambda<Func<Type, object>>
                (
                    Expression.New(serviceType),
                    Expression.Parameter(typeof(Type), "serviceType")
                ).Compile();

            RegisterGServiceMethodExecutor(servicePath, serviceName, serviceType, mi, new TypeFactoryWrapper(handlerFactoryFn));
        }

        public void Register(string servicePath, string serviceName, Type serviceType, MethodInfo mi, Func<Type, object> handlerFactoryFn)
        {
            RegisterGServiceMethodExecutor(servicePath, serviceName, serviceType, mi, new TypeFactoryWrapper(handlerFactoryFn));
        }

        /// <summary>
        /// 注册action 
        /// </summary>
        /// <param name="servicePath"></param>
        /// <param name="serviceName"></param>
        /// <param name="serviceType"></param>
        /// <param name="mi"></param>
        /// <param name="serviceFactoryFn"></param>
        public void RegisterGServiceMethodExecutor(string servicePath, string serviceName, Type serviceType, MethodInfo mi, ITypeFactory serviceFactoryFn)
        {
            //构造执行方法
            var serviceMethodExecFn = BuildServiceMethodExecutor(mi, serviceType);
            //返回类型
            var responseType = mi.ReturnType;
            //是否是异步的
            bool isAsync = responseType.FullName.StartsWith("System.Threading.Tasks.Task`1");
            if (isAsync)
                responseType = responseType.GetGenericArguments()[0];//如果是异步的 返回类型是拿 泛型的定义
            if (responseType == typeof(void))
                responseType = null;//判断是否没有返回类型

            ServiceExecFn handlerFn = null;
            if (isAsync)
            {
                //如果是异步的 
                Func<object, object> getResult = mi.ReturnType.GetProperty("Result").CreateGetter();
                handlerFn = (requestContext, dto) =>
                {
                    var service = serviceFactoryFn.CreateInstance(serviceType);
                    ServiceExecFn serviceExec = (reqCtx, req) => serviceMethodExecFn(req, service);
                    return ManagedAsyncServiceMethodExec(serviceExec, service, requestContext, dto, responseType, getResult);
                };
            }
            else
            {
                //同步
                handlerFn = (requestContext, dto) =>
                {
                    var service = serviceFactoryFn.CreateInstance(serviceType);
                    ServiceExecFn serviceExec = (reqCtx, req) => serviceMethodExecFn(req, service);
                    try
                    {
                        return ManagedServiceMethodExec(serviceExec, service, requestContext, dto, responseType);
                    }
                    finally
                    {
                    }
                };
            }

            AddToMethodExecMap(servicePath, mi, serviceType, handlerFn);
        }

        private static Func<object, object, object> BuildServiceMethodExecutor(MethodInfo mi, Type serviceType)
        {
            Type requestType = mi.GetParameters()[0].ParameterType;

            try
            {
                var requestDtoParam = Expression.Parameter(typeof(object), "requestDto");
                var requestDtoStrong = Expression.Convert(requestDtoParam, requestType);

                var serviceParam = Expression.Parameter(typeof(object), "serviceObj");
                var serviceStrong = Expression.Convert(serviceParam, serviceType);

                Expression callMethod = Expression.Call(serviceStrong,
                    mi, new Expression[] { requestDtoStrong });

                var executeFunc = Expression.Lambda<Func<object, object, object>>
                    (callMethod, requestDtoParam, serviceParam).Compile();

                return executeFunc;

            }
            catch (Exception)
            {
                //problems with MONO, using reflection for fallback
                return (request, service) => mi.Invoke(service, new[] { request });
            }
        }

        /// <summary>
        /// 加入到operationExecMap集合 key为：servicePath
        /// </summary>
        /// <param name="servicePath"></param>
        /// <param name="methodInfo"></param>
        /// <param name="serviceType"></param>
        /// <param name="handlerFn"></param>
        private void AddToMethodExecMap(string servicePath, MethodInfo methodInfo, Type serviceType, ServiceExecFn handlerFn)
        {
            if (!operationExecMap.ContainsKey(servicePath))
                operationExecMap[servicePath] = new Dictionary<string, ServiceExecFn>();

            //同一个servicePath(指的是二级目录) 如果有多个同样名称的方法 会报错
            if (operationExecMap[servicePath].ContainsKey(methodInfo.Name.ToLower()))
            {
                throw new AmbiguousMatchException(
                    string.Format(
                    "Could not register method name '{0}' with service '{1}' as it has already been registered.\n"
                    + "Each method name can only be registered once by a service, method name overloading is not allowed by AntServiceStack.",
                    methodInfo.Name, serviceType.FullName));
            }

            operationExecMap[servicePath].Add(methodInfo.Name.ToLower(), handlerFn);

            // RestrictAttribute attributes are only annotated on methods of service implementation
            MethodInfo methodInfoImpl = serviceType.GetMethod(methodInfo.Name, BindingFlags.Public | BindingFlags.Instance);

            //查看是否有限制条件(比如是否限制只能localhost访问之类的)
            var serviceAttrs = methodInfoImpl.GetCustomAttributes(typeof(RestrictAttribute), false);
            if (serviceAttrs.Length > 0)
            {
                operationServiceAttrs.Add(methodInfo.Name.ToLower(), (RestrictAttribute)serviceAttrs[0]);
            }
        }

        private static object ManagedServiceMethodExec(
            ServiceExecFn serviceExec,
            object service, IRequestContext requestContext, object dto, Type responseType)
        {
            
            InjectRequestContext(service, requestContext);

            Stopwatch stopwatch = new Stopwatch();
            var httpRequest = requestContext != null ? requestContext.Get<IHttpRequest>() : null;
            var httpResponse = requestContext != null ? requestContext.Get<IHttpResponse>() : null;

            try
            {
                if (EndpointHost.Config != null && EndpointHost.Config.PreExecuteServiceFilter != null)
                {
                    EndpointHost.Config.PreExecuteServiceFilter(service, httpRequest, httpResponse);
                }

                string identity = null;
                if (EndpointHost.ServiceManager != null && EndpointHost.MetadataMap != null && httpRequest != null)
                    identity = EndpointHost.Config.MetadataMap[httpRequest.ServicePath].GetOperationByOpName(httpRequest.OperationName).Key;
                
                object response;
                //Executes the service and returns the result
                stopwatch.Start();
                try
                {
                    response = serviceExec(requestContext, dto);
                }
                catch (Exception exception)
                {
                    throw;
                }
                finally
                {
                }
                stopwatch.Stop();
                // Record service execution time
                if (httpResponse != null)
                {
                    httpResponse.ResponseObject = response;
                    if (httpResponse.ExecutionResult != null)
                    {
                        httpResponse.ExecutionResult.ServiceExecutionTime = stopwatch.ElapsedMilliseconds;
                    }
                }

                if (response == null) // null response
                {
                    throw new NullReferenceException("Null response returned by service call");
                }

                if (EndpointHost.Config != null && EndpointHost.Config.PostExecuteServiceFilter != null)
                {
                    EndpointHost.Config.PostExecuteServiceFilter(service, httpRequest, httpResponse);
                }

                IHasResponseStatus hasResponseStatus = response as IHasResponseStatus;
                if (hasResponseStatus != null && hasResponseStatus.ResponseStatus != null
                    && hasResponseStatus.ResponseStatus.Ack == AckCodeType.Failure)
                {
                    if (hasResponseStatus.ResponseStatus.Errors.Count > 0)
                        ErrorUtils.LogError("Internal handled Service Error", httpRequest, hasResponseStatus.ResponseStatus, false, "FXD300003");
                    else
                        ErrorUtils.LogError("Internal handled Service Error. But no error data in ResponseStatus.Errors. QA please fire bug to dev and let dev fix it.",
                            httpRequest, hasResponseStatus.ResponseStatus, true, "FXD300028");
                }

                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = ErrorUtils.CreateServiceErrorResponse(httpRequest, ex, responseType);
                if (httpResponse != null)
                {
                    httpResponse.ResponseObject = errorResponse;
                    if (httpResponse.ExecutionResult != null)
                    {
                        httpResponse.ExecutionResult.ExceptionCaught = ex;
                        // Mark service excution throws exception
                        httpResponse.ExecutionResult.ServiceExceptionThrown = true;
                        // take service execution time into accout even the service execution throws excetpion
                        if (stopwatch.IsRunning)
                        {
                            stopwatch.Stop();
                        }
                        httpResponse.ExecutionResult.ServiceExecutionTime = stopwatch.ElapsedMilliseconds;
                    }
                }
                return errorResponse;
            }
            finally
            {
                if (EndpointHost.AppHost != null)
                {
                    //Gets disposed by AppHost or ContainerAdapter if set
                    EndpointHost.AppHost.Release(service);
                }
                else
                {
                    using (service as IDisposable) { }
                }
            }
        }

        /// <summary>
        /// 构造异步的执行action
        /// </summary>
        /// <param name="serviceExec"></param>
        /// <param name="service"></param>
        /// <param name="requestContext"></param>
        /// <param name="dto"></param>
        /// <param name="responseType"></param>
        /// <param name="getResult"></param>
        /// <returns></returns>
        private static Task<object> ManagedAsyncServiceMethodExec(
            ServiceExecFn serviceExec,
            object service, IRequestContext requestContext, object dto, Type responseType, Func<object, object> getResult)
        {
            Stopwatch stopwatch = new Stopwatch();
            var httpRequest = requestContext != null ? requestContext.Get<IHttpRequest>() : null;
            var httpResponse = requestContext != null ? requestContext.Get<IHttpResponse>() : null;
            string identity = null;
            if(EndpointHost.ServiceManager != null && EndpointHost.MetadataMap != null && httpRequest != null)
                identity = EndpointHost.Config.MetadataMap[httpRequest.ServicePath].GetOperationByOpName(httpRequest.OperationName).Key;
            
            Task serviceTask = null;
            try
            {
                InjectRequestContext(service, requestContext);

                if (EndpointHost.Config != null && EndpointHost.Config.PreExecuteServiceFilter != null)
                    EndpointHost.Config.PreExecuteServiceFilter(service, httpRequest, httpResponse);

                //Executes the async service and returns the task
                stopwatch.Start();
                try
                {
                    serviceTask = serviceExec(requestContext, dto) as Task;
                }
                catch (Exception ex)
                {

                    object startTimeObject;
                    if (httpRequest.Items.TryGetValue(ServiceCatConstants.SOA2AsyncServiceStartTimeKey, out startTimeObject))
                    {
                        var startTime = (DateTime)startTimeObject;
                    }
                    throw;
                }
                finally
                {
                }

                if (serviceTask == null) // null task
                    throw new InvalidOperationException(ServiceUtils.AsyncOperationReturnedNullTask);

                if(serviceTask.Status == TaskStatus.Created)
                    throw new InvalidAsynchronousStateException("Service task status is invalid: TaskStatus.Created");
            }
            catch (Exception ex)
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                try
                {
                    if (stopwatch.IsRunning)
                    {
                        stopwatch.Stop();
                    }
                    var response = ErrorUtils.CreateServiceErrorResponse(httpRequest, ex, responseType);
                    if (httpResponse != null)
                    {
                        httpResponse.ResponseObject = response;
                        if (httpResponse.ExecutionResult != null)
                        {
                            httpResponse.ExecutionResult.ExceptionCaught = ex;
                            httpResponse.ExecutionResult.ServiceExceptionThrown = true;
                            httpResponse.ExecutionResult.ServiceExecutionTime = stopwatch.ElapsedMilliseconds;
                        }
                    }
                    tcs.TrySetResult(response);
                }
                catch (Exception ex1)
                {
                    tcs.TrySetException(ex1);
                }

                return tcs.Task;
            }

            return serviceTask.ContinueWith(t => 
            {
                try
                {
                    Exception taskException = t.Exception;
                    
                    stopwatch.Stop();

                    if (taskException != null) // handler service exception
                    {
                        Exception ex = taskException.InnerException ?? taskException;
                        var responseObject = ErrorUtils.CreateServiceErrorResponse(httpRequest, ex, responseType);
                        
                        if (httpResponse != null)
                        {
                            if (httpResponse.ExecutionResult != null)
                            {
                                httpResponse.ExecutionResult.ExceptionCaught = ex;
                                // Mark service execution throws exception
                                httpResponse.ExecutionResult.ServiceExceptionThrown = true;
                                httpResponse.ExecutionResult.ServiceExecutionTime = stopwatch.ElapsedMilliseconds;
                            }
                            httpResponse.ResponseObject = responseObject;
                        }
                        return responseObject;
                    }

                    object response = getResult(serviceTask); // get the real response
                    if (response == null) // null response, create service error response
                    {
                        var ex = new InvalidOperationException("Null response returned by service call");
                        var responseObject = ErrorUtils.CreateServiceErrorResponse(httpRequest, ex, responseType);
                        if (httpResponse != null)
                        {
                            httpResponse.ResponseObject = responseObject;
                            if (httpResponse.ExecutionResult != null)
                            {
                                httpResponse.ExecutionResult.ExceptionCaught = ex;
                                httpResponse.ExecutionResult.ServiceExceptionThrown = true;
                                httpResponse.ExecutionResult.ServiceExecutionTime = stopwatch.ElapsedMilliseconds;
                            }
                        }
                        return responseObject;
                    }

                    if (EndpointHost.Config != null && EndpointHost.Config.PostExecuteServiceFilter != null)
                        EndpointHost.Config.PostExecuteServiceFilter(service, httpRequest, httpResponse);

                    IHasResponseStatus hasResponseStatus = response as IHasResponseStatus;
                    if (hasResponseStatus != null && hasResponseStatus.ResponseStatus != null
                        && hasResponseStatus.ResponseStatus.Ack == AckCodeType.Failure)
                    {
                        if (hasResponseStatus.ResponseStatus.Errors.Count > 0)
                            ErrorUtils.LogError("Internal handled Service Error", httpRequest, hasResponseStatus.ResponseStatus, false, "FXD300003");
                        else
                            ErrorUtils.LogError("Internal handled Service Error. But no error data in ResponseStatus.Errors. QA please fire bug to dev and let dev fix it.",
                                httpRequest, hasResponseStatus.ResponseStatus, true, "FXD300028");
                    }
                    if (httpResponse != null)
                    {
                        httpResponse.ResponseObject = response;
                        if (httpResponse.ExecutionResult != null)
                        {
                            httpResponse.ExecutionResult.ServiceExecutionTime = stopwatch.ElapsedMilliseconds;
                        }
                    }
                    return response;
                }
                catch (Exception ex) // catch unexpected framework exception
                {
                    var responseObject = ErrorUtils.CreateFrameworkErrorResponse(httpRequest, ex, responseType);
                    if (httpResponse != null)
                    {
                        httpResponse.ResponseObject = responseObject;
                        if (httpResponse.ExecutionResult != null)
                        {
                            httpResponse.ExecutionResult.ExceptionCaught = ex;
                            // Mark framework excution throws exception
                            httpResponse.ExecutionResult.FrameworkExceptionThrown = true;
                            httpResponse.ExecutionResult.ServiceExecutionTime = stopwatch.ElapsedMilliseconds;
                        }
                    }
                    return responseObject;
                }
                finally
                {

                    if (EndpointHost.AppHost != null)
                    {
                        //Gets disposed by AppHost or ContainerAdapter if set
                        EndpointHost.AppHost.Release(service);
                    }
                    else
                    {
                        using (service as IDisposable) { }
                    }
                }
            });
        }

        /// <summary>
        /// 注入 HttRequest 和 HttpResponse
        /// </summary>
        /// <param name="service"></param>
        /// <param name="requestContext"></param>
        private static void InjectRequestContext(object service, IRequestContext requestContext)
        {
            if (requestContext == null) return;

            var serviceRequiresContext = service as IRequiresRequestContext;
            if (serviceRequiresContext != null)
            {
                serviceRequiresContext.RequestContext = requestContext;
            }

            var servicesRequiresHttpRequest = service as IRequiresHttpRequest;
            if (servicesRequiresHttpRequest != null)
                servicesRequiresHttpRequest.HttpRequest = requestContext.Get<IHttpRequest>();

            var serviceReqeuiresHttpResponse = service as IRequiresHttpResponse;
            if(serviceReqeuiresHttpResponse !=  null)
                serviceReqeuiresHttpResponse.HttpResponse = requestContext.Get<IHttpResponse>();
        }

        //Execute HTTP
        public object Execute(string operationName, object request, IRequestContext requestContext)
        {
            if (EnableAccessRestrictions)
            {
                AssertServiceRestrictions(operationName,
                    requestContext != null ? requestContext.EndpointAttributes : EndpointAttributes.None);
            }

            string servicePath = ServiceMetadata.DefaultServicePath;
            if (requestContext != null)
            {
                IHttpRequest httpRequest = requestContext.Get<IHttpRequest>();
                if (httpRequest != null)
                    servicePath = httpRequest.ServicePath;
            }
            var serviceExector = GetServiceExecutorByOperationName(operationName, servicePath);
            return serviceExector(requestContext, request);
        }

        private ServiceExecFn GetServiceExecutorByOperationName(string operationName, string servicePath)
        {
            ServiceExecFn serviceExecutor;
            if (!operationExecMap[servicePath].TryGetValue(operationName.ToLower(), out serviceExecutor))
            {
                throw new NotImplementedException(string.Format("Unable to resolve service method '{0}'", operationName));
            }

            return serviceExecutor;
        }

        public object ExecuteText(string operationName, string requestXml, Type requestType, IRequestContext requestContext)
        {
            var request = WrappedXmlSerializer.DeserializeFromString(requestXml, requestType);
            IHttpRequest httpReq = requestContext.Get<IHttpRequest>();
            if (httpReq != null)
                httpReq.RequestObject = request;
            var response = Execute(operationName, request, requestContext);
            var responseXml = WrappedXmlSerializer.SerializeToString(response, false);
            return responseXml;
        }

        public void AssertServiceRestrictions(string operationName, EndpointAttributes actualAttributes)
        {
            if (EndpointHost.Config != null && !EndpointHost.Config.EnableAccessRestrictions) return;

            RestrictAttribute restrictAttr;
            var hasNoAccessRestrictions = !operationServiceAttrs.TryGetValue(operationName.ToLower(), out restrictAttr)
                || restrictAttr.HasNoAccessRestrictions;

            if (hasNoAccessRestrictions)
            {
                return;
            }

            var failedScenarios = new StringBuilder();
            foreach (var requiredScenario in restrictAttr.AccessibleToAny)
            {
                var allServiceRestrictionsMet = (requiredScenario & actualAttributes) == actualAttributes;
                if (allServiceRestrictionsMet)
                {
                    return;
                }

                var passed = requiredScenario & actualAttributes;
                var failed = requiredScenario & ~(passed);

                failedScenarios.AppendFormat("\n -[{0}]", failed);
            }

            var internalDebugMsg = (EndpointAttributes.InternalNetworkAccess & actualAttributes) != 0
                ? "\n Unauthorized call was made from: " + actualAttributes
                : "";

            throw new UnauthorizedAccessException(
                string.Format("Could not execute service method '{0}', The following restrictions were not met: '{1}'" + internalDebugMsg,
                    operationName, failedScenarios));
        }
    }
}
