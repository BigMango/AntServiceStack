using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Configuration;

using AntServiceStack.Text;
using AntServiceStack.ServiceHost;
using AntServiceStack.Common.Extensions;
using AntServiceStack.Common.Web;
using AntServiceStack.Common.Types;
using System.Net.NetworkInformation;
using Freeway.Logging;

namespace AntServiceStack.Common.Utils
{
    public static class ServiceUtils
    {
        private static ILog log = LogManager.GetLogger(typeof(ServiceUtils));

        public const string ServiceNamespacePrefix = "soa.ant.com.";

        public const string TRACE_ID_HTTP_HEADER = "CLOGGING_TRACE_ID";
        public const string SPAN_ID_HTTP_HEADER = "CLOGGING_SPAN_ID";

        public const string ESB_TRACE_ID_HTTP_HEADER = "Tracing-TraceId";
        public const string ESB_SPAN_ID_HTTP_HEADER = "Tracing-SpanId";

        public const string ESBServiceAppIdHttpHeaderKey = "ESB-Service-AppId";
        public const string ESBServiceHostIPHttpHeaderKey = "ESB-Service-HostIP";

        public const string AppIdHttpHeaderKey = "SOA20-Client-AppId";
        public const string AppIdESBClientHttpHeaderKey = "ClientAppId";

        public const string ServiceAppIdHttpHeaderKey = "SOA20-Service-AppId";

        public const string ServiceHostIPHttpHeaderKey = "SOA20-Service-HostIP";

        public const string ResponseStatusHttpHeaderKey = "SOA20-Response-Status";

        public const string H5GatewaySpecialHeaderName = "x-gate-request.toplevel.uuid";

        public const string H5GatewayRequestIPHeaderName = "x-gate-request.client.ip";

        public const string WebAPISpecialHeaderName = "x-webapi-request-toplevel-uuid";

        public const string WebAPIRequestIPHeaderName = "x-webapi-request-client-ip";

        public const string H5GatewayResponseDataHeaderPrefix = "x-gate-response.";

        public const string MobileUserIdExtensionKey = "uid";

        public const string MobileAuthTokenExtensionKey = "auth";

        public const string MobileUserPhoneExtensionKey = "uphone";

        public const string MobileIsMemberAuthExtensionKey = "IsMemberAuth";

        public const string MobileIsNonMemberAuthExtensionKey = "IsNonMemberAuth";

        public const string MobilePAuthErrorCodeExtensionKey = "PAuthErrorCode";

        public const string MobilePAuthErrorMessageExtensionKey = "PAuthErrorMessage";

        internal static List<string> MobileWriteBackExtensionKeys { get; private set; }

        public const string ServiceUniqueIdentityFormat = "{0}{{{1}}}";

        public const string CheckHealthOperationName = "CheckHealth";

        public const string ReservedErrorCodePrefix = "FXD300";

        private const string SOA2VersionCatKeyPrefix = "net-";

        public const string AsyncOperationReturnedNullTask = "Async operation returned null task!";

        public const string InvalidTokenExceptionMessage = "Invalid token for authentication.";

        public static string AppId { get; private set; }

        public static string HostIP { get; private set; }

        internal static string AntServiceStackVersion { get; private set; }

        internal static string SOA2VersionCatName { get; private set; }

        internal static string MachineName { get; private set; }

        static ServiceUtils()
        {
            MobileWriteBackExtensionKeys = new List<string>() { ServiceUtils.MobileAuthTokenExtensionKey };

            AppId = ConfigurationManager.AppSettings["AppId"];
            if (string.IsNullOrWhiteSpace(AppId))
                AppId = null;
            else
                AppId = AppId.Trim();

            AntServiceStackVersion = typeof(ServiceUtils).Assembly.GetName().Version.ToString();
            SOA2VersionCatName = SOA2VersionCatKeyPrefix + AntServiceStackVersion;

            HostIP = HostUtility.IPv4;
            MachineName = HostUtility.Name;
        }

        public static bool IsCheckHealthOperation(string operation)
        {
            return string.Equals(CheckHealthOperationName, operation, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// http://soa.ant.com/innovationwork/CloudBag/v1 变成  soa.ant.com.innovationwork.CloudBag.v1
        /// </summary>
        /// <param name="serviceNamespace"></param>
        /// <returns></returns>
        public static string ConvertNamespaceToMetricPrefix(string serviceNamespace)
        {
            // remove http prefix if having
            var prefix = serviceNamespace.Replace("http://", "");
            // remove https prefix if having
            prefix = prefix.Replace("https://", "");
            // replace all slash with dot
            prefix = prefix.ReplaceAll("/", ".");
            return prefix.ToLower();
        }

        /// <summary>
        ///  http://soa.ant.com/innovationwork/CloudBag/v1 变成  cloudbagrestfulapi.soa.ant.com.innovationwork.CloudBag.v1
        ///  
        /// </summary>
        /// <param name="serviceNamespace"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static string RefineServiceName(string serviceNamespace, string serviceName)
        {
            return (ConvertNamespaceToMetricPrefix(serviceNamespace) + "." + serviceName).ToLower().Replace(ServiceNamespacePrefix, string.Empty);
        }

        public static string GetServiceDomain(string refinedServiceName)
        {
            string[] parts = refinedServiceName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
                return parts[0];
            return null;
        }

        // Check if this service is supported by AntServiceStack
        public static bool IsCSerivce(Type type)
        {
            foreach (Type intf in type.GetInterfaces())
            {
                if (intf.HasAttribute<AntServiceInterfaceAttribute>())
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetServiceUniqueIdentity(string serviceName, string serviceNamespace)
        {
            return string.Format(ServiceUniqueIdentityFormat, serviceName, serviceNamespace);
        }

        public static string GetIPV4Address(string userHostAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userHostAddress))
                    return userHostAddress;
                if (!userHostAddress.Contains(':'))
                    return userHostAddress;

                IPAddress[] addresses = Dns.GetHostAddresses(userHostAddress);
                foreach (IPAddress ip in addresses)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }

                IPHostEntry hostEntry = Dns.GetHostEntry(userHostAddress);
                foreach (IPAddress ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        public static string GetClientAppId(this HttpRequest request)
        {
            return GetClientAppId(request.Headers);
        }

        public static string GetClientAppId(this IHttpRequest request)
        {
            return GetClientAppId(request.Headers);
        }

        private static string GetClientAppId(NameValueCollection headers)
        {
            if (headers == null)
                return null;

            string clientAppId = headers[AppIdHttpHeaderKey];
            if (string.IsNullOrWhiteSpace(clientAppId))
            {
                clientAppId = headers[CatConstants.CALL_APP];
                if (string.IsNullOrWhiteSpace(clientAppId))
                {
                    clientAppId = headers[AppIdESBClientHttpHeaderKey];
                    if (string.IsNullOrWhiteSpace(clientAppId))
                        clientAppId = null;
                }
            }

            return clientAppId;
        }

        public static string GetClientIP(this HttpRequest request)
        {
            string ipAddress;
            if (request.Headers[ServiceUtils.H5GatewaySpecialHeaderName] != null 
                && !string.IsNullOrWhiteSpace(request.Headers[ServiceUtils.H5GatewayRequestIPHeaderName]))
                ipAddress = request.Headers[ServiceUtils.H5GatewayRequestIPHeaderName];
            else if (request.Headers[ServiceUtils.WebAPISpecialHeaderName] != null 
                && !string.IsNullOrWhiteSpace(request.Headers[ServiceUtils.WebAPIRequestIPHeaderName]))
                ipAddress = request.Headers[ServiceUtils.WebAPIRequestIPHeaderName];
            else
                ipAddress = request.Headers[HttpHeaders.XForwardedFor] ?? (request.Headers[HttpHeaders.XRealIp] ?? request.UserHostAddress);
            return GetIPV4Address(ipAddress) ?? ipAddress;
        }

        public static string GetClientIP(this IHttpRequest request)
        {
            return request.RemoteIp;
        }

        public static void AddExtensionData(this IHasResponseStatus response, string key, string value)
        {
            if (response == null || key == null)
                return;

            if (response.ResponseStatus == null)
                response.ResponseStatus = new ResponseStatusType();

            if (response.ResponseStatus.Extension == null)
                response.ResponseStatus.Extension = new List<ExtensionType>();

            List<ExtensionType> existed = response.ResponseStatus.Extension.Where(i => i != null && i.Id == key).ToList();
            foreach (ExtensionType exitedItem in existed)
                response.ResponseStatus.Extension.Remove(exitedItem);

            response.ResponseStatus.Extension.Add(
                new ExtensionType()
                {
                    Id = key,
                    Value = value
                });
        }

        public static void AddExtensionData(this IHasMobileRequestHead request, string name, string value)
        {
            if (request == null || name == null)
                return;

            if (request.head == null)
                request.head = new MobileRequestHead();

            if (request.head.extension == null)
                request.head.extension = new List<ExtensionFieldType>();

            List<ExtensionFieldType> existed = request.head.extension.Where(i => i != null && i.name == name).ToList();
            foreach (ExtensionFieldType exitedItem in existed)
                request.head.extension.Remove(exitedItem);

            request.head.extension.Add(
                new ExtensionFieldType()
                {
                    name = name,
                    value = value
                });
        }

        public static string GetExtensionData(this IHasMobileRequestHead request, string name)
        {
            if (request == null || request.head == null || request.head.extension == null || name == null)
                return null;

            ExtensionFieldType field = request.head.extension.Where(i => i != null && i.name == name).LastOrDefault();
            if (field == null)
                return null;
            return field.value;
        }

        public static bool HasExtensionData(this IHasMobileRequestHead request, string name)
        {
            return GetExtensionData(request, name) != null;
        }

        public static bool IsH5GatewayRequest(this HttpRequest request)
        {
            return request.Headers[H5GatewaySpecialHeaderName] != null;
        }

        public static bool IsH5GatewayRequest(this IHttpRequest request)
        {
            return request.Headers[H5GatewaySpecialHeaderName] != null;
        }

        public static Dictionary<string, string> AddErrorCode(this Dictionary<string, string> tagData, string errorCode)
        {
            if (tagData == null)
                tagData = new Dictionary<string, string>();

            tagData["ErrorCode"] = errorCode;
            return tagData;
        }

        public static bool IsMemberAuth(this IHasMobileRequestHead request)
        {
            return GetExtensionData(request, MobileIsMemberAuthExtensionKey) == bool.TrueString;
        }

        public static bool IsNonMemberAuth(this IHasMobileRequestHead request)
        {
            return GetExtensionData(request, MobileIsNonMemberAuthExtensionKey) == bool.TrueString;
        }

        /// <summary>
        /// 此方法不是线程安全的，只能在初始化时调用，请勿在并发情况下调用
        /// </summary>
        /// <param name="extensionKey"></param>
        public static void RegisterMobileWriteBackExtensionKey(string extensionKey)
        {
            if (MobileWriteBackExtensionKeys.Contains(extensionKey))
                return;
            MobileWriteBackExtensionKeys.Add(extensionKey);
        }
    }
}
