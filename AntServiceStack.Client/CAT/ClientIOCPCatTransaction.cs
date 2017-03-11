using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Execution;
using AntServiceStack.Common.CAT;
using AntServiceStack.Common;
using AntServiceStack.Common.Utils;
using Freeway.Logging;

namespace AntServiceStack.Client.CAT
{
    internal class ClientIOCPCatTransaction : CatTransaction
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClientIOCPCatTransaction));

        private ExecutionContext _context;

        public ClientIOCPCatTransaction(ExecutionContext context)
            : base(ClientCatConstants.SOA2ClientIOCPCallTransactionName, context.OperationKey)
        {
            _context = context;
        }


        public override void Start()
        {
            try
            {
                base.Start();


                ClientCatUtils.LogCallerEvent();
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
