using Service.Throttle.Model;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;

namespace Service.Throttle.Helper
{
    public static class ThrottlingHelper
    {
        private const string HttpContext = "MS_HttpContext";
        private const string RemoteEndpointMessage = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";
        private const string OwinContext = "MS_OwinContext";
        private const string NSClientIPkey = "NS-Client-IP";
        private const string RequestClientIPkey = "Req-Client-IP";

        /// <summary>
        /// Get IP Address
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetClientIp(this HttpRequestMessage request)
        {
            // Web-hosting
            if (request.Properties.ContainsKey(HttpContext))
            {
                HttpContextWrapper ctx =
                    (HttpContextWrapper)request.Properties[HttpContext];
                if (ctx != null)
                {
                    if (!string.IsNullOrEmpty(ctx.Request.Headers.Get(RequestClientIPkey))) //Request client ip
                        return ctx.Request.Headers.Get(RequestClientIPkey);
                    else if (!string.IsNullOrEmpty(ctx.Request.Headers.Get(NSClientIPkey)))
                        return ctx.Request.Headers.Get(NSClientIPkey);
                    else
                        return ctx.Request.UserHostAddress;
                }
            }

            // Self-hosting
            if (request.Properties.ContainsKey(RemoteEndpointMessage))
            {
                RemoteEndpointMessageProperty remoteEndpoint =
                    (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessage];
                if (remoteEndpoint != null)
                {
                    return remoteEndpoint.Address;
                }
            }

            // Self-hosting using Owin
            if (request.Properties.ContainsKey(OwinContext))
            {
                OwinContext owinContext = (OwinContext)request.Properties[OwinContext];
                if (owinContext != null)
                {
                    if (owinContext.Request.Headers.ContainsKey(RequestClientIPkey))
                        return owinContext.Request.Headers.Get(RequestClientIPkey);
                    //If forwaded request get from custom header value else default
                    if (owinContext.Request.Headers.ContainsKey(NSClientIPkey))
                        return owinContext.Request.Headers.Get(NSClientIPkey);
                    else
                        return owinContext.Request.RemoteIpAddress;
                }
            }

            return null;
        }

        internal static string ComputeCounterKey(string identityKey, Policy rule)
        {

            var key = string.Concat(rule.period, "-", identityKey);

            var idBytes = System.Text.Encoding.UTF8.GetBytes(key);

            byte[] hashBytes;

            using (var algorithm = System.Security.Cryptography.SHA1.Create())
            {
                hashBytes = algorithm.ComputeHash(idBytes);
            }

            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }

        internal static TimeSpan ConvertToTimeSpan(string timeSpan)
        {
            var l = timeSpan.Length - 1;
            var value = timeSpan.Substring(0, l);
            var type = timeSpan.Substring(l, 1);

            switch (type)
            {
                case "d": return TimeSpan.FromDays(double.Parse(value));
                case "h": return TimeSpan.FromHours(double.Parse(value));
                case "m": return TimeSpan.FromMinutes(double.Parse(value));
                case "s": return TimeSpan.FromSeconds(double.Parse(value));
                default: throw new FormatException(string.Format("{0} can't be converted to TimeSpan, unknown type {1}", timeSpan, type));
            }
        }
    }
}
