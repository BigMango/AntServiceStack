using System;
using System.Globalization;
using System.IO;
using System.Web;
using AntServiceStack.ServiceHost;

namespace AntServiceStack.WebHost.Endpoints.Extensions
{
    public class HttpResponseWrapper
        : IHttpResponse
    {
        //private static readonly ILog Log = LogManager.GetLogger(typeof(HttpResponseWrapper));

        private readonly HttpResponse response;
        private readonly Stream _lengthObservableStream;

        public HttpResponseWrapper(HttpResponse response)
        {
            this.response = response;
            this.response.TrySkipIisCustomErrors = true;
            this._lengthObservableStream = new LengthObservableResponseStream(response.OutputStream);
            this.Cookies = new Cookies(this);
            this.ExecutionResult = new ExecutionResult();
        }

        public IExecutionResult ExecutionResult { get; set; }

        public object ResponseObject { get; set; }

        public HttpResponse Response
        {
            get { return response; }
        }

        public object OriginalResponse
        {
            get { return response; }
        }

        public int StatusCode
        {
            get { return this.response.StatusCode; }
            set { this.response.StatusCode = value; }
        }

        public string StatusDescription
        {
            get { return this.response.StatusDescription; }
            set { this.response.StatusDescription = value; }
        }

        public string ContentType
        {
            get { return response.ContentType; }
            set { response.ContentType = value; }
        }

        public ICookies Cookies { get; set; }

        public void AddHeader(string name, string value)
        {
            response.AddHeader(name, value);
        }

        public void Redirect(string url)
        {
            response.Redirect(url);
        }

        public Stream OutputStream
        {
            get { return _lengthObservableStream; }
        }

        public long SerializationTimeInMillis { get; set; }

        public void Write(string text)
        {
            response.Write(text);
        }

        public void Close()
        {
            if (this.IsClosed) return;
            this.IsClosed = true;
            try
            {
                _lengthObservableStream.Close();
            }
            catch { }
            response.CloseOutputStream();
        }

        public void End()
        {
            if (this.IsClosed) return;
            this.IsClosed = true;
            try
            {
                response.ClearContent();
                response.End();
            }
            catch { }
        }

        public void Flush()
        {
            response.Flush();
        }

        public bool IsClosed
        {
            get;
            private set;
        }

        public void SetContentLength(long contentLength)
        {
            try
            {
                response.Headers.Add("Content-Length", contentLength.ToString(CultureInfo.InvariantCulture));
            }
            catch (PlatformNotSupportedException /*ignore*/) { } //This operation requires IIS integrated pipeline mode.
        }
    }
}