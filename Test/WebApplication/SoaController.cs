using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AntServiceStack.Common.Types;
using AntServiceStack.ServiceHost;
using TestContract;
using TestContract.Yuzd.Client;

namespace WebApplication
{
    public class SoaController : Ihelloworld
    {


     

        [Route("/HelloWorld", Summary = "HelloWorld")]
        public TestContract.HelloWorldResponseType HelloWorld(TestContract.HelloWorldRequestType request)
        {

            try
            {
                var client = helloyuzdClient.GetInstance();
                var re = new TestContract.Yuzd.Client.HelloWorldRequestType();
                var rep = client.HelloWorld(re);
                return new TestContract.HelloWorldResponseType
                {
                    Response = rep.Response
                };
            }
            catch (Exception ex)
            {

                return new TestContract.HelloWorldResponseType
                {
                    Response = ex.Message
                };
            }

        }

        public async Task<TestContract.HelloWorldResponseType> HelloWorldAsync(TestContract.HelloWorldRequestType request)
        {
            return new TestContract.HelloWorldResponseType
            {
                Response = Environment.MachineName
            };
        }
    }

}