using Service.Throttle.Helper;
using Service.Throttle.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Service.Throttle
{
    public class EnableThrottlingAttribute : ActionFilterAttribute
    {
        #region variables
        private static readonly object ProcessLocker = new object();
        private const string allClientThrollingPolicyCacheData = "ClientThrollingPolicy";
        private string _ThrottlePolicyKey { get; set; }
        RequestIdentity requestIdentity;
        #endregion
        public EnableThrottlingAttribute()
        { }
        public EnableThrottlingAttribute(string ThrottlePolicyKey)
        {
            _ThrottlePolicyKey = ThrottlePolicyKey;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            string endPointName = actionContext.Request.RequestUri.AbsolutePath;

            #region STEP 1: Get user details like email, IP address, customerid, etc. & fill dictionary
            requestIdentity = GetIdentity(actionContext);

            Dictionary<string, string> dictIdentity = new Dictionary<string, string>();
            dictIdentity.Add(KeyType.clientKey.ToString(), requestIdentity.clientKey);
            dictIdentity.Add(KeyType.ip.ToString(), requestIdentity.ipAddress);
            #endregion

            #region STEP 2: Get ThrottleData.json file data which is saved in http memory cache.
            List<Client> clients = (List<Client>)MemoryCacherHelper.GetValue(allClientThrollingPolicyCacheData);
            Client client = clients.Where(x => x.clientKey == requestIdentity.clientKey).FirstOrDefault();
            ThrottlePolicy throttlePolicy = client != null ? client.throttlePolicy : clients[0].throttlePolicy;
            #endregion

            #region STEP 3: Filtered whiteList users, throttling not applied to these users.
            if (throttlePolicy.whiteListClients.Contains(requestIdentity.clientKey)
                || throttlePolicy.whiteListClients.Contains(requestIdentity.ipAddress))
                return;
            #endregion

            #region Step 4: Apply policy -> unique -> method name + identity key like emil, ip etc.
            ClientRateLimiting clientRateLimiting = throttlePolicy.clientRateLimiting.Where(p => p.key == _ThrottlePolicyKey).FirstOrDefault();
            if (clientRateLimiting != null && !string.IsNullOrEmpty(dictIdentity[clientRateLimiting.keyType.ToString()]))
            {
                ApplyAndCheckPolicy(actionContext, clientRateLimiting.policy, dictIdentity[clientRateLimiting.keyType.ToString()] + endPointName);

                Trace.TraceInformation("******************Identity=>" + clientRateLimiting.keyType.ToString() + " => " + dictIdentity[clientRateLimiting.keyType.ToString()] + "/Method Call=>" + actionContext.Request.RequestUri.AbsoluteUri);
            }
            #endregion
        }

        #region private methods

        /// <summary>
        /// To apply policy, check limit & restrict user.
        /// </summary>
        /// <param name="actionContext">Request context</param>
        /// <param name="policyList">List of throttle policies</param>
        /// <param name="IdentityKey">Key like emailId, IP address etc.</param>
        private void ApplyAndCheckPolicy(HttpActionContext actionContext, List<Policy> policyList, string IdentityKey)
        {
            foreach (Policy item in policyList)
            {
                var key = ThrottlingHelper.ComputeCounterKey(IdentityKey, item);
                var allowExecute = false;
                item.PeriodTimespan = ThrottlingHelper.ConvertToTimeSpan(item.period);

                var throttleCounter = new ThrottleCounter()
                {
                    Timestamp = DateTime.UtcNow,
                    TotalRequests = 1
                };
                lock (ProcessLocker)
                {
                    var entry = (ThrottleCounter?)HttpRuntime.Cache[key];
                    if (entry.HasValue)
                    {
                        // entry has not expired
                        if (entry.Value.Timestamp + item.PeriodTimespan >= DateTime.UtcNow)
                        {
                            // increment request count
                            var totalRequests = entry.Value.TotalRequests + 1;

                            // deep copy
                            throttleCounter = new ThrottleCounter
                            {
                                Timestamp = entry.Value.Timestamp,
                                TotalRequests = totalRequests
                            };
                        }
                    }

                    if (HttpRuntime.Cache[key] != null)
                    {
                        HttpRuntime.Cache[key] = throttleCounter;
                    }
                    else
                    {
                        HttpRuntime.Cache.Add(
                            key,
                            throttleCounter,
                            null,
                            Cache.NoAbsoluteExpiration,
                            item.PeriodTimespan,
                            CacheItemPriority.Low,
                            null);
                        allowExecute = true;
                    }
                    if (throttleCounter.TotalRequests > item.limit)
                        allowExecute = false;
                    else
                        allowExecute = true;

                    if (!allowExecute)
                    {
                        actionContext.Response = actionContext.Request.CreateResponse(
                            (HttpStatusCode)429,
                            string.Format("API calls quota exceeded!")
                        );
                        actionContext.Response.Headers.Add("Retry-After", RetryAfterFrom(throttleCounter.Timestamp, item));
                        Trace.TraceError(string.Format(Environment.NewLine + "Request {0} from IpAddress:{1} clientKey:{2} has been throttled (blocked), quota {3}/{4} exceeded by {5}"
                                                , actionContext.Request.RequestUri.AbsoluteUri
                                                , requestIdentity.ipAddress
                                                , requestIdentity.clientKey
                                                , item.limit
                                                , item.period
                                                , throttleCounter.TotalRequests));
                    }
                }
            }

        }

        /// <summary>
        /// Get requested user details
        /// </summary>
        /// <param name="actionContext">Request context</param>
        /// <returns></returns>
        private RequestIdentity GetIdentity(HttpActionContext actionContext)
        {
            return new RequestIdentity()
            {
                ipAddress = actionContext.Request.GetClientIp(),
                clientKey = (new RequestExtractorHelper(actionContext.Request)).GetClientKey()
            };
        }

        /// <summary>
        /// To get next request retry time(in seconds)
        /// </summary>
        /// <param name="timestamp">Last request timestamp.</param>
        /// <param name="rule">ThrottlePolicy</param>
        /// <returns></returns>
        private string RetryAfterFrom(DateTime timestamp, Policy rule)
        {
            var secondsPast = Convert.ToInt32((DateTime.UtcNow - timestamp).TotalSeconds);
            var retryAfter = Convert.ToInt32(rule.PeriodTimespan.TotalSeconds);
            retryAfter = retryAfter > 1 ? retryAfter - secondsPast : 1;
            return retryAfter.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        #endregion
    }
}
