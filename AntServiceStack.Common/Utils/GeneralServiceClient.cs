using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.Reflection;

namespace AntServiceStack.Common.Utils
{
    public class GeneralServiceClient
    {
        public HttpMethodEnum Method { get; set; }
        public TimeSpan Timeout { get; set; }
        public TimeSpan ReadWriteTimeout { get; set; }
        public bool AllowAutoDirect { get; set; }
        public DataFormat Format { get; set; }
        public NameValueCollection Headers { get; private set; }

        public GeneralServiceClient()
        {
            Method = HttpMethodEnum.POST;
            Timeout = TimeSpan.FromSeconds(100);
            ReadWriteTimeout = TimeSpan.FromSeconds(300);
            AllowAutoDirect = true;
            Format = DataFormat.XML;
            Headers = new NameValueCollection();
        }

        public virtual T Invoke<T>(string url, object dto = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("The target url is null or whitespace.");

            if (Method == HttpMethodEnum.NotSupported)
                throw new NotSupportedException("HTTP Method is not supported.");

            if (Format == DataFormat.NotSupported)
                throw new NotSupportedException("Data Transfer Format is not supported.");

            if (dto != null && (Method == HttpMethodEnum.GET || Method == HttpMethodEnum.DELETE))
                url += GeneralSerializer.SerializeToQueryString(dto);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = Method.ToString();
            request.Timeout = (int)Timeout.TotalMilliseconds;
            request.ReadWriteTimeout = (int)ReadWriteTimeout.TotalMilliseconds;
            request.AllowAutoRedirect = AllowAutoDirect;
            request.ContentType = Format.ToContentType();
            request.Accept = request.ContentType;
            request.Headers.Add(Headers);

            try
            {
                if (dto != null && (Method == HttpMethodEnum.POST || Method == HttpMethodEnum.PUT))
                {
                    using (var requestStream = request.GetRequestStream())
                    {
                        GeneralSerializer.Serialize(dto, requestStream, Format);
                    }
                }

                using (WebResponse response = request.GetResponse())
                {
                    if (response.ContentLength == 0)
                        return default(T);
                    using (var responseStream = response.GetResponseStream())
                    {
                        return GeneralSerializer.Deserialize<T>(responseStream, Format);
                    }
                }
            }
            catch (WebException ex)
            {
                using (ex.Response)
                {
                    if (ex.Response != null && ex.Response is HttpWebResponse)
                    {
                        HttpWebResponse response = (HttpWebResponse)ex.Response;
                        if (response.StatusCode == HttpStatusCode.Forbidden)
                            throw new UnauthorizedAccessException(ex.Message, ex);
                        if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
                            throw new NotSupportedException(ex.Message, ex);
                        if (response.StatusCode == HttpStatusCode.BadRequest)
                            throw new BadRequestException(ex.Message, ex);
                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                            throw new ServiceInternalException(ex.Message, ex);
                        if ((int)response.StatusCode == 429)
                            throw new RateLimitingException(ex.Message, ex);
                    }

                    throw ex;
                }
            }
        }
    }
}
