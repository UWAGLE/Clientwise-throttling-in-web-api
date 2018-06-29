using System;

namespace Service.Throttle.Model
{
    [Serializable]
    public struct ThrottleCounter
    {
        public DateTime Timestamp { get; set; }
        public long TotalRequests { get; set; }
    }
}
