using System;
using System.Collections.Concurrent;

namespace GaleForceCore.Managers
{
    public class BucketData
    {
        public int Quota { get; set; }

        public int PriorityQueueCount { get; set; }

        public TimeSpan ReservationTimeout { get; set; }

        public int UsedSlots { get; set; }

        public int NextReservationId { get; set; } = 1;

        public ConcurrentDictionary<int, Reservation> Reservations
        {
            get;
            set;
        } = new ConcurrentDictionary<int, Reservation>();

        public TimeSpan TimeWindow { get; set; }

        public ConcurrentBag<DateTime> RequestTimestamps { get; set; } = new ConcurrentBag<DateTime>();
    }
}
