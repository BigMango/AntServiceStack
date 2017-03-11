//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;

//namespace AntServiceStack.Text
//{
//    public static class HttpUtils
//    {
//        public static string UserAgent = "AntServiceStack.Text";
//        [ThreadStatic]
//        public static IHttpResultsFilter ResultsFilter;

//        public static string AddQueryParam(this string url, string key, object val, bool encode = true)
//        {
//            return HttpUtils.AddQueryParam(url, key, val.ToString(), encode);
//        }

//        public static string AddQueryParam(this string url, object key, string val, bool encode = true)
//        {
//            return HttpUtils.AddQueryParam(url, (key ?? (object)"").ToString(), val, encode);
//        }

//        public static string AddQueryParam(this string url, string key, string val, bool encode = true)
//        {
//            if (string.IsNullOrEmpty(url))
//                return (string)null;
//            string str = url.IndexOf('?') == -1 ? "?" : "&";
//            return url + str + key + "=" + (encode ? val.UrlEncode(false) : val);
//        }

//        public static string SetQueryParam(this string url, string key, string val)
//        {
//            if (string.IsNullOrEmpty(url))
//                return (string)null;
//            int startIndex1 = url.IndexOf('?');
//            if (startIndex1 != -1)
//            {
//                int num = startIndex1 + 1 == url.IndexOf(key, startIndex1, PclExport.Instance.InvariantComparison) ? startIndex1 : url.IndexOf("&" + key, startIndex1, PclExport.Instance.InvariantComparison);
//                if (num != -1)
//                {
//                    int startIndex2 = url.IndexOf('&', num + 1);
//                    if (startIndex2 == -1)
//                        startIndex2 = url.Length;
//                    return url.Substring(0, num + key.Length + 1) + "=" + val.UrlEncode(false) + url.Substring(startIndex2);
//                }
//            }
//            string str = startIndex1 == -1 ? "?" : "&";
//            return url + str + key + "=" + val.UrlEncode(false);
//        }

//        public static string AddHashParam(this string url, string key, object val)
//        {
//            return HttpUtils.AddHashParam(url, key, val.ToString());
//        }

//        public static string AddHashParam(this string url, string key, string val)
//        {
//            if (string.IsNullOrEmpty(url))
//                return (string)null;
//            string str = url.IndexOf('#') == -1 ? "#" : "/";
//            return url + str + key + "=" + val.UrlEncode(false);
//        }

//        public static string SetHashParam(this string url, string key, string val)
//        {
//            if (string.IsNullOrEmpty(url))
//                return (string)null;
//            int startIndex1 = url.IndexOf('#');
//            if (startIndex1 != -1)
//            {
//                int num = startIndex1 + 1 == url.IndexOf(key, startIndex1, PclExport.Instance.InvariantComparison) ? startIndex1 : url.IndexOf("/" + key, startIndex1, PclExport.Instance.InvariantComparison);
//                if (num != -1)
//                {
//                    int startIndex2 = url.IndexOf('/', num + 1);
//                    if (startIndex2 == -1)
//                        startIndex2 = url.Length;
//                    return url.Substring(0, num + key.Length + 1) + "=" + val.UrlEncode(false) + url.Substring(startIndex2);
//                }
//            }
//            string str = url.IndexOf('#') == -1 ? "#" : "/";
//            return url + str + key + "=" + val.UrlEncode(false);
//        }

//        public static string GetJsonFromUrl(this string url, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.GetStringFromUrl("application/json", requestFilter, responseFilter);
//        }

//        public static Task<string> GetJsonFromUrlAsync(this string url, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.GetStringFromUrlAsync("application/json", requestFilter, responseFilter);
//        }

//        public static string GetXmlFromUrl(this string url, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.GetStringFromUrl("application/xml", requestFilter, responseFilter);
//        }

//        public static Task<string> GetXmlFromUrlAsync(this string url, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.GetStringFromUrlAsync("application/xml", requestFilter, responseFilter);
//        }

//        public static string GetCsvFromUrl(this string url, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.GetStringFromUrl("text/csv", requestFilter, responseFilter);
//        }

//        public static Task<string> GetCsvFromUrlAsync(this string url, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.GetStringFromUrlAsync("text/csv", requestFilter, responseFilter);
//        }

//        public static string GetStringFromUrl(this string url, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl((string)null, (string)null, (string)null, accept, requestFilter, responseFilter);
//        }

//        public static Task<string> GetStringFromUrlAsync(this string url, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrlAsync((string)null, (string)null, (string)null, accept, requestFilter, responseFilter);
//        }

//        public static string PostStringToUrl(this string url, string requestBody = null, string contentType = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("POST", requestBody, contentType, accept, requestFilter, responseFilter);
//        }

//        public static Task<string> PostStringToUrlAsync(this string url, string requestBody = null, string contentType = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrlAsync("POST", requestBody, contentType, accept, requestFilter, responseFilter);
//        }

//        public static string PostToUrl(this string url, string formData = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            string url1 = url;
//            string method = "POST";
//            string str = "application/x-www-form-urlencoded";
//            string requestBody = formData;
//            string contentType = str;
//            string accept1 = accept;
//            Action<HttpWebRequest> requestFilter1 = requestFilter;
//            Action<HttpWebResponse> responseFilter1 = responseFilter;
//            return url1.SendStringToUrl(method, requestBody, contentType, accept1, requestFilter1, responseFilter1);
//        }

//        public static Task<string> PostToUrlAsync(this string url, string formData = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            string url1 = url;
//            string method = "POST";
//            string str = "application/x-www-form-urlencoded";
//            string requestBody = formData;
//            string contentType = str;
//            string accept1 = accept;
//            Action<HttpWebRequest> requestFilter1 = requestFilter;
//            Action<HttpWebResponse> responseFilter1 = responseFilter;
//            return url1.SendStringToUrlAsync(method, requestBody, contentType, accept1, requestFilter1, responseFilter1);
//        }

//        public static string PostToUrl(this string url, object formData = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            string str1 = formData != null ? QueryStringSerializer.SerializeToString<object>(formData) : (string)null;
//            string url1 = url;
//            string method = "POST";
//            string str2 = "application/x-www-form-urlencoded";
//            string requestBody = str1;
//            string contentType = str2;
//            string accept1 = accept;
//            Action<HttpWebRequest> requestFilter1 = requestFilter;
//            Action<HttpWebResponse> responseFilter1 = responseFilter;
//            return url1.SendStringToUrl(method, requestBody, contentType, accept1, requestFilter1, responseFilter1);
//        }

//        public static Task<string> PostToUrlAsync(this string url, object formData = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            string str1 = formData != null ? QueryStringSerializer.SerializeToString<object>(formData) : (string)null;
//            string url1 = url;
//            string method = "POST";
//            string str2 = "application/x-www-form-urlencoded";
//            string requestBody = str1;
//            string contentType = str2;
//            string accept1 = accept;
//            Action<HttpWebRequest> requestFilter1 = requestFilter;
//            Action<HttpWebResponse> responseFilter1 = responseFilter;
//            return url1.SendStringToUrlAsync(method, requestBody, contentType, accept1, requestFilter1, responseFilter1);
//        }

//        public static string PostJsonToUrl(this string url, string json, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("POST", json, "application/json", "application/json", requestFilter, responseFilter);
//        }

//        public static Task<string> PostJsonToUrlAsync(this string url, string json, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrlAsync("POST", json, "application/json", "application/json", requestFilter, responseFilter);
//        }

//        public static string PostJsonToUrl(this string url, object data, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("POST", data.ToJson<object>(), "application/json", "application/json", requestFilter, responseFilter);
//        }

//        public static Task<string> PostJsonToUrlAsync(this string url, object data, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrlAsync("POST", data.ToJson<object>(), "application/json", "application/json", requestFilter, responseFilter);
//        }

//        public static string PostXmlToUrl(this string url, string xml, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("POST", xml, "application/xml", "application/xml", requestFilter, responseFilter);
//        }

//        public static Task<string> PostXmlToUrlAsync(this string url, string xml, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrlAsync("POST", xml, "application/xml", "application/xml", requestFilter, responseFilter);
//        }

//        public static string PostCsvToUrl(this string url, string csv, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("POST", csv, "text/csv", "text/csv", requestFilter, responseFilter);
//        }

//        public static Task<string> PostCsvToUrlAsync(this string url, string csv, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrlAsync("POST", csv, "text/csv", "text/csv", requestFilter, responseFilter);
//        }

//        public static string PutStringToUrl(this string url, string requestBody = null, string contentType = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("PUT", requestBody, contentType, accept, requestFilter, responseFilter);
//        }

//        public static Task<string> PutStringToUrlAsync(this string url, string requestBody = null, string contentType = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrlAsync("PUT", requestBody, contentType, accept, requestFilter, responseFilter);
//        }

//        public static string PutToUrl(this string url, string formData = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            string url1 = url;
//            string method = "PUT";
//            string str = "application/x-www-form-urlencoded";
//            string requestBody = formData;
//            string contentType = str;
//            string accept1 = accept;
//            Action<HttpWebRequest> requestFilter1 = requestFilter;
//            Action<HttpWebResponse> responseFilter1 = responseFilter;
//            return url1.SendStringToUrl(method, requestBody, contentType, accept1, requestFilter1, responseFilter1);
//        }

//        public static Task<string> PutToUrlAsync(this string url, string formData = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            string url1 = url;
//            string method = "PUT";
//            string str = "application/x-www-form-urlencoded";
//            string requestBody = formData;
//            string contentType = str;
//            string accept1 = accept;
//            Action<HttpWebRequest> requestFilter1 = requestFilter;
//            Action<HttpWebResponse> responseFilter1 = responseFilter;
//            return url1.SendStringToUrlAsync(method, requestBody, contentType, accept1, requestFilter1, responseFilter1);
//        }

//        public static string PutToUrl(this string url, object formData = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            string str1 = formData != null ? QueryStringSerializer.SerializeToString<object>(formData) : (string)null;
//            string url1 = url;
//            string method = "PUT";
//            string str2 = "application/x-www-form-urlencoded";
//            string requestBody = str1;
//            string contentType = str2;
//            string accept1 = accept;
//            Action<HttpWebRequest> requestFilter1 = requestFilter;
//            Action<HttpWebResponse> responseFilter1 = responseFilter;
//            return url1.SendStringToUrl(method, requestBody, contentType, accept1, requestFilter1, responseFilter1);
//        }

//        public static Task<string> PutToUrlAsync(this string url, object formData = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            string str1 = formData != null ? QueryStringSerializer.SerializeToString<object>(formData) : (string)null;
//            string url1 = url;
//            string method = "PUT";
//            string str2 = "application/x-www-form-urlencoded";
//            string requestBody = str1;
//            string contentType = str2;
//            string accept1 = accept;
//            Action<HttpWebRequest> requestFilter1 = requestFilter;
//            Action<HttpWebResponse> responseFilter1 = responseFilter;
//            return url1.SendStringToUrlAsync(method, requestBody, contentType, accept1, requestFilter1, responseFilter1);
//        }

//        public static string PutJsonToUrl(this string url, string json, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("PUT", json, "application/json", "application/json", requestFilter, responseFilter);
//        }

//        public static Task<string> PutJsonToUrlAsync(this string url, string json, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrlAsync("PUT", json, "application/json", "application/json", requestFilter, responseFilter);
//        }

//        public static string PutJsonToUrl(this string url, object data, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("PUT", data.ToJson<object>(), "application/json", "application/json", requestFilter, responseFilter);
//        }

//        public static Task<string> PutJsonToUrlAsync(this string url, object data, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrlAsync("PUT", data.ToJson<object>(), "application/json", "application/json", requestFilter, responseFilter);
//        }

//        public static string PutXmlToUrl(this string url, string xml, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("PUT", xml, "application/xml", "application/xml", requestFilter, responseFilter);
//        }

//        public static Task<string> PutXmlToUrlAsync(this string url, string xml, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrlAsync("PUT", xml, "application/xml", "application/xml", requestFilter, responseFilter);
//        }

//        public static string PutCsvToUrl(this string url, string csv, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("PUT", csv, "text/csv", "text/csv", requestFilter, responseFilter);
//        }

//        public static Task<string> PutCsvToUrlAsync(this string url, string csv, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrlAsync("PUT", csv, "text/csv", "text/csv", requestFilter, responseFilter);
//        }

//        public static string DeleteFromUrl(this string url, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("DELETE", (string)null, (string)null, accept, requestFilter, responseFilter);
//        }

//        public static Task<string> DeleteFromUrlAsync(this string url, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrlAsync("DELETE", (string)null, (string)null, accept, requestFilter, responseFilter);
//        }

//        public static string OptionsFromUrl(this string url, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("OPTIONS", (string)null, (string)null, accept, requestFilter, responseFilter);
//        }

//        public static Task<string> OptionsFromUrlAsync(this string url, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrlAsync("OPTIONS", (string)null, (string)null, accept, requestFilter, responseFilter);
//        }

//        public static string HeadFromUrl(this string url, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("HEAD", (string)null, (string)null, accept, requestFilter, responseFilter);
//        }

//        public static Task<string> HeadFromUrlAsync(this string url, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrlAsync("HEAD", (string)null, (string)null, accept, requestFilter, responseFilter);
//        }

//        public static string SendStringToUrl(this string url, string method = null, string requestBody = null, string contentType = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
//            if (method != null)
//                webReq.Method = method;
//            if (contentType != null)
//                webReq.ContentType = contentType;
//            webReq.Accept = accept;
//            PclExport.Instance.AddCompression((WebRequest)webReq);
//            if (requestFilter != null)
//                requestFilter(webReq);
//            if (HttpUtils.ResultsFilter != null)
//                return HttpUtils.ResultsFilter.GetString(webReq, requestBody);
//            if (requestBody != null)
//            {
//                using (Stream requestStream = PclExport.Instance.GetRequestStream((WebRequest)webReq))
//                {
//                    using (StreamWriter streamWriter = new StreamWriter(requestStream))
//                        streamWriter.Write(requestBody);
//                }
//            }
//            using (WebResponse response = PclExport.Instance.GetResponse((WebRequest)webReq))
//            {
//                using (Stream responseStream = response.GetResponseStream())
//                {
//                    using (StreamReader streamReader = new StreamReader(responseStream))
//                    {
//                        if (responseFilter != null)
//                            responseFilter((HttpWebResponse)response);
//                        return streamReader.ReadToEnd();
//                    }
//                }
//            }
//        }

//        public static Task<string> SendStringToUrlAsync(this string url, string method = null, string requestBody = null, string contentType = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
//            if (method != null)
//                httpWebRequest.Method = method;
//            if (contentType != null)
//                httpWebRequest.ContentType = contentType;
//            httpWebRequest.Accept = accept;
//            PclExport.Instance.AddCompression((WebRequest)httpWebRequest);
//            if (requestFilter != null)
//                requestFilter(httpWebRequest);
//            if (HttpUtils.ResultsFilter != null)
//            {
//                string @string = HttpUtils.ResultsFilter.GetString(httpWebRequest, requestBody);
//                TaskCompletionSource<string> completionSource = new TaskCompletionSource<string>();
//                completionSource.SetResult(@string);
//                return completionSource.Task;
//            }
//            if (requestBody != null)
//            {
//                using (Stream requestStream = PclExport.Instance.GetRequestStream((WebRequest)httpWebRequest))
//                {
//                    using (StreamWriter streamWriter = new StreamWriter(requestStream))
//                        streamWriter.Write(requestBody);
//                }
//            }
//            Task<HttpWebResponse> responseAsync = HttpUtils.GetResponseAsync(httpWebRequest);
//            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
//            responseAsync.ContinueWith((Action<Task<HttpWebResponse>>)(task =>
//            {
//                if (task.Exception != null)
//                    tcs.SetException((Exception)task.Exception);
//                else if (task.IsCanceled)
//                {
//                    tcs.SetCanceled();
//                }
//                else
//                {
//                    HttpWebResponse result = task.Result;
//                    if (responseFilter != null)
//                        responseFilter(result);
//                    using (Stream responseStream = result.GetResponseStream())
//                    {
//                        using (StreamReader streamReader = new StreamReader(responseStream))
//                            tcs.SetResult(streamReader.ReadToEnd());
//                    }
//                }
//            }));
//            return tcs.Task;
//        }

//        public static byte[] GetBytesFromUrl(this string url, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendBytesToUrl((string)null, (byte[])null, (string)null, accept, requestFilter, responseFilter);
//        }

//        public static Task<byte[]> GetBytesFromUrlAsync(this string url, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendBytesToUrlAsync((string)null, (byte[])null, (string)null, accept, requestFilter, responseFilter);
//        }

//        public static byte[] PostBytesToUrl(this string url, byte[] requestBody = null, string contentType = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            string url1 = url;
//            string method = "POST";
//            string str = contentType;
//            byte[] requestBody1 = requestBody;
//            string contentType1 = str;
//            string accept1 = accept;
//            Action<HttpWebRequest> requestFilter1 = requestFilter;
//            Action<HttpWebResponse> responseFilter1 = responseFilter;
//            return url1.SendBytesToUrl(method, requestBody1, contentType1, accept1, requestFilter1, responseFilter1);
//        }

//        public static Task<byte[]> PostBytesToUrlAsync(this string url, byte[] requestBody = null, string contentType = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            string url1 = url;
//            string method = "POST";
//            string str = contentType;
//            byte[] requestBody1 = requestBody;
//            string contentType1 = str;
//            string accept1 = accept;
//            Action<HttpWebRequest> requestFilter1 = requestFilter;
//            Action<HttpWebResponse> responseFilter1 = responseFilter;
//            return url1.SendBytesToUrlAsync(method, requestBody1, contentType1, accept1, requestFilter1, responseFilter1);
//        }

//        public static byte[] PutBytesToUrl(this string url, byte[] requestBody = null, string contentType = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            string url1 = url;
//            string method = "PUT";
//            string str = contentType;
//            byte[] requestBody1 = requestBody;
//            string contentType1 = str;
//            string accept1 = accept;
//            Action<HttpWebRequest> requestFilter1 = requestFilter;
//            Action<HttpWebResponse> responseFilter1 = responseFilter;
//            return url1.SendBytesToUrl(method, requestBody1, contentType1, accept1, requestFilter1, responseFilter1);
//        }

//        public static Task<byte[]> PutBytesToUrlAsync(this string url, byte[] requestBody = null, string contentType = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            string url1 = url;
//            string method = "PUT";
//            string str = contentType;
//            byte[] requestBody1 = requestBody;
//            string contentType1 = str;
//            string accept1 = accept;
//            Action<HttpWebRequest> requestFilter1 = requestFilter;
//            Action<HttpWebResponse> responseFilter1 = responseFilter;
//            return url1.SendBytesToUrlAsync(method, requestBody1, contentType1, accept1, requestFilter1, responseFilter1);
//        }

//        public static byte[] SendBytesToUrl(this string url, string method = null, byte[] requestBody = null, string contentType = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
//            if (method != null)
//                webReq.Method = method;
//            if (contentType != null)
//                webReq.ContentType = contentType;
//            webReq.Accept = accept;
//            PclExport.Instance.AddCompression((WebRequest)webReq);
//            if (requestFilter != null)
//                requestFilter(webReq);
//            if (HttpUtils.ResultsFilter != null)
//                return HttpUtils.ResultsFilter.GetBytes(webReq, requestBody);
//            if (requestBody != null)
//            {
//                using (Stream requestStream = PclExport.Instance.GetRequestStream((WebRequest)webReq))
//                    requestStream.Write(requestBody, 0, requestBody.Length);
//            }
//            using (WebResponse response = PclExport.Instance.GetResponse((WebRequest)webReq))
//            {
//                if (responseFilter != null)
//                    responseFilter((HttpWebResponse)response);
//                using (Stream responseStream = response.GetResponseStream())
//                    return responseStream.ReadFully();
//            }
//        }

//        public static Task<byte[]> SendBytesToUrlAsync(this string url, string method = null, byte[] requestBody = null, string contentType = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
//            if (method != null)
//                httpWebRequest.Method = method;
//            if (contentType != null)
//                httpWebRequest.ContentType = contentType;
//            httpWebRequest.Accept = accept;
//            PclExport.Instance.AddCompression((WebRequest)httpWebRequest);
//            if (requestFilter != null)
//                requestFilter(httpWebRequest);
//            if (HttpUtils.ResultsFilter != null)
//            {
//                byte[] bytes = HttpUtils.ResultsFilter.GetBytes(httpWebRequest, requestBody);
//                TaskCompletionSource<byte[]> completionSource = new TaskCompletionSource<byte[]>();
//                completionSource.SetResult(bytes);
//                return completionSource.Task;
//            }
//            if (requestBody != null)
//            {
//                using (Stream requestStream = PclExport.Instance.GetRequestStream((WebRequest)httpWebRequest))
//                    requestStream.Write(requestBody, 0, requestBody.Length);
//            }
//            Task<HttpWebResponse> responseAsync = HttpUtils.GetResponseAsync(httpWebRequest);
//            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();
//            responseAsync.ContinueWith((Action<Task<HttpWebResponse>>)(task =>
//            {
//                if (task.Exception != null)
//                    tcs.SetException((Exception)task.Exception);
//                else if (task.IsCanceled)
//                {
//                    tcs.SetCanceled();
//                }
//                else
//                {
//                    HttpWebResponse result = task.Result;
//                    if (responseFilter != null)
//                        responseFilter(result);
//                    using (Stream responseStream = result.GetResponseStream())
//                        tcs.SetResult(responseStream.ReadFully());
//                }
//            }));
//            return tcs.Task;
//        }

//        public static bool IsAny300(this Exception ex)
//        {
//            HttpStatusCode? status = ex.GetStatus();
//            HttpStatusCode? nullable1 = status;
//            if ((nullable1.GetValueOrDefault() < HttpStatusCode.MultipleChoices ? 0 : (nullable1.HasValue ? 1 : 0)) == 0)
//                return false;
//            HttpStatusCode? nullable2 = status;
//            if (nullable2.GetValueOrDefault() < HttpStatusCode.BadRequest)
//                return nullable2.HasValue;
//            return false;
//        }

//        public static bool IsAny400(this Exception ex)
//        {
//            HttpStatusCode? status = ex.GetStatus();
//            HttpStatusCode? nullable1 = status;
//            if ((nullable1.GetValueOrDefault() < HttpStatusCode.BadRequest ? 0 : (nullable1.HasValue ? 1 : 0)) == 0)
//                return false;
//            HttpStatusCode? nullable2 = status;
//            if (nullable2.GetValueOrDefault() < HttpStatusCode.InternalServerError)
//                return nullable2.HasValue;
//            return false;
//        }

//        public static bool IsAny500(this Exception ex)
//        {
//            HttpStatusCode? status = ex.GetStatus();
//            HttpStatusCode? nullable = status;
//            if ((nullable.GetValueOrDefault() < HttpStatusCode.InternalServerError ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
//                return status.Value < (HttpStatusCode)600;
//            return false;
//        }

//        public static bool IsNotModified(this Exception ex)
//        {
//            HttpStatusCode? status = ex.GetStatus();
//            if (status.GetValueOrDefault() == HttpStatusCode.NotModified)
//                return status.HasValue;
//            return false;
//        }

//        public static bool IsBadRequest(this Exception ex)
//        {
//            HttpStatusCode? status = ex.GetStatus();
//            if (status.GetValueOrDefault() == HttpStatusCode.BadRequest)
//                return status.HasValue;
//            return false;
//        }

//        public static bool IsNotFound(this Exception ex)
//        {
//            HttpStatusCode? status = ex.GetStatus();
//            if (status.GetValueOrDefault() == HttpStatusCode.NotFound)
//                return status.HasValue;
//            return false;
//        }

//        public static bool IsUnauthorized(this Exception ex)
//        {
//            HttpStatusCode? status = ex.GetStatus();
//            if (status.GetValueOrDefault() == HttpStatusCode.Unauthorized)
//                return status.HasValue;
//            return false;
//        }

//        public static bool IsForbidden(this Exception ex)
//        {
//            HttpStatusCode? status = ex.GetStatus();
//            if (status.GetValueOrDefault() == HttpStatusCode.Forbidden)
//                return status.HasValue;
//            return false;
//        }

//        public static bool IsInternalServerError(this Exception ex)
//        {
//            HttpStatusCode? status = ex.GetStatus();
//            if (status.GetValueOrDefault() == HttpStatusCode.InternalServerError)
//                return status.HasValue;
//            return false;
//        }

//        public static HttpStatusCode? GetResponseStatus(this string url)
//        {
//            try
//            {
//                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
//                using (WebResponse response = PclExport.Instance.GetResponse((WebRequest)httpWebRequest))
//                {
//                    HttpWebResponse httpWebResponse = response as HttpWebResponse;
//                    return httpWebResponse != null ? new HttpStatusCode?(httpWebResponse.StatusCode) : new HttpStatusCode?();
//                }
//            }
//            catch (Exception ex)
//            {
//                return ex.GetStatus();
//            }
//        }

//        public static HttpStatusCode? GetStatus(this Exception ex)
//        {
//            if (ex == null)
//                return new HttpStatusCode?();
//            WebException webEx = ex as WebException;
//            if (webEx != null)
//                return HttpUtils.GetStatus(webEx);
//            IHasStatusCode hasStatusCode = ex as IHasStatusCode;
//            if (hasStatusCode != null)
//                return new HttpStatusCode?((HttpStatusCode)hasStatusCode.StatusCode);
//            return new HttpStatusCode?();
//        }

//        public static HttpStatusCode? GetStatus(this WebException webEx)
//        {
//            if (webEx == null)
//                return new HttpStatusCode?();
//            HttpWebResponse httpWebResponse = webEx.Response as HttpWebResponse;
//            if (httpWebResponse != null)
//                return new HttpStatusCode?(httpWebResponse.StatusCode);
//            return new HttpStatusCode?();
//        }

//        public static bool HasStatus(this Exception ex, HttpStatusCode statusCode)
//        {
//            HttpStatusCode? status = ex.GetStatus();
//            HttpStatusCode httpStatusCode = statusCode;
//            if (status.GetValueOrDefault() == httpStatusCode)
//                return status.HasValue;
//            return false;
//        }

//        public static string GetResponseBody(this Exception ex)
//        {
//            WebException webException = ex as WebException;
//            if (webException == null || webException.Response == null || webException.Status != WebExceptionStatus.ProtocolError)
//                return (string)null;
//            using (StreamReader streamReader = new StreamReader(webException.Response.GetResponseStream()))
//                return streamReader.ReadToEnd();
//        }

//        public static string ReadToEnd(this WebResponse webRes)
//        {
//            using (Stream responseStream = webRes.GetResponseStream())
//            {
//                using (StreamReader streamReader = new StreamReader(responseStream))
//                    return streamReader.ReadToEnd();
//            }
//        }

//        public static IEnumerable<string> ReadLines(this WebResponse webRes)
//        {
//            using (Stream responseStream = webRes.GetResponseStream())
//            {
//                using (StreamReader streamReader = new StreamReader(responseStream))
//                {
//                    string line;
//                    while ((line = streamReader.ReadLine()) != null)
//                        yield return line;
//                }
//            }
//        }

//        public static HttpWebResponse GetErrorResponse(this string url)
//        {
//            try
//            {
//                WebRequest webRequest = WebRequest.Create(url);
//                using (WebResponse response = PclExport.Instance.GetResponse(webRequest))
//                {
//                    response.ReadToEnd();
//                    return (HttpWebResponse)null;
//                }
//            }
//            catch (WebException ex)
//            {
//                return (HttpWebResponse)ex.Response;
//            }
//        }

//        public static Task<Stream> GetRequestStreamAsync(this WebRequest request)
//        {
//            return HttpUtils.GetRequestStreamAsync((HttpWebRequest)request);
//        }

//        public static Task<Stream> GetRequestStreamAsync(this HttpWebRequest request)
//        {
//            TaskCompletionSource<Stream> tcs = new TaskCompletionSource<Stream>();
//            try
//            {
//                request.BeginGetRequestStream((AsyncCallback)(iar =>
//                {
//                    try
//                    {
//                        tcs.SetResult(request.EndGetRequestStream(iar));
//                    }
//                    catch (Exception ex)
//                    {
//                        tcs.SetException(ex);
//                    }
//                }), (object)null);
//            }
//            catch (Exception ex)
//            {
//                tcs.SetException(ex);
//            }
//            return tcs.Task;
//        }

//        public static Task<TBase> ConvertTo<TDerived, TBase>(this Task<TDerived> task) where TDerived : TBase
//        {
//            TaskCompletionSource<TBase> tcs = new TaskCompletionSource<TBase>();
//            task.ContinueWith((Action<Task<TDerived>>)(t => tcs.SetResult((TBase)t.Result)), TaskContinuationOptions.OnlyOnRanToCompletion);
//            task.ContinueWith((Action<Task<TDerived>>)(t => tcs.SetException((IEnumerable<Exception>)t.Exception.InnerExceptions)), TaskContinuationOptions.OnlyOnFaulted);
//            task.ContinueWith((Action<Task<TDerived>>)(t => tcs.SetCanceled()), TaskContinuationOptions.OnlyOnCanceled);
//            return tcs.Task;
//        }

//        public static Task<WebResponse> GetResponseAsync(this WebRequest request)
//        {
//            return HttpUtils.GetResponseAsync((HttpWebRequest)request).ConvertTo<HttpWebResponse, WebResponse>();
//        }

//        public static Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest request)
//        {
//            TaskCompletionSource<HttpWebResponse> tcs = new TaskCompletionSource<HttpWebResponse>();
//            try
//            {
//                request.BeginGetResponse((AsyncCallback)(iar =>
//                {
//                    try
//                    {
//                        tcs.SetResult((HttpWebResponse)request.EndGetResponse(iar));
//                    }
//                    catch (Exception ex)
//                    {
//                        tcs.SetException(ex);
//                    }
//                }), (object)null);
//            }
//            catch (Exception ex)
//            {
//                tcs.SetException(ex);
//            }
//            return tcs.Task;
//        }

//        public static void UploadFile(this WebRequest webRequest, Stream fileStream, string fileName, string mimeType, string accept = null, Action<HttpWebRequest> requestFilter = null, string method = "POST")
//        {
//            HttpWebRequest httpWebRequest = (HttpWebRequest)webRequest;
//            httpWebRequest.Method = method;
//            if (accept != null)
//                httpWebRequest.Accept = accept;
//            if (requestFilter != null)
//                requestFilter(httpWebRequest);
//            string str = "----------------------------" + Guid.NewGuid().ToString("N");
//            httpWebRequest.ContentType = "multipart/form-data; boundary=" + str;
//            byte[] asciiBytes1 = ("\r\n--" + str + "\r\n").ToAsciiBytes();
//            byte[] asciiBytes2 = string.Format("\r\n--" + str + "\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\nContent-Type: {1}\r\n\r\n", (object)fileName, (object)mimeType).ToAsciiBytes();
//            long num = fileStream.Length + (long)asciiBytes2.Length + (long)asciiBytes1.Length;
//            PclExport.Instance.InitHttpWebRequest(httpWebRequest, new long?(num), false, false);
//            if (HttpUtils.ResultsFilter != null)
//            {
//                HttpUtils.ResultsFilter.UploadStream(httpWebRequest, fileStream, fileName);
//            }
//            else
//            {
//                using (Stream requestStream = PclExport.Instance.GetRequestStream((WebRequest)httpWebRequest))
//                {
//                    requestStream.Write(asciiBytes2, 0, asciiBytes2.Length);
//                    fileStream.CopyTo(requestStream, 4096);
//                    requestStream.Write(asciiBytes1, 0, asciiBytes1.Length);
//                    PclExport.Instance.CloseStream(requestStream);
//                }
//            }
//        }

//        public static void UploadFile(this WebRequest webRequest, Stream fileStream, string fileName)
//        {
//            if (fileName == null)
//                throw new ArgumentNullException("fileName");
//            string mimeType = MimeTypes.GetMimeType(fileName);
//            if (mimeType == null)
//                throw new ArgumentException("Mime-type not found for file: " + fileName);
//            webRequest.UploadFile(fileStream, fileName, mimeType, (string)null, (Action<HttpWebRequest>)null, "POST");
//        }

//        public static string PostXmlToUrl(this string url, object data, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("POST", data.ToXml<object>(), "application/xml", "application/xml", requestFilter, responseFilter);
//        }

//        public static string PostCsvToUrl(this string url, object data, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("POST", data.ToCsv<object>(), "text/csv", "text/csv", requestFilter, responseFilter);
//        }

//        public static string PutXmlToUrl(this string url, object data, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("PUT", data.ToXml<object>(), "application/xml", "application/xml", requestFilter, responseFilter);
//        }

//        public static string PutCsvToUrl(this string url, object data, Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
//        {
//            return url.SendStringToUrl("PUT", data.ToCsv<object>(), "text/csv", "text/csv", requestFilter, responseFilter);
//        }
//    }
//}
