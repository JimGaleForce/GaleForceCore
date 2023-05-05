using System;
using System.Collections.Generic;
using System.Linq;

namespace GaleForceCore.Managers
{
    public class Reservation
    {
        public int Id { get; set; }

        public string Owner { get; set; }

        public string App { get; set; }

        public string Instance { get; set; }

        public int Priority { get; set; }

        public Status Status { get; set; }

        public DateTime Requested { get; set; }

        public DateTime Checked { get; set; }

        public List<DateTime> Used { get; set; } = new List<DateTime>();

        public TimeSpan? AwaitingReadyTimeout { get; set; }

        public TimeSpan? AllocatedTimeout { get; set; }

        public int RequestedSlots { get; set; }

        public int RemainingSlots { get; set; }

        public static Reservation From(ThrottlerRequest request)
        {
            var res = new Reservation()
            {
                Owner = request.Owner,
                App = request.App,
                RequestedSlots = request.RequestedSlots,
                RemainingSlots = request.RequestedSlots,
                Priority = request.Priority,
                Status = Status.Awaiting,
                Instance = request.Instance
            };

            return res;
        }
    }
}
