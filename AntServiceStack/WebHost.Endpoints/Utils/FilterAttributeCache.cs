using System;
using System.Collections.Generic;
using AntServiceStack.ServiceHost;
using System.Threading;

namespace AntServiceStack.WebHost.Endpoints.Utils
{
    public static class FilterAttributeCache
    {
        private static Dictionary<string, IHasRequestFilter[]> requestFilterAttributes
            = new Dictionary<string, IHasRequestFilter[]>();

        private static Dictionary<string, IHasResponseFilter[]> responseFilterAttributes
            = new Dictionary<string, IHasResponseFilter[]>();

        private static IHasRequestFilter[] ShallowCopy(this IHasRequestFilter[] filters)
        {
            var to = new IHasRequestFilter[filters.Length];
            for (int i = 0; i < filters.Length; i++)
            {
                to[i] = filters[i].Copy();
            }
            return to;
        }

        private static IHasResponseFilter[] ShallowCopy(this IHasResponseFilter[] filters)
        {
            var to = new IHasResponseFilter[filters.Length];
            for (int i = 0; i < filters.Length; i++)
            {
                to[i] = filters[i].Copy();
            }
            return to;
        }

        public static IHasRequestFilter[] GetRequestFilterAttributes(string servicePath, string operationName)
        {
            IHasRequestFilter[] attrs;
            if (requestFilterAttributes.TryGetValue(operationName, out attrs)) return attrs.ShallowCopy();

            Operation op = EndpointHost.Config.MetadataMap[servicePath].GetOperationByOpName(operationName);
            var attributes = op.RequestFilters;

            attributes.Sort((x, y) => x.Priority - y.Priority);
            attrs = attributes.ToArray();

            Dictionary<string, IHasRequestFilter[]> snapshot, newCache;
            do
            {
                snapshot = requestFilterAttributes;
                newCache = new Dictionary<string, IHasRequestFilter[]>(requestFilterAttributes);
                newCache[operationName] = attrs;

            } while (!ReferenceEquals(
            Interlocked.CompareExchange(ref requestFilterAttributes, newCache, snapshot), snapshot));

            return attrs.ShallowCopy();
        }

        public static IHasResponseFilter[] GetResponseFilterAttributes(string servicePath, string operationName)
        {
            IHasResponseFilter[] attrs;
            if (responseFilterAttributes.TryGetValue(operationName, out attrs)) return attrs.ShallowCopy();

            Operation op = EndpointHost.Config.MetadataMap[servicePath].GetOperationByOpName(operationName);
            var attributes = op.ResponseFilters;

            attributes.Sort((x, y) => x.Priority - y.Priority);
            attrs = attributes.ToArray();

            Dictionary<string, IHasResponseFilter[]> snapshot, newCache;
            do
            {
                snapshot = responseFilterAttributes;
                newCache = new Dictionary<string, IHasResponseFilter[]>(responseFilterAttributes);
                newCache[operationName] = attrs;

            } while (!ReferenceEquals(
            Interlocked.CompareExchange(ref responseFilterAttributes, newCache, snapshot), snapshot));

            return attrs.ShallowCopy();
        }
    }
}
