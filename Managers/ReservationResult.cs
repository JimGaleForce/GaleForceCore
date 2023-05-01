using System;

namespace GaleForceCore.Managers
{
    public class ReservationResult
    {
        public ReservationStatus Status { get; set; }

        public int ReservationId { get; set; }

        public int? RemainingSlots { get; set; }
    }
}
