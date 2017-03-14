using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.CAT;
using AntServiceStack.Common.Execution;
using Freeway.Logging;

namespace AntServiceStack.Client.CAT
{
    internal class ClientIOCPRequestCatTransaction : CatFabricTransaction
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClientIOCPRequestCatTransaction));

        private ExecutionContext _context;
        //private string _rootMessageId;
        //private string _clientMessageId;
        //private string _serviceMessageId;

        public ClientIOCPRequestCatTransaction(ExecutionContext context)
            : base(ClientCatConstants.SOA2ClientIOCPRequestTransactionName, context.OperationKey)
        {
            _context = context;
        }

        public override void Start()
        {
            try
            {
                base.Start();
                this.InitializeMessage();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
            }
        }

        public override void Fork()
        {
            try
            {
                base.Fork();
                this.InitializeMessage();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
            }
        }

        private void InitializeMessage()
        {

        }

        public void PrepareWebRequest(HttpWebRequest request)
        {
            try
            {

                //request.Headers[CatConstants.ROOT_MESSAGE_ID] = _rootMessageId;
                //request.Headers[CatConstants.CURRENT_MESSAGE_ID] = _clientMessageId;
                //request.Headers[CatConstants.SERVER_MESSAGE_ID] = _serviceMessageId;
                //request.Headers[CatConstants.CALL_APP] = Cat.Domain;

            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
            }
        }

        public override void End()
        {
            base.End();
        }
    }
}
