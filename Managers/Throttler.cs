using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GaleForceCore.Helpers;

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

        public ReservationResult RequestSlots(ThrottlerRequest request, DateTime? dateTime = null)
        {
            lock (_syncLock)
            {
                var dt = dateTime ?? DateTime.UtcNow;

                var response = new ReservationResult
                {
                    Id = _dataSource.GetNewId(),
                    Key = request.Key,
                    Status = Status.Awaiting,
                    AllocatedSlots = 0,
                    Slots = 0
                };

                var reservations = _dataSource.GetReservations(request.Key);
                var bucket = _dataSource.GetBucket(request.Key);

                // clear ALL matching instances from this app/owner
                ClearLocksFrom(request, reservations);

                // add a new request into the correct place
                var res = reservations.GetOrAdd(response.Id, (reservationId) => Reservation2.From(request));
                res.Requested = dt;
                res.Checked = dt;
                res.Status = response.Status;

                return _CheckAndUse(response, dt);
            }
        }

        public ReservationResult CheckAndUse(ReservationResult response, DateTime? dateTime = null)
        {
            lock (_syncLock)
            {
                return _CheckAndUse(response, dateTime);
            }
        }

        private ReservationResult _CheckAndUse(ReservationResult response, DateTime? dateTime = null)
        {
            if (string.IsNullOrEmpty(response.Key) || response.Id == 0)
            {
                return null;
            }

            var dt = dateTime ?? DateTime.UtcNow;

            var reservations = _dataSource.GetReservations(response.Key);
            var bucket = _dataSource.GetBucket(response.Key);

            // settle current locks 
            SettleCurrentLocks(response.Key, reservations, bucket, dt);
            var minSeconds = AllowAvailableSlots(response.Key, reservations, dt);
            response.MinimumWaitSeconds = minSeconds;

            var status = reservations.FirstOrDefault(r => r.Key == response.Id);
            if (status.Key == 0)
            {
                response.Status = Status.Deleted;
                return response;
            }

            status.Value.Checked = dt;

            if (status.Value.Status == Status.Ready || status.Value.Status == Status.Allocated)
            {
                response.Status = Status.Granted;
                response.Slots =
                    status.Value.RequestedSlots;

                for (var i = 0; i < response.Slots; i++)
                {
                    _dataSource.UseSlot(response.Key, dt);
                }

                status.Value.RemainingSlots -= response.Slots;
                if (status.Value.RemainingSlots > 0)
                {
                    status.Value.Status = Status.Allocated;
                }
                else
                {
                    status.Value.Status = Status.InUse;
                }
            }
            else
            {
                response.Status = status.Value.Status;
                response.Slots = status.Value.RemainingSlots;
            }

            return response;
        }

        public ReservationResult ReleaseSlots(ReservationResult response)
        {
            if (string.IsNullOrEmpty(response.Key) || response.Id == 0)
            {
                return null;
            }

            var reservations = _dataSource.GetReservations(response.Key);
            var bucket = _dataSource.GetBucket(response.Key);

            var status = reservations.FirstOrDefault(r => r.Key == response.Id);
            if (status.Key == 0)
            {
                response.Status = Status.Deleted;
                return response;
            }

            if (status.Value.Status == Status.Ready ||
                status.Value.Status == Status.Allocated ||
                status.Value.Status == Status.Awaiting)
            {
                status.Value.Status = Status.Released;
                response.Status = Status.Released;
                response.Slots = status.Value.RemainingSlots;
            }

            return response;
        }

        private int AllowAvailableSlots(
            string key,
            ConcurrentDictionary<int, Reservation2> reservations,
            DateTime? dateTime = null)
        {
            var list = reservations.Values.ToList();
            var ready = list.Count(r => r.Status == Status.Ready || r.Status == Status.Allocated);
            var awaiting = list.Where(r => r.Status == Status.Awaiting);

            var usedOf = _dataSource.CountUsedOf(key, dateTime);
            var sysAvailable = usedOf.Item2 - usedOf.Item1;
            var available = sysAvailable - ready;
            if (available > 0)
            {
                var ordered = awaiting.OrderBy(r => r.Priority).ThenBy(r => r.Requested).Take(available);
                foreach (var o in ordered)
                {
                    if (o.RequestedSlots > available)
                    {
                        break;
                    }

                    o.Status = Status.Ready;
                    available -= o.RequestedSlots;
                }
            }

            return usedOf.Item3;
        }

        private void ClearLocksFrom(ThrottlerRequest request, ConcurrentDictionary<int, Reservation2> reservations)
        {
            var matches = reservations.Where(
                r => r.Value.Instance == request.Instance &&
                    r.Value.Owner == request.Owner &&
                    r.Value.App == request.App)
                .ToList();
            foreach (var m in matches)
            {
                m.Value.Status = Status.Deleted;
            }
        }

        private bool SettleCurrentLocks(
            string key,
            ConcurrentDictionary<int, Reservation2> reservations,
            BucketInfo bucket,
            DateTime? dateTime = null)
        {
            if (reservations == null)
            {
                return false;
            }

            var dt = dateTime ?? DateTime.UtcNow;

            // remove reservations that have a status for Status.Deleted
            var deletables = reservations.Where(r => r.Value.Status == Status.Deleted).Select(r => r.Key).ToList();
            if (deletables.Any())
            {
                foreach (var deletable in deletables)
                {
                    reservations.TryRemove(deletable, out _);
                }
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
                    if (maxDateTime < dt)
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
                    if (maxDateTime < dt)
                    {
                        res.Value.Status = Status.Abandoned;
                    }
                }

                if (res.Value.Status > Status.Allocated)
                {
                    var timeout = bucket.RequestDropoff ?? TimeSpan.FromMinutes(2);
                    var maxDateTime = TimeHelpers.Max(res.Value.Requested, res.Value.Checked).Add(timeout);
                    if (maxDateTime < dt)
                    {
                        res.Value.Status = Status.Deleted;
                    }
                }
            }

            // remove reservations that have a status for Status.Deleted
            deletables = reservations.Where(r => r.Value.Status == Status.Deleted).Select(r => r.Key).ToList();
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

        Tuple<int, int, int> CountUsedOf(string key, DateTime? dateTime = null);

        void UseSlot(string key, DateTime? dateTime);
    }

    public class MemoryDataSource2 : IDataSource2
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<int, Reservation2>> _reservations = new ConcurrentDictionary<string, ConcurrentDictionary<int, Reservation2>>();
        private ConcurrentDictionary<string, BucketInfo> _buckets = new ConcurrentDictionary<string, BucketInfo>();
        private ConcurrentDictionary<string, List<DateTime>> _used = new ConcurrentDictionary<string, List<DateTime>>();

        private int nextId = 1;

        public bool AddBucket(string key, BucketInfo bucket)
        {
            if (_buckets != null && _buckets.ContainsKey(key))
            {
                return false;
            }

            _buckets.TryAdd(key, bucket);
            return true;
        }

        public BucketInfo GetBucket(string key)
        {
            return _buckets.ContainsKey(key) ? _buckets[key] : null;
        }

        public ConcurrentDictionary<int, Reservation2> GetReservations(string key)
        {
            return _reservations.GetOrAdd(key, keyx => new ConcurrentDictionary<int, Reservation2>());
        }

        public int GetNewId()
        {
            return nextId++;
        }

        public Tuple<int, int, int> CountUsedOf(string key, DateTime? dateTime = null)
        {
            var bucket = GetBucket(key);

            if (_used == null)
            {
                return new Tuple<int, int, int>(0, bucket.QuotaPerTimeSpan, 0);
            }

            var seconds = bucket.TimeSpan.TotalSeconds;
            var expireSeconds = seconds * 2;
            if (bucket.RequestDropoff != null)
            {
                expireSeconds = bucket.RequestDropoff.Value.TotalSeconds;
            }

            var dt = dateTime ?? DateTime.UtcNow;
            var dtMin = dt.AddSeconds(-seconds);

            var dtExpire = dt.AddSeconds(-expireSeconds);
            var items = _used.GetOrAdd(key, (keyx) => new List<DateTime>());

            var toDel = items.Where(i => i < dtExpire).ToList();
            foreach (var td in toDel)
            {
                items.Remove(td);
            }

            var count = items.Count(idt => idt >= dtMin);
            var ordered = items.OrderByDescending(i => i).Take(bucket.QuotaPerTimeSpan);
            var minSeconds = 2;
            if (ordered.Count() == bucket.QuotaPerTimeSpan)
            {
                minSeconds = Math.Max(2, ((int)((ordered.Last().AddSeconds(seconds) - dt).TotalSeconds + 0.99)));
            }

            return new Tuple<int, int, int>(count, bucket.QuotaPerTimeSpan, minSeconds);
        }

        public void UseSlot(string key, DateTime? dateTime = null)
        {
            var items = _used.GetOrAdd(key, (keyx) => new List<DateTime>());
            items.Add(dateTime ?? DateTime.UtcNow);
        }
    }

    public class BucketInfo
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public int QuotaPerTimeSpan { get; set; }

        public TimeSpan TimeSpan { get; set; } = TimeSpan.FromMinutes(1);

        public TimeSpan? DefaultAwaitingReadyTimeout { get; set; }

        public TimeSpan? DefaultAllocatedTimeout { get; set; }

        public TimeSpan? RequestDropoff { get; set; }
    }

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

    public class Reservation2
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

        public static Reservation2 From(ThrottlerRequest request)
        {
            var res = new Reservation2()
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

    public enum Status
    {
        None = 0,
        Awaiting = 1,
        Ready = 2,
        Granted = 3, //only used to hand to caller when consuming - server is InUse
        Allocated = 4,
        InUse = 5,
        Completed = 6,
        Released = 7,
        Abandoned = 8,
        Deleted = 9
    }
}
