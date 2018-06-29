using System;

namespace Service.Throttle.Model
{
    public class Policy
    {
        public string period { get; set; }
        public int limit { get; set; }
        public TimeSpan PeriodTimespan { get; set; }
    }

    enum KeyType
    {
        ip,
        clientKey,
        userSessionId
    }
}
