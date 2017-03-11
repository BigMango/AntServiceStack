using System;
using System.IO;
using System.Net;
using Freeway.Logging;
using AntServiceStack.ServiceHost;
using System.Collections.Generic;

namespace AntServiceStack.WebHost.Endpoints.Extensions
{
    public class HttpListenerResponseWrapper
        : IHttpResponse
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpListenerResponseWrapper));

        private readonly HttpListenerResponse response;
        private readonly Stream _lengthObservableStream;

        public HttpListenerResponseWrapper(HttpListenerResponse response)
        {
            this.response = response;
            this._lengthObservableStream = new LengthObservableResponseStream(response.OutputStream);
            this.Cookies = new Cookies(this);
            this.ExecutionResult = new ExecutionResult();
        }

        public IExecutionResult ExecutionResult { get; set; }

        public object ResponseObject { get; set; }

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
            try
            {
                var bOutput = System.Text.Encoding.UTF8.GetBytes(text);
                response.ContentLength64 = bOutput.Length;

                var outputStream = response.OutputStream;
                outputStream.Write(bOutput, 0, bOutput.Length);
                Close();
            }
            catch (Exception ex)
            {
                Log.Error("Could not WriteTextToResponse: " + ex.Message, ex, 
                    new Dictionary<string, string>() 
                    {
                        {"ErrorCode", "FXD300057"}
                    });
                throw;
            }
        }

        public void Close()
        {
            if (!this.IsClosed)
            {
                this.IsClosed = true;

                try
                {
                    _lengthObservableStream.Close();
                }
                catch { }

                try
                {
                    this.response.CloseOutputStream();
                }
                catch (Exception ex)
                {
                    Log.Error("Error closing HttpListener output stream", ex,
                        new Dictionary<string, string>() 
                        {
                            {"ErrorCode", "FXD300058"}
                        });
                }
            }
        }

        public void End()
        {
            Close();
        }

        public void Flush()
        {
            response.OutputStream.Flush();
        }

        public bool IsClosed
        {
            get;
            private set;
        }

        public void SetContentLength(long contentLength)
        {
            //you can happily set the Content-Length header in Asp.Net
            //but HttpListener will complain if you do - you have to set ContentLength64 on the response.
            //workaround: HttpListener throws "The parameter is incorrect" exceptions when we try to set the Content-Length header
            response.ContentLength64 = contentLength;
        }
    }

}