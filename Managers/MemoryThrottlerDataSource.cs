using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GaleForceCore.Managers
{
    public class MemoryThrottlerDataSource : IThrottlerDataSource
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<int, Reservation>> _reservations = new ConcurrentDictionary<string, ConcurrentDictionary<int, Reservation>>();
        private ConcurrentDictionary<string, BucketInfo> _buckets = new ConcurrentDictionary<string, BucketInfo>();
        private ConcurrentDictionary<string, List<UsedSlot>> _used = new ConcurrentDictionary<string, List<UsedSlot>>();

        private int nextId = 1;

        public bool AddBucket(string key, BucketInfo bucket)
        {
            if (this._buckets != null && this._buckets.ContainsKey(key))
            {
                return false;
            }

            this._buckets.TryAdd(key, bucket);
            return true;
        }

        public BucketInfo GetBucket(string key)
        {
            return this._buckets.ContainsKey(key) ? this._buckets[key] : null;
        }

        public ConcurrentDictionary<int, Reservation> GetReservations(string key)
        {
            return this._reservations.GetOrAdd(key, keyx => new ConcurrentDictionary<int, Reservation>());
        }

        public int GetNewId()
        {
            return this.nextId++;
        }

        public void UseSlot(string key, string app, DateTime? dateTime = null)
        {
            var items = this._used.GetOrAdd(key, (keyx) => new List<UsedSlot>());
            items.Add(new UsedSlot { DateTime = dateTime ?? DateTime.UtcNow, App = app });
        }

        public List<UsedSlot> GetUsed(string key)
        {
            return this._used.GetOrAdd(key, (keyx) => new List<UsedSlot>());
        }

        public void ProvideValues(ReservationResult response)
        {
            return;
        }

        public List<string> GetKeys()
        {
            return this._buckets.Keys.ToList();
        }
    }
}
