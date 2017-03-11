using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace AntServiceStack.Baiji
{
    public class JsonConfig
    {
        private const bool DefaultIncludeNullValues = false;

        private static bool? includeNullValues;
        public static bool IncludeNullValues 
        {
            get
            {
                bool? currentScopeValue = JsonConfigScope.Current != null ? JsonConfigScope.Current.IncludeNullValues : null;
                return currentScopeValue ?? includeNullValues ?? DefaultIncludeNullValues;
            }
            set
            {
                if (!includeNullValues.HasValue)
                    includeNullValues = value;
            }
        }

        public static JsonConfigScope BeginScope()
        {
            return new JsonConfigScope();
        }

        internal static void Reset()
        {
            includeNullValues = null;
        }
    }

    public class JsonConfigScope : IDisposable
    {
        bool disposed;
        JsonConfigScope previous;

        [ThreadStatic]
        private static JsonConfigScope current;

        internal JsonConfigScope()
        {
#if !SILVERLIGHT
            Thread.BeginThreadAffinity();
#endif
            previous = current;
            current = this;
        }

        internal static JsonConfigScope Current
        {
            get
            {
                return current;
            }
        }

        public static void DisposeCurrent()
        {
            if (current != null)
            {
                current.Dispose();
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                Debug.Assert(this == current, "Disposed out of order.");

                current = previous;
#if !SILVERLIGHT
                Thread.EndThreadAffinity();
#endif
            }
        }

        public bool? IncludeNullValues { get; set; }
    }
}
