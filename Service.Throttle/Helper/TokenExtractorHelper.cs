using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http.Controllers;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using Microsoft.Owin;

namespace Service.Throttle.Helper
{
    public class RequestExtractorHelper
    {
        private const string HttpContext = "MS_HttpContext";
        private const string RemoteEndpointMessage = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";
        private const string OwinContext = "MS_OwinContext";
        private const string NSClientIPkey = "NS-Client-IP";
        private const string RequestClientIPkey = "Req-Client-IP";
        private HttpRequestMessage _request = null;

        public RequestExtractorHelper(HttpRequestMessage request)
        {
            _request = request;
        }

        public string GetClientIp()
        {
            // Web-hosting
            if (_request.Properties.ContainsKey(HttpContext))
            {
                HttpContextWrapper ctx =
                    (HttpContextWrapper)_request.Properties[HttpContext];
                if (ctx != null)
                {
                    if (!string.IsNullOrEmpty(ctx.Request.Headers.Get(RequestClientIPkey))) //Request client ip
                        return ctx.Request.Headers.Get(RequestClientIPkey);
                    if (!string.IsNullOrEmpty(ctx.Request.Headers.Get(NSClientIPkey)))
                        return ctx.Request.Headers.Get(NSClientIPkey);
                    else
                        return ctx.Request.UserHostAddress;
                }
            }

            // Self-hosting
            if (_request.Properties.ContainsKey(RemoteEndpointMessage))
            {
                RemoteEndpointMessageProperty remoteEndpoint =
                    (RemoteEndpointMessageProperty)_request.Properties[RemoteEndpointMessage];
                if (remoteEndpoint != null)
                {
                    return remoteEndpoint.Address;
                }
            }

            // Self-hosting using Owin
            if (_request.Properties.ContainsKey(OwinContext))
            {
                OwinContext owinContext = (OwinContext)_request.Properties[OwinContext];
                if (owinContext != null)
                {
                    //If forwaded request get from custom header value else default
                    if (owinContext.Request.Headers.ContainsKey(RequestClientIPkey))
                        return owinContext.Request.Headers.Get(RequestClientIPkey);
                    if (owinContext.Request.Headers.ContainsKey(NSClientIPkey))
                        return owinContext.Request.Headers.Get(NSClientIPkey);
                    else
                        return owinContext.Request.RemoteIpAddress;
                }
            }

            return null;
        }

        public string GetClientKey()
        {
            if (_request.Headers.Contains("x"))
                return _request.Headers.GetValues("x").FirstOrDefault();
            else
                return string.Empty;
        }

        public string GetUserSessionId()
        {
            if (_request.Headers.Contains("ss"))
                return _request.Headers.GetValues("ss").FirstOrDefault();
            else
                return string.Empty;
        }
        public string GetClientLanguages()
        {
            try
            {
                return ((HttpContextWrapper)_request.Properties["MS_HttpContext"]).Request.UserLanguages[0];
            }
            catch
            {
                return "de-CH";
            }
        }
    }
}
