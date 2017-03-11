using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using AntServiceStack.Common.Utils;
using AntServiceStack.ServiceClient;
using AntServiceStack.Common.CAT;
using AntServiceStack.Common.Execution;
using Freeway.Logging;

namespace AntServiceStack.Client.CAT
{
    internal class ClientIOCPResponseCatTransaction : CatFabricTransaction
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClientIOCPResponseCatTransaction));

        private ExecutionContext _context;

        public ClientIOCPResponseCatTransaction(ExecutionContext context)
            : base(ClientCatConstants.SOA2ClientIOCPResponseTransactionName, context.OperationKey)
        {
            _context = context;
        }

        public void HandleWebResponse(HttpWebResponse response)
        {
            try
            {

                string serviceAppId = response.Headers[ServiceUtils.ServiceAppIdHttpHeaderKey];

                string serviceIP = response.Headers[ServiceUtils.ServiceHostIPHttpHeaderKey];
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
            }
        }

        public override void MarkSuccess()
        {
            try
            {

                base.MarkSuccess();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
            }
        }

        public override void MarkFailure(Exception ex)
        {
            try
            {
             

                base.MarkFailure(ex);

                if (ex is WebException)
                {
                    WebException webEx = ex as WebException;
                    if (webEx is WebProtocolException)
                    {
                        var protocolException = webEx as WebProtocolException;
                        HttpWebResponse response = webEx.Response as HttpWebResponse;
                        
                    }
                    else
                    {
                    }
                }
                else if (ex is CServiceException)
                {
                    CServiceException serviceException = ex as CServiceException;
                    if (serviceException.ResponseErrors != null && serviceException.ResponseErrors.Count > 0 && serviceException.ResponseErrors[0] != null)
                    {
                        string errorType = serviceException.ResponseErrors[0].ErrorClassification.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Warn(e);
            }
        }

        public override void End()
        {

            base.End();
        }
    }
}
