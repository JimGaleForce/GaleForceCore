using GaleForceCore.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GaleForceCore.Managers
{
    public class Throttler2
    {
        private readonly IDataSource2 _dataSource;
        private readonly object _syncLock = new object();

        public Throttler2(IDataSource2 dataSource)
        {
            _dataSource = dataSource;
        }

        public ReservationResult RequestSlots(
            string key,
            int requestedSlots,
            int minReadySlots = 1,
            int priority = 5,
            DateTime? dateTime = null)
        {
            lock (_syncLock)
            {
                var response = new ReservationResult
                {
                    ReservationId = _dataSource.GetNewId()                    
                };

                // settle current locks 
                SettleCurrentLocks(key, dateTime);

                var currentAvailableSlots = GetAvailableCount(key);

                if (currentAvailableSlots >= minReadySlots)
                {
                    Use(minReadySlots, requestedSlots, response, key);
                }
            }
        }

        private void Use(int useSlots, int ofSlots, ReservationResult result, string key)
        {
            var reservations = _dataSource.GetReservations(key);
            var bucket = _dataSource.GetBucket(key);

            if (!reservations.ContainsKey(result.ReservationId))
            {
                var res = new Reservation2
                {
                    Id = result.ReservationId,

                    // here!
                };

                reservations.GetOrAdd(result.ReservationId, );
            }
        }

        private int GetAvailableCount(string key)
        {
            var reservations = _dataSource.GetReservations(key);
            var bucket = _dataSource.GetBucket(key);

            if (bucket == null || reservations == null)
            {
                return 0;
            }

            var used = reservations.Where(r => r.Value.Status == Status.Ready || r.Value.Status == Status.Allocated)
                .Sum(r => r.Value.RequestedSlots);

            return bucket.QuotaPerMinute - used;
        }

        private bool SettleCurrentLocks(string key, DateTime? dateTime = null)
        {
            var reservations = _dataSource.GetReservations(key);
            var bucket = _dataSource.GetBucket(key);

            if (reservations == null)
            {
                return false;
            }

            // expire current locks
            foreach (var res in reservations)
            {
                if (res.Value.Status == Status.Awaiting || res.Value.Status == Status.Ready)
                {
                    var timeout = res.Value.AwaitingReadyTimeout ??
                        bucket.DefaultAwaitingReadyTimeout ??
                        TimeSpan.FromMinutes(1);
                    var maxDateTime = TimeHelpers.Max(res.Value.Requested, res.Value.Checked).Add(timeout);
                    if (maxDateTime < dateTime)
                    {
                        res.Value.Status = Status.Abandoned;
                    }
                }
                else if (res.Value.Status == Status.Allocated)
                {
                    var timeout = res.Value.AllocatedTimeout ??
                        bucket.DefaultAllocatedTimeout ??
                        TimeSpan.FromMinutes(1);
                    var maxDateTime = TimeHelpers.Max(res.Value.Requested, res.Value.Checked).Add(timeout);
                    if (maxDateTime < dateTime)
                    {
                        res.Value.Status = Status.Abandoned;
                    }
                }

                if (res.Value.Status > Status.Allocated)
                {
                    var timeout = bucket.RequestDropoff ?? TimeSpan.FromMinutes(2);
                    var maxDateTime = TimeHelpers.Max(res.Value.Requested, res.Value.Checked).Add(timeout);
                    if (maxDateTime < dateTime)
                    {
                        res.Value.Status = Status.Deleted;
                    }
                }
            }

            // remove reservations that have a status for Status.Deleted
            var deletables = reservations.Where(r => r.Value.Status == Status.Deleted).Select(r => r.Key).ToList();
            if (deletables.Any())
            {
                foreach (var deletable in deletables)
                {
                    reservations.TryRemove(deletable, out _);
                }
            }

            return true;
        }
    }

    public interface IDataSource2
    {
        bool AddBucket(string key, BucketInfo bucket);

        BucketInfo GetBucket(string key);

        ConcurrentDictionary<int, Reservation2> GetReservations(string key);

        int GetNewId();
    }

    public class MemoryDataSource2 : IDataSource2
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<int, Reservation2>> _reservations;
        private ConcurrentDictionary<string, BucketInfo> _buckets;
        private int nextId = 1;

        public bool AddBucket(string key, BucketInfo bucket)
        {
            if (_buckets.ContainsKey(key))
            {
                return false;
            }

            _buckets[key] = bucket;
            return true;
        }

        public BucketInfo GetBucket(string key)
        {
            return _buckets.ContainsKey(key) ? _buckets[key] : null;
        }

        public ConcurrentDictionary<int, Reservation2> GetReservations(string key)
        {
            return _reservations.ContainsKey(key) ? _reservations[key] : null;
        }

        public int GetNewId()
        {
            return nextId++;
        }
    }

    public class BucketInfo
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public int QuotaPerMinute { get; set; }

        public int QuotaPerSecond { get; set; }

        public TimeSpan? DefaultAwaitingReadyTimeout { get; set; }

        public TimeSpan? DefaultAllocatedTimeout { get; set; }

        public TimeSpan? RequestDropoff { get; set; }
    }

    public class Reservation2
    {
        public int Id { get; set; }

        public string Owner { get; set; }

        public string App { get; set; }

        public Status Status { get; set; }

        public DateTime Requested { get; set; }

        public DateTime Checked { get; set; }

        public List<DateTime> Used { get; set; } = new List<DateTime>();

        public TimeSpan? AwaitingReadyTimeout { get; set; }

        public TimeSpan? AllocatedTimeout { get; set; }

        public int RequestedSlots { get; set; }
    }

    public enum Status
    {
        None = 0,
        Awaiting = 1,
        Ready = 2,
        Allocated = 3,
        InUse = 4,
        Completed = 5,
        Released = 6,
        Abandoned = 7,
        Deleted = 8
    }

    public class Throttler
    {
        private readonly IDataSource _dataSource;
        private readonly object _syncLock = new object();

        public Throttler(IDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public ReservationResult RequestSlots(
            string bucketKey,
            int requestedSlots,
            int pri = 1,
            DateTime? dateTime = null)
        {
            lock (_syncLock)
            {
                UpdateWaitingReservations(bucketKey, dateTime);

                var quota = _dataSource.GetQuota(bucketKey);
                var timeWindow = _dataSource.GetTimeWindow(bucketKey);
                var requestTimestamps = _dataSource.GetRequestTimestamps(bucketKey);

                CleanExpiredRequests(requestTimestamps, timeWindow, dateTime);

                int reservationId;

                if (requestTimestamps.Count + requestedSlots <= quota)
                {
                    for (int i = 0; i < requestedSlots; i++)
                    {
                        requestTimestamps.Add(dateTime ?? DateTime.UtcNow);
                    }

                    _dataSource.UpdateRequestTimestamps(bucketKey, requestTimestamps);
                    reservationId = _dataSource.AddReservation(
                        bucketKey,
                        requestedSlots,
                        pri,
                        dateTime ?? DateTime.UtcNow);

                    return new ReservationResult
                    {
                        Status = ReservationStatus.Allowed,
                        RemainingSlots = quota - (requestTimestamps.Count + requestedSlots),
                        ReservationId = reservationId
                    };
                }

                reservationId = _dataSource.AddReservation(bucketKey, requestedSlots, pri, dateTime ?? DateTime.UtcNow);
                return new ReservationResult
                {
                    Status = ReservationStatus.Waiting,
                    ReservationId = reservationId
                };
            }
        }

        public ReservationResult CheckReservationStatus(string bucketKey, int reservationId, DateTime? dateTime = null)
        {
            lock (_syncLock)
            {
                var reservation = _dataSource.GetReservationStatus(bucketKey, reservationId);

                if (reservation == null)
                {
                    return null;
                }

                if (reservation.Status == ReservationStatus.Waiting)
                {
                    UpdateWaitingReservations(bucketKey, dateTime);

                    if (reservation.Status == ReservationStatus.Allowed)
                    {
                        return new ReservationResult
                        {
                            Status = ReservationStatus.Allowed,
                            ReservationId = reservationId
                        };
                    }
                }

                return new ReservationResult
                {
                    Status = reservation.Status,
                    ReservationId = reservationId
                };
            }
        }

        public void ReleaseReservation(string bucketKey, int reservationId, DateTime? dateTime = null)
        {
            lock (_syncLock)
            {
                _dataSource.RemoveReservation(bucketKey, reservationId);
                UpdateWaitingReservations(bucketKey, dateTime);
            }
        }

        private List<int> OrderQueuedReservations(ConcurrentDictionary<int, Reservation> reservations)
        {
            return reservations
                .OrderBy(x => x.Value.Status)
                .OrderByDescending(x => x.Value.Priority)
                .OrderBy(x => x.Value.RequestTime)
                .Select(x => x.Key)
                .ToList();
        }

        private void UpdateWaitingReservations(string bucketKey, DateTime? dateTime = null)
        {
            var remainingSlots = GetRemainingSlots(bucketKey);

            var queuedReservations = OrderQueuedReservations(_dataSource.GetQueuedReservations(bucketKey));
            remainingSlots = ProcessReservationsTimeouts(bucketKey, dateTime, remainingSlots, queuedReservations);

            queuedReservations = OrderQueuedReservations(_dataSource.GetQueuedReservations(bucketKey));
            remainingSlots = ProcessReservations(bucketKey, dateTime, remainingSlots, queuedReservations);
        }

        private int ProcessReservationsTimeouts(
            string bucketKey,
            DateTime? dateTime,
            int remainingSlots,
            List<int> queuedReservations)
        {
            foreach (var reservationId in queuedReservations)
            {
                var reservation = _dataSource.GetReservationStatus(bucketKey, reservationId);

                var dt = dateTime ?? DateTime.UtcNow;
                if (dt - reservation.RequestTime > _dataSource.GetReservationTimeout(bucketKey))
                {
                    reservation.Status = ReservationStatus.Expired;
                    _dataSource.RemoveReservation(bucketKey, reservationId);
                    continue;
                }
            }

            return remainingSlots;
        }

        private int ProcessReservations(
            string bucketKey,
            DateTime? dateTime,
            int remainingSlots,
            List<int> queuedReservations)
        {
            foreach (var reservationId in queuedReservations)
            {
                var reservation = _dataSource.GetReservationStatus(bucketKey, reservationId);

                if (remainingSlots >= reservation.RequestedSlots)
                {
                    reservation.Status = ReservationStatus.Allowed;
                    _dataSource.UpdateReservationStatus(bucketKey, reservationId, ReservationStatus.Allowed);
                    _dataSource.UpdateBucketUsage(bucketKey, reservation.RequestedSlots);
                    remainingSlots -= reservation.RequestedSlots;
                }
            }

            return remainingSlots;
        }

        private int GetRemainingSlots(string bucketKey)
        {
            var quota = _dataSource.GetQuota(bucketKey);
            var usedSlots = _dataSource.GetUsed(bucketKey);

            // _dataSource.GetPriorityQueueCount(bucketKey);
            var remainingSlots = quota - usedSlots;

            return remainingSlots;
        }

        private bool IsQuotaExceeded(string bucketKey, int requestedSlots)
        {
            var quota = _dataSource.GetQuota(bucketKey);
            var usedSlots = _dataSource.GetPriorityQueueCount(bucketKey);
            var totalRequests = usedSlots + requestedSlots;

            return totalRequests > quota;
        }

        private void CleanExpiredRequests(
            List<DateTime> requestTimestamps,
            TimeSpan timeWindow,
            DateTime? dateTime = null)
        {
            DateTime threshold = (dateTime ?? DateTime.UtcNow) - timeWindow;
            requestTimestamps.RemoveAll(timestamp => timestamp < threshold);
        }
    }
}
