using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GaleForceCore.Managers
{
    public class MemoryDataSource : IDataSource
    {
        private readonly ConcurrentDictionary<string, BucketData> _buckets = new ConcurrentDictionary<string, BucketData>();

        public int GetQuota(string bucketKey) => _buckets[bucketKey].Quota;

        public int GetPriorityQueueCount(string bucketKey) => _buckets[bucketKey].PriorityQueueCount;

        public TimeSpan GetReservationTimeout(string bucketKey) => _buckets[bucketKey].ReservationTimeout;

        public void UpdateBucketUsage(string bucketKey, int usedSlots) => _buckets[bucketKey].UsedSlots += usedSlots;

        public ConcurrentDictionary<int, Reservation> GetQueuedReservations(string bucketKey)
        {
            return _buckets[bucketKey].Reservations;
        }

        public int GetUsed(string bucketKey)
        {
            return _buckets[bucketKey].Reservations
                .Where(r => r.Value.Status == ReservationStatus.Waiting)
                .Sum(r => r.Value.RequestedSlots);
        }

        public int AddReservation(string bucketKey, int requestedSlots, int pri, DateTime requestTime)
        {
            var reservationId = _buckets[bucketKey].NextReservationId++;
            _buckets[bucketKey].Reservations
                .TryAdd(
                    reservationId,
                    new Reservation
                    {
                        Id = reservationId,
                        Status = ReservationStatus.Waiting,
                        RequestedSlots = requestedSlots,
                        RequestTime = requestTime,
                        Priority = pri
                    });

            return reservationId;
        }

        public void RemoveReservation(string bucketKey, int reservationId)
        {
            if (CheckId(bucketKey, reservationId))
            {
                _buckets[bucketKey].Reservations.TryRemove(reservationId, out _);
            }
        }

        public void UpdateReservationStatus(string bucketKey, int reservationId, ReservationStatus status)
        {
            if (CheckId(bucketKey, reservationId))
            {
                _buckets[bucketKey].Reservations[reservationId].Status = status;
            }
        }

        public Reservation GetReservationStatus(string bucketKey, int reservationId)
        {
            return CheckId(bucketKey, reservationId) ? _buckets[bucketKey].Reservations[reservationId] : null;
        }

        public TimeSpan GetTimeWindow(string bucketKey) => _buckets[bucketKey].TimeWindow;

        public List<DateTime> GetRequestTimestamps(string bucketKey)
        {
            return _buckets[bucketKey].RequestTimestamps.ToList();
        }

        public void UpdateRequestTimestamps(string bucketKey, List<DateTime> requestTimestamps)
        {
            _buckets[bucketKey].RequestTimestamps = new ConcurrentBag<DateTime>(requestTimestamps);
        }

        public void AddBucket(
            string bucketKey,
            int quota,
            int priorityQueueCount,
            TimeSpan reservationTimeout,
            TimeSpan timeWindow)
        {
            _buckets.TryAdd(
                bucketKey,
                new BucketData
                {
                    Quota = quota,
                    PriorityQueueCount = priorityQueueCount,
                    ReservationTimeout = reservationTimeout,
                    TimeWindow = timeWindow
                });
        }

        private bool CheckId(string bucketKey, int reservationId)
        {
            return _buckets.ContainsKey(bucketKey) && _buckets[bucketKey].Reservations.ContainsKey(reservationId);
        }
    }
}
