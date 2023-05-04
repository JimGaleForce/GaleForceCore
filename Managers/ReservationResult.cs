using System;

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
    }

    public class QueueInfo
    {
        public int Used { get; set; }

        public int Of { get; set; }

        public TimeSpan In { get; set; }

        public int Completed { get; set; }
    }
}
