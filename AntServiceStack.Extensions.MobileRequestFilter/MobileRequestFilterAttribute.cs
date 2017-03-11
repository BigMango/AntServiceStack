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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public partial class MobileRequestFilterAttribute : RequestFilterAttribute
    {
        public const string H5GateWaySpecialHeaderName = MobileRequestUtils.H5GateWaySpecialHeaderName;

        internal static MobileAuthServiceClient _mobileAuthServiceClient;
        internal static AuthorizationClient _secondAuthServiceClient;

        static MobileRequestFilterAttribute()
        {
            _mobileAuthServiceClient = MobileAuthServiceClient.GetInstance();
            _mobileAuthServiceClient.Format = "json";
            _mobileAuthServiceClient.Timeout = TimeSpan.FromSeconds(5);
            _mobileAuthServiceClient.ReadWriteTimeout = TimeSpan.FromSeconds(15);
            _mobileAuthServiceClient.ConfigCHystrixCommandMaxConcurrentCount("ValidateAndGetNewToken", 200);

            _secondAuthServiceClient = AuthorizationClient.GetInstance();
            _secondAuthServiceClient.Format = "json";
            _secondAuthServiceClient.Timeout = TimeSpan.FromSeconds(5);
            _secondAuthServiceClient.ReadWriteTimeout = TimeSpan.FromSeconds(15);
            _secondAuthServiceClient.ConfigCHystrixCommandMaxConcurrentCount("CheckSecondToken", 200);
            _secondAuthServiceClient.ConfigCHystrixCommandMaxConcurrentCount("GenSecondAuthorizationToken", 200);

            ServiceUtils.RegisterMobileWriteBackExtensionKey(MobileRequestUtils.MobileSecondAuthExtensionKey);
        }

        public static string DataTransferFormat
        {
            get
            {
                return _mobileAuthServiceClient.Format;
            }
            set
            {
                _mobileAuthServiceClient.Format = value;
            }
        }

        public AuthenticationModeEnum AuthenticationMode { get; set; }

        public BUEnum BU { get; set; }

        private bool? _isOnDemandMode;
        private bool IsOnDemandMode
        {
            get
            {
                if (_isOnDemandMode.HasValue)
                    return _isOnDemandMode.Value;

                _isOnDemandMode = AuthenticationMode == AuthenticationModeEnum.OnDemand ||
                                 AuthenticationMode == AuthenticationModeEnum.OnDemand_AllowNonMemberAuth ||
                                 AuthenticationMode == AuthenticationModeEnum.OnDemand_UseSecondAuth;
                return _isOnDemandMode.Value;
            }
        }

        private bool? _isH5OnlyMode;
        private bool IsH5OnlyMode
        {
            get
            {
                if (_isH5OnlyMode.HasValue)
                    return _isH5OnlyMode.Value;

                _isH5OnlyMode = AuthenticationMode == AuthenticationModeEnum.H5Only ||
                                 AuthenticationMode == AuthenticationModeEnum.H5Only_AllowNonMemberAuth ||
                                 AuthenticationMode == AuthenticationModeEnum.H5Only_UseSecondAuth;
                return _isH5OnlyMode.Value;
            }
        }

        private bool? _isAlwaysMode;
        private bool IsAlwaysMode
        {
            get
            {
                if (_isAlwaysMode.HasValue)
                    return _isAlwaysMode.Value;

                _isAlwaysMode = AuthenticationMode == AuthenticationModeEnum.Always ||
                                 AuthenticationMode == AuthenticationModeEnum.Always_AllowNonMemberAuth ||
                                 AuthenticationMode == AuthenticationModeEnum.Always_UseSecondAuth;
                return _isAlwaysMode.Value;
            }
        }

        private bool? _allowNonMemberAuth;
        private bool AllowNonMemberAuth
        {
            get
            {
                if (_allowNonMemberAuth.HasValue)
                    return _allowNonMemberAuth.Value;

                _allowNonMemberAuth = AuthenticationMode == AuthenticationModeEnum.OnDemand_AllowNonMemberAuth ||
                                 AuthenticationMode == AuthenticationModeEnum.H5Only_AllowNonMemberAuth ||
                                 AuthenticationMode == AuthenticationModeEnum.Always_AllowNonMemberAuth;
                return _allowNonMemberAuth.Value;
            }
        }

        private bool? _useSecondAuth;
        private bool UseSecondAuth
        {
            get
            {
                if (_useSecondAuth.HasValue)
                    return _useSecondAuth.Value;

                _useSecondAuth = AuthenticationMode == AuthenticationModeEnum.OnDemand_UseSecondAuth ||
                                 AuthenticationMode == AuthenticationModeEnum.H5Only_UseSecondAuth ||
                                 AuthenticationMode == AuthenticationModeEnum.Always_UseSecondAuth;
                return _useSecondAuth.Value;
            }
        }

        private bool IsPayment
        {
            get
            {
                return BU == BUEnum.Payment;
            }
        }
    }
}
