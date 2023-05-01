using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GaleForceCore.Managers
{
    public interface IDataSource
    {
        int GetQuota(string bucketKey);

        int GetPriorityQueueCount(string bucketKey);

        TimeSpan GetReservationTimeout(string bucketKey);

        void UpdateBucketUsage(string bucketKey, int usedSlots);

        ConcurrentDictionary<int, Reservation> GetQueuedReservations(string bucketKey);

        int AddReservation(string bucketKey, int requestedSlots, int pri, DateTime requestTime);

        void RemoveReservation(string bucketKey, int reservationId);

        void UpdateReservationStatus(string bucketKey, int reservationId, ReservationStatus status);

        Reservation GetReservationStatus(string bucketKey, int reservationId);

        TimeSpan GetTimeWindow(string bucketKey);

        List<DateTime> GetRequestTimestamps(string bucketKey);

        void UpdateRequestTimestamps(string bucketKey, List<DateTime> requestTimestamps);

        int GetUsed(string bucketKey);
    }
}
