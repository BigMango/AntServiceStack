using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AntServiceStack.Common.Types;
using AntServiceStack.ServiceHost;
using TestMegaContract;

namespace MegaApplication
{
    public class SoaController : Ihelloworld
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

        public async Task<HelloWorldResponseType> HelloWorldAsync(HelloWorldRequestType request)
        {
            return new HelloWorldResponseType
            {
                Response = Environment.MachineName
            };
        }
    }

}