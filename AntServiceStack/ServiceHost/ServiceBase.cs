using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.OrmLite;
using AntServiceStack.CacheAccess;
using AntServiceStack.Common.Types;
using AntServiceStack.Common.Interface.ServiceHost;

namespace AntServiceStack.ServiceHost
{
    /// <summary>
    /// Generic + Useful service base class, by extending this base class,
    /// service implementation will have direct access to raw http request and response object,
    /// service implementation can also leverage IOC to resolve dependency at runtime.
    /// </summary>
    public abstract class ServiceBase : IRequiresRequestContext, IResolver, IDisposable, ICheckHealth
    {
        public IRequestContext RequestContext { get; set; }

        public T TryResolve<T>()
        {
            return EndpointHost.AppHost.TryResolve<T>();
        }

        private IHttpRequest request;
        protected virtual IHttpRequest HttpRequest
        {
            get { return request ?? (request = RequestContext != null ? RequestContext.Get<IHttpRequest>() : TryResolve<IHttpRequest>()); }
        }

        private IHttpResponse response;
        protected virtual IHttpResponse HttpResponse
        {
            get { return response ?? (response = RequestContext != null ? RequestContext.Get<IHttpResponse>() : TryResolve<IHttpResponse>()); }
        }

        private ICacheClient cache;
        public virtual ICacheClient Cache
        {
            get
            {
                return cache ?? (cache = TryResolve<ICacheClient>());
            }
        }

        private IDbConnection db;
        public virtual IDbConnection Db
        {
            get { return db ?? (db = TryResolve<IDbConnectionFactory>().OpenDbConnection()); }
        }

        public virtual void Dispose()
        {
            if (db != null)
                db.Dispose();
        }

        public virtual CheckHealthResponseType CheckHealth(CheckHealthRequestType request)
        {
            return new CheckHealthResponseType();
        }
    }
}
