//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Net;


//using AntServiceStack.Common.Utils;
//using AntServiceStack.ServiceClient;
//using AntServiceStack.Common.Execution;
//using AntServiceStack.Common.CAT;
//using AntServiceStack.Client.ServiceClient;
//using AntServiceStack.Common;
//using Freeway.Logging;

//namespace AntServiceStack.Client.CAT
//{
//    internal class ClientCatTransaction : CatTransaction
//    {
//        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClientCatTransaction));

//        private ClientExecutionContext _context;
//        private bool _isIpDirectInvocation;

//        private string _rootMessageId;
//        private string _clientMessageId;
//        private string _serviceMessageId;

//        public ClientCatTransaction(ClientExecutionContext context)
//            : base(CatConstants.TYPE_SOA_CALL, context.OperationKey)
//        {
//            _context = context;
//            _isIpDirectInvocation = context.Host != null && context.Host.All(ch => char.IsDigit(ch) || ch == '.');
//        }

//        public override void Start()
//        {
//            try
//            {
//                base.Start();
                
//                if (Transaction == null)
//                    return;

//                IMessageTree tree = Cat.GetThreadLocalMessageTree();
//                _rootMessageId = tree.RootMessageId ?? tree.MessageId;
//                _clientMessageId = tree.MessageId;
//                _serviceMessageId = Cat.CreateMessageId();

//                ClientCatUtils.LogCallerEvent();
//                Cat.LogEvent(ClientCatConstants.SOA2ClientCallFormatCatKey, _context.Format);
//            }
//            catch (Exception ex)
//            {
//                _logger.Warn(ex);
//            }
//        }

//        public void PrepareWebRequest(HttpWebRequest request)
//        {
//            try
//            {
//                if (Transaction == null)
//                    return;

//                request.Headers[CatConstants.ROOT_MESSAGE_ID] = _rootMessageId;
//                request.Headers[CatConstants.CURRENT_MESSAGE_ID] = _clientMessageId;
//                request.Headers[CatConstants.SERVER_MESSAGE_ID] = _serviceMessageId;
//                request.Headers[CatConstants.CALL_APP] = Cat.Domain;

//                Cat.LogEvent(ClientCatConstants.SOA2ClientVersionCatKey, ServiceUtils.SOA2VersionCatName);
//                Cat.LogSizeEvent(ClientCatConstants.RequestSizeCatKey, request.ContentLength);
//                if (_isIpDirectInvocation)
//                    Cat.LogEvent(CatConstants.TYPE_SOA_CALL_SERVER, _context.Host);

//                Cat.LogEvent(CatConstants.TYPE_REMOTE_CALL, CatConstants.NAME_REQUEST, CatConstants.SUCCESS, _serviceMessageId);
//            }
//            catch (Exception ex)
//            {
//                _logger.Warn(ex);
//            }
//        }

//        public void HandleWebResponse(HttpWebResponse response)
//        {
//            try
//            {
//                if (Transaction == null)
//                    return;

//                string serviceAppId = response.Headers[ServiceUtils.ServiceAppIdHttpHeaderKey];
//                if (!string.IsNullOrWhiteSpace(serviceAppId))
//                    Cat.LogEvent(CatConstants.TYPE_SOA_CALL_APP, serviceAppId);

//                string serviceIP = response.Headers[ServiceUtils.ServiceHostIPHttpHeaderKey];
//                if (!_isIpDirectInvocation && !string.IsNullOrWhiteSpace(serviceIP))
//                    Cat.LogEvent(CatConstants.TYPE_SOA_CALL_SERVER, serviceIP);
//            }
//            catch { }
//        }

//        public override void MarkSuccess()
//        {
//            try
//            {
//                if (Transaction == null)
//                    return;

//                Cat.LogEvent(ClientCatConstants.SOA2ClientResponseCodeCatKey, "Success");

//                base.MarkSuccess();
//            }
//            catch (Exception ex)
//            {
//                _logger.Warn(ex);
//            }
//        }

//        public override void MarkFailure(Exception ex, bool logError)
//        {
//            try
//            {
//                if (Transaction == null)
//                    return;

//                base.MarkFailure(ex, logError);

//                if (ex is WebException)
//                {
//                    WebException webEx = ex as WebException;
//                    if (webEx is WebProtocolException)
//                    {
//                        var protocolException = webEx as WebProtocolException;
//                        HttpWebResponse response = webEx.Response as HttpWebResponse;
//                        if (response != null)
//                            Cat.LogEvent(ClientCatConstants.SOA2ClientResponseCodeCatKey, protocolException.StatusCode.ToString());
//                    }
//                    else
//                    {
//                        Cat.LogEvent(ClientCatConstants.SOA2ClientResponseCodeCatKey, webEx.Status.ToString());
//                    }
//                }
//                else if (ex is CServiceException)
//                {
//                    CServiceException serviceException = ex as CServiceException;
//                    if (serviceException.ResponseErrors != null && serviceException.ResponseErrors.Count > 0 && serviceException.ResponseErrors[0] != null)
//                    {
//                        string errorType = serviceException.ResponseErrors[0].ErrorClassification.ToString();
//                        Cat.LogEvent(ClientCatConstants.SOA2ClientResponseCodeCatKey, errorType);
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                _logger.Warn(e);
//            }
//        }
//    }
//}
