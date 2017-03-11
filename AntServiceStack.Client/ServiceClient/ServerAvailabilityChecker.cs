using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Freeway.Logging;
using AntServiceStack.Client.ServiceClient;

namespace AntServiceStack.ServiceClient
{
    class ServerAvailabilityChecker
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ServerAvailabilityChecker));

        private static readonly HashSet<int> _defaultUnavailableHttpStatusCodes;

        private static readonly HashSet<WebExceptionStatus> _defaultUnavailableWebExceptionStatus;

        private static readonly HashSet<string> _defaultUnavailableExceptionNames;

        static ServerAvailabilityChecker()
        {
            _defaultUnavailableHttpStatusCodes = new HashSet<int>() { 403, 404, 405 };

            _defaultUnavailableWebExceptionStatus = new HashSet<WebExceptionStatus>() { WebExceptionStatus.NameResolutionFailure, WebExceptionStatus.ConnectFailure };

            _defaultUnavailableExceptionNames = new HashSet<string>();
        }

        private static HashSet<int> ParseHttpStatusCodes(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var set = new HashSet<int>();
            string[] parts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int statusCode;
            foreach (var part in parts)
            {
                var trimmedValue = part.Trim();
                if (int.TryParse(trimmedValue, out statusCode))
                    set.Add(statusCode);
                else
                {
                    log.Warn("Error occurred while parse HttpStatusCode: " + trimmedValue);
                    continue;
                }
            }
            return set;
        }

        private static HashSet<WebExceptionStatus> ParseWebExceptionStatus(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            
            var set = new HashSet<WebExceptionStatus>();
            string[] parts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            WebExceptionStatus webExceptionStatus;
            foreach (var part in parts)
            {
                var trimmedValue = part.Trim();
                if (Enum.TryParse<WebExceptionStatus>(trimmedValue, out webExceptionStatus))
                    set.Add(webExceptionStatus);
                else
                {
                    log.Warn("Error occurred while parse WebExceptionStatus: " + trimmedValue);
                    continue;
                }
            }
            return set;
        }

        public static void CheckServerAvailability(ClientExecutionContext context, Exception exception = null)
        {
            try
            {
                if (exception == null)
                {
                    return;
                }

             

                if (exception is WebException)
                {
                    var webException = (WebException)exception;
                    bool available = webException.Status != WebExceptionStatus.ProtocolError ? 
                        CheckGenericWebException(context, webException) : 
                        CheckProtocolWebException(context, webException);

                    if (!available)
                    {
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                log.Warn("Failed to check server availability", ex);
            }
        }

        private static bool CheckGenericWebException(ClientExecutionContext context, WebException webException)
        {
            return false;
        }

        private static bool CheckProtocolWebException(ClientExecutionContext context, WebException webException)
        {
            var httpResponse = webException.Response as HttpWebResponse;
            if (httpResponse == null)
                return false;

            int statusCode = (int)httpResponse.StatusCode;
            return false;
        }
    }
}
