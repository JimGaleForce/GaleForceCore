using System;
using System.Collections.Generic;

namespace GaleForceCore.Managers
{
    public class ReservationResult
    {
        public string Key { get; set; }

        public int Id { get; set; }

        public Status Status { get; set; }

        public int Slots { get; set; }

        public int AllocatedSlots { get; set; }

        public int MinimumWaitSeconds { get; set; }

        public QueueInfo Info { get; set; }

        public Dictionary<string, string> Values { get; set; }
    }
}
