using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Client.CAT
{
    internal class ClientCatConstants
    {
        public const string RequestSizeCatKey = "SOA2Client.reqSize";
        public const string SOA2ClientVersionCatKey = "SOA2Client.version";
        public const string SOA2ClientResponseCodeCatKey = "SOA2Client.resCode";
        public const string SOA2ClientCallFormatCatKey = "SOA2Client.callFormat";

        public const string SOA2ClientIOCPRequestTransactionName = "SOA2AsyncClient.request";
        public const string SOA2ClientIOCPResponseTransactionName = "SOA2AsyncClient.response";

        public const string SOA2ClientIOCPCallTransactionName = "SOA2AsyncClient";

        public const string SOA2ClientCallerCatKey = "SOA2Client.caller";
    }
}
