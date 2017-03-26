using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Owin;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;

namespace AntServiceStack.Manager.SignalR
{
    public class HubAuthorizeAttribute : AuthorizeAttribute
    {
        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            var token = request.Headers.Get("token");
            if (!string.IsNullOrEmpty(token))
            {
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Authentication, token));
                var identity = new ClaimsIdentity(claims,"signalr");
                request.Environment["server.User"] = new ClaimsPrincipal(identity);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            var connectionId = hubIncomingInvokerContext.Hub.Context.ConnectionId;
            // check the authenticated user principal from environment
            var environment = hubIncomingInvokerContext.Hub.Context.Request.Environment;
            var principal = environment["server.User"] as ClaimsPrincipal;
            if (principal != null && principal.Identity != null && principal.Identity.IsAuthenticated)
            {
                // create a new HubCallerContext instance with the principal generated from token
                // and replace the current context so that in hubs we can retrieve current user identity
                hubIncomingInvokerContext.Hub.Context = new HubCallerContext(new ServerRequest(environment), connectionId);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}