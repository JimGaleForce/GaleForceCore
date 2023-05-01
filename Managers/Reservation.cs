using System;

namespace GaleForceCore.Managers
{
    public class Reservation
    {
        public int Id { get; set; }

        public int RequestedSlots { get; set; }

        public DateTime RequestTime { get; set; }

        public ReservationStatus Status { get; set; }

        public int Priority { get; set; }

        public override string ToString()
        {
            return $"Id={Id}, Req={RequestedSlots}, Status={Status}, Pri={Priority}, Time={RequestTime}";
        }
    }

    public enum ReservationStatus
    {
        Waiting = 1,
        Allowed = 2,
        Denied = 3,
        Expired = 4,
        Released = 5,
        Missing = 6
    }
}
