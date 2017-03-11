using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using AntServiceStack.Interface;
using AntServiceStack.ServiceHost;
using AntServiceStack.Common.Types;
using AntServiceStack.Common.Utils;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.Extensions.MobileRequestFilter;
using AntServiceStack.ServiceClient;
using AntServiceStack.Extensions.MobileRequestFilter.SecondAuthServiceClient;

namespace AntServiceStack.Extensions.MobileRequestFilter
{
    public partial class MobileRequestFilterAttribute : RequestFilterAttribute
    {
        public override void Execute(IHttpRequest req, IHttpResponse res, object requestDto)
        {
            if (IsH5OnlyMode && !req.IsGatewayRequest())
                return;

            if (AuthenticationMode == AuthenticationModeEnum.BanH5Request)
            {
                if (req.IsGatewayRequest())
                {
                    ErrorUtils.LogError("H5 reqeust to internal operation " + req.OperationName + " is not allowed.", req, default(Exception), false, "FXD300017");

                    res.StatusCode = (int)HttpStatusCode.Forbidden;
                    res.AddHeader(ServiceUtils.ResponseStatusHttpHeaderKey, AckCodeType.Failure.ToString());
                    string traceIdString = req.Headers[ServiceUtils.TRACE_ID_HTTP_HEADER];
                    if (!string.IsNullOrWhiteSpace(traceIdString))
                        res.AddHeader(ServiceUtils.TRACE_ID_HTTP_HEADER, traceIdString);
                    res.LogRequest(req);
                    res.EndHttpHandlerRequest(true);
                }

                return;
            }

            if (req.OperationName.Trim().ToLower() == ServiceUtils.CheckHealthOperationName.ToLower())
                return;

            IHasMobileRequestHead mobileRequest = requestDto as IHasMobileRequestHead;
            bool hasMobileRequestHead = mobileRequest != null && mobileRequest.head != null;
            string auth = mobileRequest.GetAuth(req);
            if (hasMobileRequestHead)
                mobileRequest.head.auth = auth;
            bool hasAuthToken = !string.IsNullOrWhiteSpace(auth);
            if (!hasMobileRequestHead && !hasAuthToken)
            {
                if (AuthenticationMode == AuthenticationModeEnum.ByPass || IsOnDemandMode)
                    return;

                res.WriteErrorToResponse(
                    req,
                    req.ResponseContentType,
                    new MobileRequestFilterException("OperationName: " + req.OperationName + ". Request head is null and cookie auth is null."),
                    false,
                    "FXD300018");
                if (res.ExecutionResult != null)
                {
                    res.ExecutionResult.FrameworkExceptionThrown = false;
                    res.ExecutionResult.ValidationExceptionThrown = true;
                }

                res.AddHeader(ServiceUtils.ResponseStatusHttpHeaderKey, AckCodeType.Failure.ToString());
                res.LogRequest(req);
                res.EndHttpHandlerRequest(true);
                return;
            }

            string sauth = mobileRequest.GetSAuth();
            bool hasSAuthToken = !string.IsNullOrWhiteSpace(sauth);
            if (IsOnDemandMode && !hasAuthToken)
            {
                if (!UseSecondAuth || UseSecondAuth && !hasSAuthToken)
                    return;
            }

            if (AuthenticationMode == AuthenticationModeEnum.ByPass)
            {
                if (hasAuthToken)
                    mobileRequest.AddExtensionData(ServiceUtils.MobileAuthTokenExtensionKey, auth);

                if (hasSAuthToken)
                    mobileRequest.AddExtensionData(MobileRequestUtils.MobileSecondAuthExtensionKey, sauth);

                return;
            }

            try
            {
                if (UseSecondAuth)
                {
                    AuthenticateSecondAuth(req, mobileRequest, auth, sauth);
                    return;
                }

                AuthenticateRequest(req, mobileRequest, auth);
            }
            catch (Exception ex)
            {
                res.WriteErrorToResponse(req, req.ResponseContentType, ex, false, "FXD300016");
                if (res.ExecutionResult != null)
                {
                    res.ExecutionResult.FrameworkExceptionThrown = false;
                    res.ExecutionResult.ValidationExceptionThrown = true;
                }

                res.AddHeader(ServiceUtils.ResponseStatusHttpHeaderKey, AckCodeType.Failure.ToString());
                res.LogRequest(req);
                res.EndHttpHandlerRequest(true);
            }
        }

        protected virtual void AuthenticateRequest(IHttpRequest req, IHasMobileRequestHead mobileRequest, string auth)
        {
            if (mobileRequest.HasExtensionData(ServiceUtils.MobileAuthTokenExtensionKey))
                throw new MobileRequestFilterException(
                    "OperationName: " + req.OperationName + ". Request Head Extension fileds have had the authenticated auth. Request Head has bad data or MobileRequestFilter has been applied.");

            ValidateAndGetNewTokenResponse response = null;
            try
            {
                response = _mobileAuthServiceClient.ValidateAndGetNewToken(new ValidateAndGetNewTokenRequest() { Token = auth });
            }
            catch (CServiceException ex)
            {
                if (IsOnDemandMode)
                    return;
                throw new MobileRequestFilterException("OperationName: " + req.OperationName + ". Error happened when doing Auth.", ex);
            }
            catch (Exception ex)
            {
                if (IsOnDemandMode)
                    return;
                throw new Exception("OperationName: " + req.OperationName + ". Error happened when connecting to mobile auth service: " + ex.Message, ex);
            }

            if (IsOnDemandMode && response.ReturnCode != 0)
                return;
            switch (response.ReturnCode)
            {
                case 0:
                    break;
                case 1001:
                    throw new MobileRequestFilterException("OperationName: " + req.OperationName + ". No auth for authentication. Message: " + response.Message);
                case 2001:
                    throw new MobileRequestFilterException("OperationName: " + req.OperationName + ". Invalid token for authentication. Message: " + response.Message);
                case 9000:
                    throw new MobileRequestFilterException("OperationName: " + req.OperationName + ". Mobile Auth Service Internal Exception. Message: " + response.Message);
                default:
                    throw new MobileRequestFilterException("OperationName: " + req.OperationName + ". Unknown Auth Service Return Code: " + response.ReturnCode + ". Message: " + response.Message);
            }

            if (string.IsNullOrWhiteSpace(response.NewToken))
            {
                if (IsOnDemandMode)
                    return;
                throw new MobileRequestFilterException("OperationName: " + req.OperationName + ". Empty new auth was returned by MobileAuthService.");
            }

            AddAuthResponseData(req, mobileRequest, response);
        }

        private void AuthenticateSecondAuth(IHttpRequest req, IHasMobileRequestHead mobileRequest, string auth, string sauth)
        {
            if (mobileRequest.HasExtensionData(ServiceUtils.MobileAuthTokenExtensionKey))
                throw new MobileRequestFilterException(
                    "OperationName: " + req.OperationName + ". Request Head Extension fileds have had the authenticated auth. Request Head has bad data or MobileRequestFilter has been applied.");
            if (mobileRequest.HasExtensionData(MobileRequestUtils.MobileSecondAuthExtensionKey))
                throw new MobileRequestFilterException(
                    "OperationName: " + req.OperationName + ". Request Head Extension fileds have had the authenticated sauth. Request Head has bad data or MobileRequestFilter has been applied.");

            if (!string.IsNullOrWhiteSpace(sauth))
            {
                CheckSecondTokenResponse checkSecondTokenResponse = null;
                try
                {
                    checkSecondTokenResponse = _secondAuthServiceClient.CheckSecondToken(
                        new CheckSecondTokenRequest()
                        {
                            Token = sauth,
                            Auth = auth
                        });
                }
                catch (CServiceException ex)
                {
                    if (IsOnDemandMode)
                        return;
                    throw new MobileRequestFilterException("OperationName: " + req.OperationName + ". Error happened when doing second Authorization.", ex);
                }
                catch (Exception ex)
                {
                    if (IsOnDemandMode)
                        return;
                    throw new Exception("OperationName: " + req.OperationName + ". Error happened when connecting to Authorization service: " + ex.Message, ex);
                }

                if (IsOnDemandMode && checkSecondTokenResponse.ReturnCode != 0)
                    return;
                switch (checkSecondTokenResponse.ReturnCode)
                {
                    case 0:
                        break;
                    case 101:
                        throw new MobileRequestFilterException("OperationName: " + req.OperationName
                            + ". No second auth for authentication. Message: " + checkSecondTokenResponse.Message);
                    case 102:
                        throw new MobileRequestFilterException("OperationName: " + req.OperationName
                            + ". No first auth for authentication. Message: " + checkSecondTokenResponse.Message);
                    case 201:
                        throw new MobileRequestFilterException("OperationName: " + req.OperationName
                            + ". Invalid token for authentication. Message: " + checkSecondTokenResponse.Message);
                    case 900:
                        throw new MobileRequestFilterException("OperationName: " + req.OperationName
                            + ". Authorization Service Internal Exception. Message: " + checkSecondTokenResponse.Message);
                    default:
                        throw new MobileRequestFilterException("OperationName: " + req.OperationName + ". Unknown Authorization Service Return Code: "
                            + checkSecondTokenResponse.ReturnCode + ". Message: " + checkSecondTokenResponse.Message);
                }

                if (string.IsNullOrWhiteSpace(checkSecondTokenResponse.Uid))
                {
                    if (IsOnDemandMode)
                        return;

                    string format = "OperationName: {0}. Empty uid was returned by Authorization service. IsNew: {1}, Message: {2}";
                    string message = string.Format(format, req.OperationName, checkSecondTokenResponse.IsNew, checkSecondTokenResponse.Message);
                    throw new MobileRequestFilterException(message);
                }

                if (string.IsNullOrWhiteSpace(checkSecondTokenResponse.Token))
                {
                    if (IsOnDemandMode)
                        return;
                    string format = "OperationName: {0}. Empty token was returned by Authorization service. IsNew: {1}, Message: {2}";
                    string message = string.Format(format, req.OperationName, checkSecondTokenResponse.IsNew, checkSecondTokenResponse.Message);
                    throw new MobileRequestFilterException(message);
                }

                if (!string.IsNullOrWhiteSpace(auth))
                    mobileRequest.AddExtensionData(ServiceUtils.MobileAuthTokenExtensionKey, auth);
                mobileRequest.AddExtensionData(MobileRequestUtils.MobileSecondAuthExtensionKey, checkSecondTokenResponse.Token);
                mobileRequest.AddExtensionData(ServiceUtils.MobileUserIdExtensionKey, checkSecondTokenResponse.Uid);
                return;
            }

            GenSecondAuthorizationTokenResponse genSecondAuthorizationTokenResponse = null;
            try
            {
                genSecondAuthorizationTokenResponse = _secondAuthServiceClient.GenSecondAuthorizationToken(
                    new GenSecondAuthorizationTokenRequest() { Auth = auth });
            }
            catch (CServiceException ex)
            {
                if (IsOnDemandMode)
                    return;
                throw new MobileRequestFilterException("OperationName: " + req.OperationName + ". Error happened when doing GenSecondAuthorizationToken.", ex);
            }
            catch (Exception ex)
            {
                if (IsOnDemandMode)
                    return;
                throw new Exception("OperationName: " + req.OperationName + ". Error happened when connecting to Authorization service: " + ex.Message, ex);
            }

            if (IsOnDemandMode && genSecondAuthorizationTokenResponse.ReturnCode != 0)
                return;
            switch (genSecondAuthorizationTokenResponse.ReturnCode)
            {
                case 0:
                    break;
                case 101:
                    throw new MobileRequestFilterException("OperationName: " + req.OperationName
                        + ". No auth for authentication. Message: " + genSecondAuthorizationTokenResponse.Message);
                case 201:
                    throw new MobileRequestFilterException("OperationName: " + req.OperationName
                        + ". Invalid token for authentication. Message: " + genSecondAuthorizationTokenResponse.Message);
                case 900:
                    throw new MobileRequestFilterException("OperationName: " + req.OperationName
                        + ". Authorization Service Internal Exception. Message: " + genSecondAuthorizationTokenResponse.Message);
                default:
                    throw new MobileRequestFilterException("OperationName: " + req.OperationName + ". Unknown Authorization Service Return Code: "
                        + genSecondAuthorizationTokenResponse.ReturnCode + ". Message: " + genSecondAuthorizationTokenResponse.Message);
            }

            if (string.IsNullOrWhiteSpace(genSecondAuthorizationTokenResponse.Token))
            {
                if (IsOnDemandMode)
                    return;
                string format = "OperationName: {0}. Empty new second auth token was returned by Authorization service. ExpiredTime: {1}, Message: {2}";
                string message = string.Format(format, req.OperationName, genSecondAuthorizationTokenResponse.ExpiredTime, genSecondAuthorizationTokenResponse.Message);
                throw new MobileRequestFilterException(message);
            }

            mobileRequest.AddExtensionData(ServiceUtils.MobileAuthTokenExtensionKey, auth);
            mobileRequest.AddExtensionData(MobileRequestUtils.MobileSecondAuthExtensionKey, genSecondAuthorizationTokenResponse.Token);
            mobileRequest.AddExtensionData(ServiceUtils.MobileUserIdExtensionKey, genSecondAuthorizationTokenResponse.Uid);
        }

        private void AddAuthResponseData(IHttpRequest req, IHasMobileRequestHead mobileRequest, ValidateAndGetNewTokenResponse response)
        {
            mobileRequest.AddExtensionData(ServiceUtils.MobileAuthTokenExtensionKey, response.NewToken);
            mobileRequest.AddExtensionData(MobileRequestUtils.MobileAuthLoginTypeExtensionKey, response.LoginType);

            bool isNonMemberAuthLoginType = MobileRequestUtils.IsNonMemberAuthLoginType(response.LoginType);
            if (string.IsNullOrWhiteSpace(response.UserID))
            {
                if (IsOnDemandMode)
                    return;
                throw new MobileRequestFilterException("OperationName: " + req.OperationName + "'MobileAuthService' service returned null or white space UserID!");
            }

            if (!isNonMemberAuthLoginType)
            {
                mobileRequest.AddExtensionData(ServiceUtils.MobileUserIdExtensionKey, response.UserID);
                mobileRequest.AddExtensionData(ServiceUtils.MobileIsMemberAuthExtensionKey, bool.TrueString);
                if (mobileRequest.head != null)
                    mobileRequest.head.auth = response.NewToken;
                return;
            }

            if (!IsPayment)
            {
                if (!AllowNonMemberAuth)
                {
                    if (IsOnDemandMode)
                        return;
                    throw new MobileRequestFilterException("OperationName: " + req.OperationName + ". Non-Member auth mode is unsupported!");
                }

                if (string.IsNullOrWhiteSpace(response.LoginName))
                {
                    if (IsOnDemandMode)
                        return;
                    throw new MobileRequestFilterNonMemberAuthException("OperationName: " + req.OperationName + ". Non-Member auth returned null or white space [LoginName] by 'MobileAuthService' service!");
                }
            }

            mobileRequest.AddExtensionData(ServiceUtils.MobileUserIdExtensionKey, response.UserID);
            mobileRequest.AddExtensionData(ServiceUtils.MobileUserPhoneExtensionKey, response.LoginName);
            mobileRequest.AddExtensionData(ServiceUtils.MobileIsNonMemberAuthExtensionKey, bool.TrueString);
            if (mobileRequest.head != null)
                mobileRequest.head.auth = response.NewToken;
        }
    }
}
