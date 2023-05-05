using System;

namespace GaleForceCore.Managers
{
    public class QueueInfo
    {
        public int Used { get; set; }

        public int Of { get; set; }

        public int Waiting { get; set; }

        public int ReservedOut { get; set; }

        public int ReservedIn { get; set; }

        public TimeSpan In { get; set; }

        public int Done { get; set; }

        public int Ready { get; set; }
    }
}
