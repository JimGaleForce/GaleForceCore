using System;
using System.Collections.Generic;
using System.Timers;

namespace GaleForceCore.MemStorage
{
    public class PersistentCache<T, T2> : Dictionary<T, T2>
    {
        private static Dictionary<T, T2> cache = null;
        private static Timer idle = new Timer() { AutoReset = true, Enabled = false, Interval = 5000 };
        private static Timer timeout = new Timer() { AutoReset = true, Enabled = false, Interval = 30000 };

        public event Action Save;

        public PersistentCache()
        {
            idle.Elapsed += Idle_Elapsed;
            timeout.Elapsed += Timeout_Elapsed;
        }

        private void Timeout_Elapsed(object sender, ElapsedEventArgs e) { DoSave(); }

        private void Idle_Elapsed(object sender, ElapsedEventArgs e) { DoSave(); }

        private void DoSave()
        {
            idle.Enabled = false;
            timeout.Enabled = false;

            Save?.Invoke();
        }

        public T2 Get(T key) { return this.ContainsKey(key) ? this[key] : default(T2); }

        public void Set(T key, T2 value)
        {
            this[key] = value;
            idle.Start();
            idle.Enabled = true;
            if(!timeout.Enabled)
            {
                timeout.Start();
                timeout.Enabled = true;
            }
        }
    }
}