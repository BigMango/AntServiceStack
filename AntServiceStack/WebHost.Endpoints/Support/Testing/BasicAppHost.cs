using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Funq;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Extensions;

namespace AntServiceStack.WebHost.Endpoints.Support.Testing
{
    public class BasicAppHost : IAppHost, IHasContainer, IDisposable
    {
        public BasicAppHost()
        {
            this.Container = new Container();
            this.PreRequestFilters = new List<Action<IHttpRequest, IHttpResponse>>();
            this.RequestFilters = new List<Action<IHttpRequest, IHttpResponse, object>>();
            this.ResponseFilters = new List<Action<IHttpRequest, IHttpResponse, object>>();
            this.CatchAllHandlers = new List<HttpHandlerResolverDelegate>();
            this.AfterInitCallbacks = new List<Action<IAppHost>>();
            this.OnDisposeCallbacks = new List<Action<IAppHost>>();
        }

        public void RegisterAs<T, TAs>() where T : TAs
        {
            this.Container.RegisterAs<T, TAs>();
        }

        public virtual void Release(object instance) { }

        public void OnEndRequest() { }

        public IServiceRoutes Routes { get; private set; }

        public void Register<T>(T instance)
        {
            this.Container.Register(instance);
        }

        public T TryResolve<T>()
        {
            return this.Container.TryResolve<T>();
        }

        public Container Container { get; set; }

        public IContentTypeFilter ContentTypeFilters { get; set; }

        public List<Action<IHttpRequest, IHttpResponse>> PreRequestFilters { get; set; }

        public List<Action<IHttpRequest, IHttpResponse, object>> RequestFilters { get; set; }

        public List<Action<IHttpRequest, IHttpResponse, object>> ResponseFilters { get; set; }

        public HandleUncaughtExceptionDelegate ExceptionHandler { get; set; }

        public List<HttpHandlerResolverDelegate> CatchAllHandlers { get; set; }

        public Dictionary<Type, Func<IHttpRequest, object>> RequestBinders
        {
            get { throw new NotImplementedException(); }
        }

        private EndpointHostConfig config;
        public EndpointHostConfig Config
        {
            get
            {
                if (config == null)
                {
                    EndpointHostConfig.Instance.ServiceManager = new ServiceManager(Container, Assembly.GetExecutingAssembly());
                    config = EndpointHostConfig.Instance;
                }
                return config;
            }
            set { config = value; }
        }

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

        public BasicAppHost Init()
        {
            EndpointHost.ConfigureHost(this, Config.ServiceManager);
            return this;
        }


        private bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            lock (this)
            {
                if (disposed) return;

                if (disposing)
                {
                    if (EndpointHost.Config != null && EndpointHost.Config.ServiceManager != null)
                    {
                        EndpointHost.Config.ServiceManager.Dispose();
                    }

                    EndpointHost.Dispose();
                }

                //release unmanaged resources here...
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public void RegisterService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}