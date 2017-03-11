namespace CHystrix.Utils.Extensions
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class WebRequestExtensions
    {
        public const string FormUrlEncoded = "application/x-www-form-urlencoded";
        public const string Json = "application/json";
        public const string MultiPartFormData = "multipart/form-data";
        public const string Xml = "application/xml";

        public static string DeleteFromUrl(this string url, string acceptContentType = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string method = "DELETE";
            string str2 = acceptContentType;
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendStringToUrl(method, null, null, str2, action, action2);
        }

        public static byte[] GetBytesFromUrl(this string url, string acceptContentType = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string str = acceptContentType;
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendBytesToUrl(null, null, null, str, action, action2);
        }

        public static string GetJsonFromUrl(this string url, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrl("application/json", requestFilter, responseFilter);
        }

        public static string GetResponseBody(this Exception ex)
        {
            WebException exception = ex as WebException;
            if ((exception == null) || (exception.Status != WebExceptionStatus.ProtocolError))
            {
                return null;
            }
            HttpWebResponse response = (HttpWebResponse) exception.Response;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }

        public static HttpStatusCode? GetResponseStatus(this string url)
        {
            HttpStatusCode? status;
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
                using (WebResponse response = request.GetResponse())
                {
                    HttpWebResponse response2 = response as HttpWebResponse;
                    status = (response2 != null) ? new HttpStatusCode?(response2.StatusCode) : null;
                }
            }
            catch (Exception exception)
            {
                status = exception.GetStatus();
            }
            return status;
        }

        public static HttpStatusCode? GetStatus(this Exception ex)
        {
            return (ex as WebException).GetStatus();
        }

        public static HttpStatusCode? GetStatus(this WebException webEx)
        {
            if (webEx == null)
            {
                return null;
            }
            HttpWebResponse response = webEx.Response as HttpWebResponse;
            if (response == null)
            {
                return null;
            }
            return new HttpStatusCode?(response.StatusCode);
        }

        public static string GetStringFromUrl(this string url, string acceptContentType = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string str = acceptContentType;
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendStringToUrl(null, null, null, str, action, action2);
        }

        public static string GetXmlFromUrl(this string url, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrl("application/xml", requestFilter, responseFilter);
        }

        public static bool HasStatus(this WebException webEx, HttpStatusCode statusCode)
        {
            HttpStatusCode? status = webEx.GetStatus();
            HttpStatusCode code = statusCode;
            return ((((HttpStatusCode) status.GetValueOrDefault()) == code) && status.HasValue);
        }

        public static string HeadFromUrl(this string url, string acceptContentType = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string method = "HEAD";
            string str2 = acceptContentType;
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendStringToUrl(method, null, null, str2, action, action2);
        }

        public static bool IsAny300(this Exception ex)
        {
            HttpStatusCode? status = ex.GetStatus();
            if (((HttpStatusCode) status) < HttpStatusCode.MultipleChoices)
            {
                return false;
            }
            return (((HttpStatusCode) status) < HttpStatusCode.BadRequest);
        }

        public static bool IsAny400(this Exception ex)
        {
            HttpStatusCode? status = ex.GetStatus();
            if (((HttpStatusCode) status) < HttpStatusCode.BadRequest)
            {
                return false;
            }
            return (((HttpStatusCode) status) < HttpStatusCode.InternalServerError);
        }

        public static bool IsAny500(this Exception ex)
        {
            HttpStatusCode? status = ex.GetStatus();
            return ((((HttpStatusCode) status) >= HttpStatusCode.InternalServerError) && (((HttpStatusCode) status.Value) < ((HttpStatusCode) 600)));
        }

        public static bool IsBadRequest(this Exception ex)
        {
            return (ex as WebException).HasStatus(HttpStatusCode.BadRequest);
        }

        public static bool IsForbidden(this Exception ex)
        {
            return (ex as WebException).HasStatus(HttpStatusCode.Forbidden);
        }

        public static bool IsInternalServerError(this Exception ex)
        {
            return (ex as WebException).HasStatus(HttpStatusCode.InternalServerError);
        }

        public static bool IsNotFound(this Exception ex)
        {
            return (ex as WebException).HasStatus(HttpStatusCode.NotFound);
        }

        public static bool IsUnauthorized(this Exception ex)
        {
            return (ex as WebException).HasStatus(HttpStatusCode.Unauthorized);
        }

        public static string OptionsFromUrl(this string url, string acceptContentType = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string method = "OPTIONS";
            string str2 = acceptContentType;
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendStringToUrl(method, null, null, str2, action, action2);
        }

        public static byte[] PostBytesToUrl(this string url, byte[] requestBody = null, string contentType = null, string acceptContentType = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string method = "POST";
            string str2 = contentType;
            byte[] buffer = requestBody;
            string str3 = acceptContentType;
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendBytesToUrl(method, buffer, str2, str3, action, action2);
        }

        public static string PostJsonToUrl(this string url, object data, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string method = "POST";
            string requestBody = data.ToJson();
            string contentType = "application/json";
            string acceptContentType = "application/json";
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendStringToUrl(method, requestBody, contentType, acceptContentType, action, action2);
        }

        public static string PostJsonToUrl(this string url, string json, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string method = "POST";
            string requestBody = json;
            string contentType = "application/json";
            string acceptContentType = "application/json";
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendStringToUrl(method, requestBody, contentType, acceptContentType, action, action2);
        }

        public static string PostStringToUrl(this string url, string requestBody = null, string contentType = null, string acceptContentType = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string method = "POST";
            string str2 = requestBody;
            string str3 = contentType;
            string str4 = acceptContentType;
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendStringToUrl(method, str2, str3, str4, action, action2);
        }

        public static string PostToUrl(this string url, string formData = null, string acceptContentType = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string method = "POST";
            string contentType = "application/x-www-form-urlencoded";
            string requestBody = formData;
            string str4 = acceptContentType;
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendStringToUrl(method, requestBody, contentType, str4, action, action2);
        }

        public static string PostXmlToUrl(this string url, string xml, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string method = "POST";
            string requestBody = xml;
            string contentType = "application/xml";
            string acceptContentType = "application/xml";
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendStringToUrl(method, requestBody, contentType, acceptContentType, action, action2);
        }

        public static byte[] PutBytesToUrl(this string url, byte[] requestBody = null, string contentType = null, string acceptContentType = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string method = "PUT";
            string str2 = contentType;
            byte[] buffer = requestBody;
            string str3 = acceptContentType;
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendBytesToUrl(method, buffer, str2, str3, action, action2);
        }

        public static string PutJsonToUrl(this string url, object data, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string method = "PUT";
            string requestBody = data.ToJson();
            string contentType = "application/json";
            string acceptContentType = "application/json";
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendStringToUrl(method, requestBody, contentType, acceptContentType, action, action2);
        }

        public static string PutJsonToUrl(this string url, string json, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string method = "PUT";
            string requestBody = json;
            string contentType = "application/json";
            string acceptContentType = "application/json";
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendStringToUrl(method, requestBody, contentType, acceptContentType, action, action2);
        }

        public static string PutStringToUrl(this string url, string requestBody = null, string contentType = null, string acceptContentType = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string method = "PUT";
            string str2 = requestBody;
            string str3 = contentType;
            string str4 = acceptContentType;
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendStringToUrl(method, str2, str3, str4, action, action2);
        }

        public static string PutToUrl(this string url, string formData = null, string acceptContentType = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string method = "PUT";
            string contentType = "application/x-www-form-urlencoded";
            string requestBody = formData;
            string str4 = acceptContentType;
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendStringToUrl(method, requestBody, contentType, str4, action, action2);
        }

        public static string PutXmlToUrl(this string url, string xml, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string method = "PUT";
            string requestBody = xml;
            string contentType = "application/xml";
            string acceptContentType = "application/xml";
            Action<HttpWebRequest> action = requestFilter;
            Action<HttpWebResponse> action2 = responseFilter;
            return url.SendStringToUrl(method, requestBody, contentType, acceptContentType, action, action2);
        }

        public static byte[] SendBytesToUrl(this string url, string method = null, byte[] requestBody = null, string contentType = null, string acceptContentType = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            byte[] buffer;
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            if (method != null)
            {
                request.Method = method;
            }
            if (contentType != null)
            {
                request.ContentType = contentType;
            }
            request.Accept = acceptContentType;
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            if (requestFilter != null)
            {
                requestFilter(request);
            }
            if (requestBody != null)
            {
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(requestBody, 0, requestBody.Length);
                }
            }
            using (WebResponse response = request.GetResponse())
            {
                if (responseFilter != null)
                {
                    responseFilter((HttpWebResponse) response);
                }
                using (Stream stream2 = response.GetResponseStream())
                {
                    buffer = stream2.ReadFully();
                }
            }
            return buffer;
        }

        public static string SendStringToUrl(this string url, string method = null, string requestBody = null, string contentType = null, string acceptContentType = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string str;
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            if (method != null)
            {
                request.Method = method;
            }
            if (contentType != null)
            {
                request.ContentType = contentType;
            }
            request.Accept = acceptContentType;
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            if (requestFilter != null)
            {
                requestFilter(request);
            }
            if (requestBody != null)
            {
                using (Stream stream = request.GetRequestStream())
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(requestBody);
                    }
                }
            }
            using (WebResponse response = request.GetResponse())
            {
                using (Stream stream2 = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream2))
                    {
                        if (responseFilter != null)
                        {
                            responseFilter((HttpWebResponse) response);
                        }
                        str = reader.ReadToEnd();
                    }
                }
            }
            return str;
        }

        public static string ToFormUrlEncoded(this NameValueCollection queryParams)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string str in queryParams)
            {
                string[] values = queryParams.GetValues(str);
                if (values != null)
                {
                    foreach (string str2 in values)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append('&');
                        }
                        builder.AppendFormat("{0}={1}", str.UrlEncode(), str2.UrlEncode());
                    }
                }
            }
            return builder.ToString();
        }
    }
}

