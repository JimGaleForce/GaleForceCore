namespace GaleForceCore.Managers
{
    public class ThrottlerRequest
    {
        public string Key { get; set; }

        public int RequestedSlots { get; set; }

        public int MinReadySlots { get; set; } = 1;

        public int Priority { get; set; } = 5;

        public string Owner { get; set; }

        public string App { get; set; }

        public string Instance { get; set; }
    }
}
