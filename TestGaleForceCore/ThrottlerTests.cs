using System;
using GaleForceCore.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestGaleForceCore
{
    [TestClass]
    public class ThrottlerTests
    {
        private MemoryDataSource _memoryDataSource;
        private Throttler _throttler;

        [TestInitialize]
        public void Setup()
        {
            _memoryDataSource = new MemoryDataSource();
            _throttler = new Throttler(_memoryDataSource);
        }

        [TestMethod]
        public void TestThrottlerMaxQuota()
        {
            // Set up a bucket with a quota of 5 requests per minute
            string bucketKey = "gpt4model";
            int quota = 5;
            _memoryDataSource.AddBucket(bucketKey, quota, 0, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            var dt = DateTime.UtcNow;

            // Request 5 slots, which should be allowed as it's within the quota
            for (int i = 0; i < quota; i++)
            {
                var result = _throttler.RequestSlots(bucketKey, 1, 1, dt);
                Assert.AreEqual(ReservationStatus.Allowed, result.Status);
            }

            // Request a 6th slot, which should result in a waiting status as the quota is exceeded
            var waitingResult = _throttler.RequestSlots(bucketKey, 1, 1, dt);
            Assert.AreEqual(ReservationStatus.Waiting, waitingResult.Status);

            var dt2 = dt.AddMinutes(1.01);

            var waitingResult2 = _throttler.RequestSlots(bucketKey, 1, 1, dt2);
            Assert.AreEqual(ReservationStatus.Allowed, waitingResult2.Status);

            // Release a slot and check if the waiting request is now allowed
            _throttler.ReleaseReservation(bucketKey, waitingResult.ReservationId.Value);
            var updatedStatus = _throttler.CheckReservationStatus(bucketKey, waitingResult.ReservationId.Value);
            Assert.AreEqual(null, updatedStatus);
        }

        [TestMethod]
        public void TestThrottlerMaxQuotaBy2()
        {
            // Set up a bucket with a quota of 5 requests per minute
            string bucketKey = "gpt4model";
            int quota = 5;
            _memoryDataSource.AddBucket(bucketKey, quota, 0, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            var dt = DateTime.UtcNow;

            var result = _throttler.RequestSlots(bucketKey, 3, 1, dt);
            Assert.AreEqual(ReservationStatus.Allowed, result.Status);

            var result1 = _throttler.RequestSlots(bucketKey, 1, 1, dt);
            Assert.AreEqual(ReservationStatus.Allowed, result1.Status);

            var result2 = _throttler.RequestSlots(bucketKey, 2, 1, dt);
            Assert.AreEqual(ReservationStatus.Waiting, result2.Status);

            var stat2 = _throttler.CheckReservationStatus(bucketKey, result2.ReservationId.Value, dt);
            Assert.AreEqual(ReservationStatus.Waiting, stat2.Status);

            _throttler.ReleaseReservation(bucketKey, result1.ReservationId.Value);

            var stat3 = _throttler.CheckReservationStatus(bucketKey, result2.ReservationId.Value, dt);
            Assert.AreEqual(ReservationStatus.Allowed, stat3.Status);

            // fail
        }

        [TestMethod]
        public void TestThrottlerMaxQuotaWithPri()
        {
            // Set up a bucket with a quota of 5 requests per minute
            string bucketKey = "gpt4model";
            int quota = 5;
            _memoryDataSource.AddBucket(bucketKey, quota, 0, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            var dt = DateTime.UtcNow;

            var result0 = _throttler.RequestSlots(bucketKey, 4, 1, dt);
            Assert.AreEqual(ReservationStatus.Allowed, result0.Status);

            var resultX = _throttler.RequestSlots(bucketKey, 1, 1, dt);
            Assert.AreEqual(ReservationStatus.Allowed, resultX.Status);

            var result1 = _throttler.RequestSlots(bucketKey, 1, 1, dt);
            Assert.AreEqual(ReservationStatus.Waiting, result1.Status);

            var result2 = _throttler.RequestSlots(bucketKey, 1, 2, dt);
            Assert.AreEqual(ReservationStatus.Waiting, result2.Status);

            var stat2 = _throttler.CheckReservationStatus(bucketKey, result2.ReservationId.Value, dt);
            Assert.AreEqual(ReservationStatus.Waiting, stat2.Status);

            _throttler.ReleaseReservation(bucketKey, resultX.ReservationId.Value);

            var stat1 = _throttler.CheckReservationStatus(bucketKey, result1.ReservationId.Value, dt);
            Assert.AreEqual(ReservationStatus.Waiting, stat1.Status);

            stat2 = _throttler.CheckReservationStatus(bucketKey, result2.ReservationId.Value, dt);
            Assert.AreEqual(ReservationStatus.Allowed, stat2.Status);
        }
    }
}
