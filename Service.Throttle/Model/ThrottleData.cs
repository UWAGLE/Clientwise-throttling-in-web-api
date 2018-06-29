using System.Collections.Generic;

namespace Service.Throttle.Model
{
    public class Client
    {
        public string clientName { get; set; }
        public string clientKey { get; set; }
        public ThrottlePolicy throttlePolicy { get; set; }
    }
    public class ThrottlePolicy
    {
        public string[] whiteListClients { get; set; }
        public List<ClientRateLimiting> clientRateLimiting { get; set; }
    }
    public class ClientRateLimiting
    {
        public string key { get; set; }
        public string keyType { get; set; }
        public List<Policy> policy { get; set; }
    }
}
