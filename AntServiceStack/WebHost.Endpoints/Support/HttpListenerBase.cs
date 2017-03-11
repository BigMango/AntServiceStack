using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Funq;
using AntServiceStack.Common;
using AntServiceStack.Common.Web;
using AntServiceStack.Configuration;
using Freeway.Logging;
using AntServiceStack.ServiceHost;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Extensions;
using System.Text.RegularExpressions;
using AntServiceStack.WebHost.Endpoints.Registry;

namespace AntServiceStack.WebHost.Endpoints.Support
{
    public delegate void DelReceiveWebRequest(HttpListenerContext context);

    /// <summary>
    /// Wrapper class for the HTTPListener to allow easier access to the
    /// server, for start and stop management and event routing of the actual
    /// inbound requests.
    /// </summary>
    public abstract class HttpListenerBase : IDisposable, IAppHost, IHasContainer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpListenerBase));

        private const int RequestThreadAbortedException = 995;

        protected HttpListener Listener;
        protected bool IsStarted = false;

        private readonly DateTime startTime;

        public static HttpListenerBase Instance { get; protected set; }

        private readonly AutoResetEvent ListenForNextRequest = new AutoResetEvent(false);

        public event DelReceiveWebRequest ReceiveWebRequest;

        static HttpListenerBase()
        {
        }

        protected HttpListenerBase()
        {
            this.startTime = DateTime.UtcNow;
            Log.Info("Begin Initializing Application...", new Dictionary<string, string>() { { "ErrorCode", "FXD300065" } });

            EndpointHostConfig.SkipPathValidation = true;
            AfterInitCallbacks = new List<Action<IAppHost>>();
            OnDisposeCallbacks = new List<Action<IAppHost>>();
        }

        public virtual void UpdateConfig<TV>(Expression<Func<EndpointHostConfig, TV>> config, TV value)
        {
            var expression = (MemberExpression)config.Body;
            string fieldName = expression.Member.Name;
            if (string.IsNullOrEmpty(fieldName))
            {
                return;
            }
            var prop = Config.GetType().GetProperty(fieldName);
            if (prop == null)
            {
                throw new NotSupportedException("can not update Config value by name:{0}".Fmt(fieldName));
            }
            prop.SetValue(Config, value, null);
        }

        protected HttpListenerBase(params Assembly[] assembliesWithServices)
            : this()
        {
            EndpointHost.ConfigureHost(this, CreateServiceManager(assembliesWithServices));
        }

        protected HttpListenerBase(params Type[] serviceTypes)
            : this()
        {
            EndpointHost.ConfigureHost(this, CreateServiceManager(serviceTypes));
        }

        protected virtual ServiceManager CreateServiceManager(params Assembly[] assembliesWithServices)
        {
            return new ServiceManager(assembliesWithServices);
        }

        protected virtual ServiceManager CreateServiceManager(params Type[] serviceTypes)
        {
            return new ServiceManager(serviceTypes);
        }

        public void Init()
        {
            if (Instance != null)
            {
                throw new InvalidDataException("HttpListenerBase.Instance has already been set");
            }

            Instance = this;

            var serviceManager = EndpointHost.Config.ServiceManager;
            if (serviceManager != null)
            {
                serviceManager.Init();
                Configure(EndpointHost.Config.ServiceManager.Container);
            }
            else
            {
                Configure(null);
            }

            EndpointHost.AfterInit();
            SetAppDomainData();

            var elapsed = DateTime.UtcNow - this.startTime;
            Log.Info(string.Format("Initializing Application took {0}ms", elapsed.TotalMilliseconds), 
                new Dictionary<string, string>() { { "ErrorCode", "FXD300066" } });

            this.Start();
        }

        public abstract void Configure(Container container);

        public virtual void SetAppDomainData()
        {
            //Required for Mono to resolve VirtualPathUtility and Url.Content urls
            var domain = Thread.GetDomain(); // or AppDomain.Current
            domain.SetData(".appDomain", "1");
            domain.SetData(".appVPath", "/");
            domain.SetData(".appPath", domain.BaseDirectory);
            if (string.IsNullOrEmpty(domain.GetData(".appId") as string))
            {
                domain.SetData(".appId", "1");
            }
            if (string.IsNullOrEmpty(domain.GetData(".domainId") as string))
            {
                domain.SetData(".domainId", "1");
            }
        }

        /// <summary>
        /// Starts the Web Service
        /// </summary>
        /// <param name="urlBase">
        /// A Uri that acts as the base that the server is listening on.
        /// Format should be: http://127.0.0.1:8080/ or http://127.0.0.1:8080/somevirtual/
        /// Note: the trailing slash is required! For more info see the
        /// HttpListener.Prefixes property on MSDN.
        /// </param>
        public virtual void Start(string urlBase = null)
        {
            if (string.IsNullOrEmpty(Config.WebHostUrl))
            {
                throw  new ArgumentException("WebHostUrl not set");
            }
            urlBase = Config.WebHostUrl;
            //if (!string.IsNullOrWhiteSpace(urlBase))
            //{
            //    UpdateConfig(r => r.WebHostUrl, urlBase);
            //}

            // *** Already running - just leave it in place
            if (this.IsStarted)
                return;

            if (this.Listener == null)
            {
                this.Listener = new HttpListener();
            }

            EndpointHost.Config.SetServiceStackHandlerFactoryPath(HttpListenerRequestWrapper.GetHandlerPathIfAny(urlBase));

            this.Listener.Prefixes.Add(urlBase);

            this.IsStarted = true;
            this.Listener.Start();

            ThreadPool.QueueUserWorkItem(Listen);

            Regex regex = new Regex("(?<protocol>https?)://.+?:(?<port>\\d{0,5})(?<virtualPath>.*)");
            Match match = regex.Match(urlBase);
            if (match.Success)
            {
                int port;
                int.TryParse(match.Groups["port"].Value, out port);
                string protocol = match.Groups["protocol"].Value;
                string virtualPath = match.Groups["virtualPath"].Value;
            }
            else
            {
                string message = "Failed to register self to registry";
                Console.WriteLine(message);
                Log.Error(message);
            }
        }

        private bool IsListening
        {
            get { return this.IsStarted && this.Listener != null && this.Listener.IsListening; }
        }

        // Loop here to begin processing of new requests.
        private void Listen(object state)
        {
            while (this.IsListening)
            {
                if (this.Listener == null) return;

                try
                {
                    this.Listener.BeginGetContext(ListenerCallback, this.Listener);
                    ListenForNextRequest.WaitOne();
                }
                catch (Exception ex)
                {
                    Log.Error("Error occurred while host is listening", ex,
                        new Dictionary<string, string>() 
                        {
                            {"ErrorCode", "FXD300051"}
                        });
                    return;
                }
                if (this.Listener == null) return;
            }
        }

        // Handle the processing of a request in here.
        private void ListenerCallback(IAsyncResult asyncResult)
        {
            var listener = asyncResult.AsyncState as HttpListener;
            HttpListenerContext context = null;

            if (listener == null) return;

            try
            {
                if (!IsListening)
                {
                    Log.Debug(string.Format("Ignoring ListenerCallback() as HttpListener is no longer listening"),
                        new Dictionary<string, string>() 
                        {
                            {"ErrorCode", "FXD300052"}
                        });
                    return;
                }
                // The EndGetContext() method, as with all Begin/End asynchronous methods in the .NET Framework,
                // blocks until there is a request to be processed or some type of data is available.
                context = listener.EndGetContext(asyncResult);
            }
            catch (Exception ex)
            {
                // You will get an exception when httpListener.Stop() is called
                // because there will be a thread stopped waiting on the .EndGetContext()
                // method, and again, that is just the way most Begin/End asynchronous
                // methods of the .NET Framework work.
                var errMsg = ex + ": " + IsListening;
                Log.Warn(errMsg, new Dictionary<string, string>() { { "ErrorCode", "FXD300053" } });
                return;
            }
            finally
            {
                // Once we know we have a request (or exception), we signal the other thread
                // so that it calls the BeginGetContext() (or possibly exits if we're not
                // listening any more) method to start handling the next incoming request
                // while we continue to process this request on a different thread.
                ListenForNextRequest.Set();
            }

            if (context == null) return;

            Log.Info(string.Format("{0} Request : {1}", context.Request.UserHostAddress, context.Request.RawUrl),
                new Dictionary<string, string>() 
                {
                    {"ErrorCode", "FXD300054"}
                });

            //System.Diagnostics.Debug.WriteLine("Start: " + requestNumber + " at " + DateTime.UtcNow);
            //var request = context.Request;

            //if (request.HasEntityBody)

            RaiseReceiveWebRequest(context);

            try
            {
                this.ProcessRequest(context);
            }
            catch (Exception ex)
            {
                var error = string.Format("Error this.ProcessRequest(context): [{0}]: {1}", ex.GetType().Name, ex.Message);
                Log.Error(error, new Dictionary<string, string>() { { "ErrorCode", "FXD300055" } });

                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("{");
                    sb.AppendLine("\"ResponseStatus\":{");
                    sb.AppendFormat(" \"ErrorCode\":{0},\n", ex.GetType().Name.EncodeJson());
                    sb.AppendFormat(" \"Message\":{0},\n", ex.Message.EncodeJson());
                    sb.AppendFormat(" \"StackTrace\":{0}\n", ex.StackTrace.EncodeJson());
                    sb.AppendLine("}");
                    sb.AppendLine("}");

                    context.Response.StatusCode = 500;
                    context.Response.ContentType = ContentType.Json;
                    var sbBytes = sb.ToString().ToUtf8Bytes();
                    context.Response.OutputStream.Write(sbBytes, 0, sbBytes.Length);
                    context.Response.OutputStream.Flush();
                    context.Response.Close();
                }
                catch (Exception errorEx)
                {
                    error = string.Format("Error this.ProcessRequest(context)(Exception while writing error to the response): [{0}]: {1}", errorEx.GetType().Name, errorEx.Message);
                    Log.Error(error, new Dictionary<string, string>() { { "ErrorCode", "FXD300056" } });

                }
            }

            //System.Diagnostics.Debug.WriteLine("End: " + requestNumber + " at " + DateTime.UtcNow);
        }

        protected void RaiseReceiveWebRequest(HttpListenerContext context)
        {
            if (this.ReceiveWebRequest != null)
                this.ReceiveWebRequest(context);
        }


        /// <summary>
        /// Shut down the Web Service
        /// </summary>
        public virtual void Stop()
        {
            if (Listener == null) return;

            try
            {
                this.Listener.Close();
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode != RequestThreadAbortedException) throw;

                Log.Error(string.Format("Swallowing HttpListenerException({0}) Thread exit or aborted request", RequestThreadAbortedException),
                    new Dictionary<string, string>() 
                    {
                        {"ErrorCode", "FXD300067"}
                    });
            }
            this.IsStarted = false;
            this.Listener = null;
        }

        /// <summary>
        /// Overridable method that can be used to implement a custom hnandler
        /// </summary>
        /// <param name="context"></param>
        protected abstract void ProcessRequest(HttpListenerContext context);

        public Container Container
        {
            get
            {
                return EndpointHost.Config.ServiceManager.Container;
            }
        }

        public void RegisterAs<T, TAs>() where T : TAs
        {
            this.Container.RegisterAutoWiredAs<T, TAs>();
        }

        public virtual void Release(object instance)
        {
            try
            {
                var iocAdapterReleases = Container.Adapter as IRelease;
                if (iocAdapterReleases != null)
                {
                    iocAdapterReleases.Release(instance);
                }
                else
                {
                    var disposable = instance as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
            catch {/*ignore*/}
        }

        public virtual void OnEndRequest()
        {
            foreach (var item in HostContext.Instance.Items.Values)
            {
                Release(item);
            }

            HostContext.Instance.EndRequest();
        }

        public void Register<T>(T instance)
        {
            this.Container.Register(instance);
        }

        public T TryResolve<T>()
        {
            return this.Container.TryResolve<T>();
        }

        /// <summary>
        /// Resolves from IoC container a specified type instance.
        /// </summary>
        /// <typeparam name="T">Type to be resolved.</typeparam>
        /// <returns>Instance of <typeparamref name="T"/>.</returns>
        public static T Resolve<T>()
        {
            if (Instance == null) throw new InvalidOperationException("AppHostBase is not initialized.");
            return Instance.Container.Resolve<T>();
        }

        /// <summary>
        /// Resolves and auto-wires a AntServiceStack Service
        /// </summary>
        /// <typeparam name="T">Type to be resolved.</typeparam>
        /// <returns>Instance of <typeparamref name="T"/>.</returns>
        public static T ResolveService<T>(HttpListenerContext httpCtx) where T : class, IRequiresRequestContext
        {
            if (Instance == null) throw new InvalidOperationException("AppHostBase is not initialized.");
            var service = Instance.Container.Resolve<T>();
            if (service == null) return null;
            service.RequestContext = httpCtx.ToRequestContext();
            return service;
        }

        public static T ResolveService<T>(HttpListenerRequest httpReq, HttpListenerResponse httpRes)
            where T : class, IRequiresRequestContext
        {
            return ResolveService<T>(httpReq.ToRequest(), httpRes.ToResponse());
        }

        public static T ResolveService<T>(IHttpRequest httpReq, IHttpResponse httpRes) where T : class, IRequiresRequestContext
        {
            if (Instance == null) throw new InvalidOperationException("AppHostBase is not initialized.");
            var service = Instance.Container.Resolve<T>();
            if (service == null) return null;
            service.RequestContext = new HttpRequestContext(httpReq, httpRes, null);
            return service;
        }

        protected IServiceController ServiceController
        {
            get
            {
                return EndpointHost.Config.ServiceController;
            }
        }

        public Dictionary<Type, Func<IHttpRequest, object>> RequestBinders
        {
            get { return EndpointHost.ServiceManager.ServiceController.RequestTypeFactoryMap; }
        }

        public IContentTypeFilter ContentTypeFilters
        {
            get
            {
                return EndpointHost.ContentTypeFilter;
            }
        }

        public List<Action<IHttpRequest, IHttpResponse>> PreRequestFilters
        {
            get
            {
                return EndpointHost.RawRequestFilters;
            }
        }

        public List<Action<IHttpRequest, IHttpResponse, object>> RequestFilters
        {
            get
            {
                return EndpointHost.RequestFilters;
            }
        }

        public List<Action<IHttpRequest, IHttpResponse, object>> ResponseFilters
        {
            get
            {
                return EndpointHost.ResponseFilters;
            }
        }

        public HandleUncaughtExceptionDelegate ExceptionHandler
        {
            get { return EndpointHost.ExceptionHandler; }
            set { EndpointHost.ExceptionHandler = value; }
        }

        public List<HttpHandlerResolverDelegate> CatchAllHandlers
        {
            get { return EndpointHost.CatchAllHandlers; }
        }

        public EndpointHostConfig Config
        {
            get { return EndpointHost.Config; }
        }

        ///TODO: plugin added with .Add method after host initialization won't be configured. Each plugin should have state so we can invoke Register method if host was already started.  
        public List<IPlugin> Plugins
        {
            get { return EndpointHost.Plugins; }
        }

        public virtual string ResolveAbsoluteUrl(string virtualPath, IHttpRequest httpReq)
        {
            return httpReq.GetAbsoluteUrl(virtualPath);
        }

        public List<Action<IAppHost>> AfterInitCallbacks { get; }
        public List<Action<IAppHost>> OnDisposeCallbacks { get; }

        public virtual void LoadPlugin(params IPlugin[] plugins)
        {
            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.Register(this);
                }
                catch (Exception ex)
                {
                    Log.Warn("Error loading plugin " + plugin.GetType().Name, ex, new Dictionary<string, string>() { { "ErrorCode", "FXD300068" } });
                }
            }
        }

        public void RegisterService(Type serviceType)
        {
            EndpointHost.Config.ServiceManager.RegisterService(serviceType);
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
                    try
                    {
                        this.Stop();
                        if (OnDisposeCallbacks != null)
                        {
                            foreach (var callback in OnDisposeCallbacks)
                            {
                                callback(this);
                            }
                        }
                        if (EndpointHost.Config.ServiceManager != null)
                        {
                            EndpointHost.Config.ServiceManager.Dispose();
                        }
                    }
                    finally
                    {
                        Instance = null;
                    }
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

    }
}
