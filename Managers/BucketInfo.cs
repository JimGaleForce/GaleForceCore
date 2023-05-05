using System;
using System.Collections.Generic;

namespace GaleForceCore.Managers
{
    public class BucketInfo
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public int QuotaPerTimeSpan { get; set; }

        public TimeSpan TimeSpan { get; set; } = TimeSpan.FromMinutes(1);

        public TimeSpan? DefaultAwaitingReadyTimeout { get; set; }

        public TimeSpan? DefaultAllocatedTimeout { get; set; }

        public TimeSpan? RequestDropoff { get; set; }

        public Dictionary<string, int> ReservedSlots { get; set; } = new Dictionary<string, int>();
    }
}
