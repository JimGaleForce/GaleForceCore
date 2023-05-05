using System;
using System.Collections.Concurrent;
using System.Linq;
using GaleForceCore.Helpers;

namespace GaleForceCore.Managers
{
    public class Throttler
    {
        private readonly IThrottlerDataSource _dataSource;
        private readonly object _syncLock = new object();

        public Throttler(IThrottlerDataSource dataSource)
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
                var res = reservations.GetOrAdd(response.Id, (reservationId) => Reservation.From(request));
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

        public ReservationResult CheckQueueInfo(ReservationResult response, DateTime? dateTime = null)
        {
            var dt = dateTime ?? DateTime.UtcNow;

            var reservations = _dataSource.GetReservations(response.Key);
            var bucket = _dataSource.GetBucket(response.Key);
            var used = _dataSource.GetUsed(response.Key);
            var minDt = dt.AddSeconds(-bucket.TimeSpan.TotalSeconds);
            var count = used.Count(u => u.DateTime >= minDt);
            var completed = reservations.Where(r => r.Value.Status > Status.Allocated).Sum(r => r.Value.RequestedSlots);
            var waiting = reservations.Where(r => r.Value.Status == Status.Awaiting).Sum(r => r.Value.RequestedSlots);
            var ready = reservations.Where(r => r.Value.Status == Status.Ready || r.Value.Status == Status.Allocated)
                .Sum(r => r.Value.RequestedSlots);

            response.Info = new QueueInfo
            {
                Done = completed,
                Used = count,
                Of = bucket.QuotaPerTimeSpan,
                Waiting = waiting,
                In = bucket.TimeSpan,
                Ready = ready
            };

            return response;
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
            var minSeconds = AllowAvailableSlots(response, reservations, bucket, dt);
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
                _dataSource.ProvideValues(response);
                response.Slots =
                    status.Value.RequestedSlots;

                for (var i = 0; i < response.Slots; i++)
                {
                    _dataSource.UseSlot(response.Key, status.Value.App, dt);
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

            CheckQueueInfo(response);

            return response;
        }

        private int AllowAvailableSlots(
            ReservationResult response,
            ConcurrentDictionary<int, Reservation> reservations,
            BucketInfo bucket,
            DateTime? dateTime = null)
        {
            var key = response.Key;
            var list = reservations.Values.ToList();
            var ready = list.Count(r => r.Status == Status.Ready || r.Status == Status.Allocated);
            var awaiting = list.Where(r => r.Status == Status.Awaiting);

            var usedOf = CountUsedOf(key, dateTime);
            var sysAvailable = usedOf.Item2 - usedOf.Item1;
            var available = sysAvailable - ready;
            if (available > 0)
            {
                var onlyProcessedReservedSlots = false;
                var ordered = awaiting.OrderBy(r => r.Priority).ThenBy(r => r.Requested);
                foreach (var o in ordered)
                {
                    // summ it all
                    var reserveds = GetReservedByOtherAndMyApp(key, o.App);

                    var reservedByOtherApps = reserveds.Item1;
                    var reservedByMyApp = reserveds.Item2;
                    var actuallyAvailable = onlyProcessedReservedSlots
                        ? reservedByMyApp
                        : Math.Max(0, available - reservedByOtherApps);

                    if (!onlyProcessedReservedSlots && o.RequestedSlots > actuallyAvailable)
                    {
                        onlyProcessedReservedSlots = true;
                    }

                    if (o.RequestedSlots <= actuallyAvailable)
                    {
                        o.Status = Status.Ready;
                        available -= o.RequestedSlots;
                    }
                }
            }

            return usedOf.Item3;
        }

        private Tuple<int, int, int> CountUsedOf(string key, DateTime? dateTime = null)
        {
            var bucket = _dataSource.GetBucket(key);

            var seconds = bucket.TimeSpan.TotalSeconds;
            var expireSeconds = seconds * 2;
            if (bucket.RequestDropoff != null)
            {
                expireSeconds = bucket.RequestDropoff.Value.TotalSeconds;
            }

            var dt = dateTime ?? DateTime.UtcNow;
            var dtMin = dt.AddSeconds(-seconds);

            var dtExpire = dt.AddSeconds(-expireSeconds);
            var items = _dataSource.GetUsed(key);

            var toDel = items.Where(i => i.DateTime < dtExpire).ToList();
            foreach (var td in toDel)
            {
                items.Remove(td);
            }

            var count = items.Count(idt => idt.DateTime >= dtMin);
            var ordered = items.OrderByDescending(i => i.DateTime).ToList();
            var minSeconds = 2;
            if (ordered.Count() >= bucket.QuotaPerTimeSpan)
            {
                minSeconds = Math.Max(2, (int)((ordered.Last().DateTime.AddSeconds(seconds) - dt).TotalSeconds + 0.99));
            }

            return new Tuple<int, int, int>(count, bucket.QuotaPerTimeSpan, minSeconds);
        }

        private Tuple<int, int> GetReservedByOtherAndMyApp(string key, string app, DateTime? dateTime = null)
        {
            var bucket = _dataSource.GetBucket(key);
            var dt = (dateTime ?? DateTime.UtcNow).AddSeconds(-bucket.TimeSpan.TotalSeconds);
            var used = _dataSource.GetUsed(key).Where(u => u.DateTime >= dt).ToList();
            var dict = bucket.ReservedSlots.ToDictionary(s => s.Key, s => s.Value);
            foreach (var u in used)
            {
                if (dict.ContainsKey(u.App) && dict[u.App] > 0)
                {
                    dict[u.App]--;
                }
            }

            var reservedForOthers = dict.Where(d => d.Key != app).Sum(d => d.Value);
            var reservedForMe = dict.ContainsKey(app) ? dict[app] : 0;
            return new Tuple<int, int>(reservedForOthers, reservedForMe);
        }

        private void ClearLocksFrom(ThrottlerRequest request, ConcurrentDictionary<int, Reservation> reservations)
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
            ConcurrentDictionary<int, Reservation> reservations,
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
}
