using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Extensions.MobileRequestFilter
{
    public enum AuthenticationModeEnum
    {
        H5Only,
        OnDemand,
        Always,
        ByPass,
        BanH5Request,
        H5Only_AllowNonMemberAuth,
        Always_AllowNonMemberAuth,
        OnDemand_AllowNonMemberAuth,
        H5Only_UseSecondAuth,
        Always_UseSecondAuth,
        OnDemand_UseSecondAuth
    }
}
