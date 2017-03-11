using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Funq;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Extensions;

namespace AntServiceStack.WebHost.Endpoints.Support.Testing
{
    public class TestAppHost : IAppHost
    {
        private readonly Funq.Container container;

        public TestAppHost()
            : this(new Container(), Assembly.GetExecutingAssembly()) { }

        public TestAppHost(Funq.Container container, params Assembly[] serviceAssemblies)
        {
            this.container = container ?? new Container();
            if (serviceAssemblies.Length == 0)
                serviceAssemblies = new[] { Assembly.GetExecutingAssembly() };

            EndpointHostConfig.Instance.ServiceManager = new ServiceManager(true, serviceAssemblies);

            this.Config = EndpointHostConfig.Instance;

            this.ContentTypeFilters = new HttpResponseFilter();
            this.PreRequestFilters = new List<Action<IHttpRequest, IHttpResponse>>();
            this.RequestFilters = new List<Action<IHttpRequest, IHttpResponse, object>>();
            this.ResponseFilters = new List<Action<IHttpRequest, IHttpResponse, object>>();
            this.CatchAllHandlers = new List<HttpHandlerResolverDelegate>();
        }

        public void RegisterAs<T, TAs>() where T : TAs
        {
            this.container.RegisterAs<T, TAs>();
        }

        public virtual void Release(object instance) { }

        public void OnEndRequest() { }

        public IServiceRoutes Routes { get; private set; }

        public void Register<T>(T instance)
        {
            container.Register(instance);
        }

        public T TryResolve<T>()
        {
            return container.TryResolve<T>();
        }

        public IContentTypeFilter ContentTypeFilters { get; set; }

        public List<Action<IHttpRequest, IHttpResponse>> PreRequestFilters { get; set; }

        public List<Action<IHttpRequest, IHttpResponse, object>> RequestFilters { get; set; }

        public List<Action<IHttpRequest, IHttpResponse, object>> ResponseFilters { get; set; }

        public HandleUncaughtExceptionDelegate ExceptionHandler { get; set; }

        public List<HttpHandlerResolverDelegate> CatchAllHandlers { get; private set; }

        public Dictionary<Type, Func<IHttpRequest, object>> RequestBinders
        {
            get { throw new NotImplementedException(); }
        }

        public EndpointHostConfig Config { get; set; }

        public void RegisterService(Type serviceType, params string[] atRestPaths)
        {
            Config.ServiceManager.RegisterService(serviceType);
        }

        public List<IPlugin> Plugins { get; private set; }

        public void LoadPlugin(params IPlugin[] plugins)
        {
            plugins.ToList().ForEach(x => x.Register(this));
        }

        public virtual string ResolveAbsoluteUrl(string virtualPath, IHttpRequest httpReq)
        {
            return httpReq.GetAbsoluteUrl(virtualPath);
        }

        public List<Action<IAppHost>> AfterInitCallbacks { get; }
        public List<Action<IAppHost>> OnDisposeCallbacks { get; }


        public void RegisterService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}