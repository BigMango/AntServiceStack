using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using AntServiceStack.Common.Utils;

namespace AntServiceStack.Common
{
    public class HostContext
    {
        public static readonly HostContext Instance = new HostContext();

        [ThreadStatic]
        private static IDictionary items; //Thread Specific

        [ThreadStatic]
        private static IHttpRequest _request;

        [ThreadStatic]
        private static IHttpResponse _response;

        public static void InitRequest(IHttpRequest request, IHttpResponse response)
        {
            _request = request;
            _response = response;
        }

        /// <summary>
        /// Gets a list of items for this request. 
        /// </summary>
        /// <remarks>This list will be cleared on every request and is specific to the original thread that is handling the request.
        /// If a handler uses additional threads, this data will not be available on those threads.
        /// </remarks>
        public virtual IDictionary Items
        {
            get
            {
                return items ?? (HttpContext.Current != null
                    ? HttpContext.Current.Items
                    : items = new Dictionary<object, object>());
            }
            set { items = value; }
        }

        public T GetOrCreate<T>(Func<T> createFn)
        {
            if (Items.Contains(typeof(T).Name))
                return (T)Items[typeof(T).Name];

            return (T)(Items[typeof(T).Name] = createFn());
        }

        public IHttpRequest Request
        {
            get
            {
                return _request;
            }
        }

        public IHttpResponse Response
        {
            get
            {
                return _response;
            }
        }

        public void EndRequest()
        {
            items = null;
            _request = null;
            _response = null;
        }

        /// <summary>
        /// Track any IDisposable's to dispose of at the end of the request in IAppHost.OnEndRequest()
        /// </summary>
        /// <param name="instance"></param>
        public void TrackDisposable(IDisposable instance)
        {
            if (instance == null) return;

            //CService is already disposed right after it has been executed
            if (ServiceUtils.IsCSerivce(instance.GetType())) return;

            DispsableTracker dispsableTracker = null;
            if (!Items.Contains(DispsableTracker.HashId))
                Items[DispsableTracker.HashId] = dispsableTracker = new DispsableTracker();
            if (dispsableTracker == null)
                dispsableTracker = (DispsableTracker)Items[DispsableTracker.HashId];
            dispsableTracker.Add(instance);
        }
    }

    public class DispsableTracker : IDisposable
    {
        public const string HashId = "__disposables";

        List<WeakReference> disposables = new List<WeakReference>();

        public void Add(IDisposable instance)
        {
            disposables.Add(new WeakReference(instance));
        }

        public void Dispose()
        {
            foreach (var wr in disposables)
            {
                var disposable = (IDisposable)wr.Target;
                if (wr.IsAlive)
                    disposable.Dispose();
            }
        }
    }
}