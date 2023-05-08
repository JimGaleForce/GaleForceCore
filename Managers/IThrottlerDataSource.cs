using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GaleForceCore.Managers
{
    public interface IThrottlerDataSource
    {
        bool AddBucket(string key, BucketInfo bucket);

        BucketInfo GetBucket(string key);

        ConcurrentDictionary<int, Reservation> GetReservations(string key);

        int GetNewId();

        void UseSlot(string key, string app, DateTime? dateTime);

        List<UsedSlot> GetUsed(string key);

        void ProvideValues(ReservationResult response);

        List<string> GetKeys();
    }
}
