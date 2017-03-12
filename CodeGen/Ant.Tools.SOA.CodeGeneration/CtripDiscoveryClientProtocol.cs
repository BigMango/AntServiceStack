using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web.Services;
using System.Web.Services.Discovery;
using System.Net;
using System.Runtime.Serialization;

namespace Ant.Tools.SOA.CodeGeneration
{
    public class CtripDiscoveryClientProtocol : DiscoveryClientProtocol
    {
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            FileWebResponse webResponse = response as FileWebResponse;
            if (webResponse == null)
                return response;

            return new CtripFileWebResponse(webResponse);
        }
    }

    public class CtripFileWebResponse : WebResponse
    {
        FileWebResponse _webResponse;

        public CtripFileWebResponse()
        {
        }

        public CtripFileWebResponse(FileWebResponse webResponse) : this()
        {
            _webResponse = webResponse;
        }

        public override Stream GetResponseStream()
        {
            Stream stream = _webResponse.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            if(reader.CurrentEncoding != Encoding.UTF8)
                return stream;
            using (reader)
            {
                byte[] temp = new byte[3];
                stream.Read(temp, 0, 3);
                stream.Seek(0, SeekOrigin.Begin);
                MemoryStream memoryStream = new MemoryStream(1024);
                if (!(temp[0] == 239 && temp[1] == 187 && temp[2] == 191))
                {
                    memoryStream.Write(new byte[] { 239, 187, 191 }, 0, 3);
                }
                StreamToMemoryStream(stream, memoryStream);
                return memoryStream;
            }
        }

        public override void Close()
        {
            _webResponse.Close();
        }

        public override long ContentLength
        {
            get
            {
                return _webResponse.ContentLength;
            }
        }

        public override Uri ResponseUri
        {
            get
            {
                return _webResponse.ResponseUri;
            }
        }

        public override string ContentType
        {
            get
            {
                return _webResponse.ContentType;
            }
        }

        public override WebHeaderCollection Headers
        {
            get
            {
                return _webResponse.Headers;
            }
        }

        public override System.Runtime.Remoting.ObjRef CreateObjRef(Type requestedType)
        {
            return _webResponse.CreateObjRef(requestedType);
        }

        public override int GetHashCode()
        {
            return _webResponse.GetHashCode();
        }

        public override bool IsFromCache
        {
            get
            {
                return _webResponse.IsFromCache;
            }
        }

        public override bool IsMutuallyAuthenticated
        {
            get
            {
                return _webResponse.IsMutuallyAuthenticated;
            }
        }

        public override object InitializeLifetimeService()
        {
            return _webResponse.InitializeLifetimeService();
        }

        public override bool Equals(object obj)
        {
            return _webResponse.Equals(obj);
        }

        public override string ToString()
        {
            return _webResponse.ToString();
        }

        public static void StreamToMemoryStream(Stream stream, MemoryStream memoryStream)
        {
            byte[] numArray = new byte[1024];
            while (true)
            {
                int num = stream.Read(numArray, 0, (int)numArray.Length);
                int num1 = num;
                if (num == 0)
                {
                    break;
                }
                memoryStream.Write(numArray, 0, num1);
            }
            memoryStream.Position = (long)0;
        }
    }

}
