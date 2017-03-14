using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AntServiceStack.Common.Types;
using AntServiceStack.ServiceHost;
using TestContract;

namespace WebApplication
{
    public class SoaController : ICloudBagRestFulApi
    {


     

        [Route("/HelloWorld", Summary = "HelloWorld")]
        public HelloWorldResponseType HelloWorld(HelloWorldRequestType request)
        {
            //if (!File.Exists(@"H:\1.txt"))
            //{
            //    throw new Exception("no file");
            //}
            return new HelloWorldResponseType
            {
                Response = Environment.MachineName  
            };

        }

        public Task<HelloWorldResponseType> HelloWorldAsync(HelloWorldRequestType request)
        {
            throw new NotImplementedException();
        }
    }

}