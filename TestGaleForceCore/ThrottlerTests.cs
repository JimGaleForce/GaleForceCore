using System;
using System.Collections.Generic;
using System.Linq;
using GaleForceCore.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestGaleForceCore
{
    [TestClass]
    public class ThrottlerTests
    {
        private MemoryDataSource2 _memoryDataSource;
        private Throttler2 _throttler;

        [TestInitialize]
        public void Setup()
        {
            _memoryDataSource = new MemoryDataSource2();
            _throttler = new Throttler2(_memoryDataSource);
        }

        [TestMethod]
        public void TestThrottlerMaxQuota()
        {
            // Set up a bucket with a quota of 5 requests per minute
            string bucketKey = "gpt4model";
            int quota = 5;
            var bucket = new BucketInfo
            {
                Key = bucketKey,
                Name = bucketKey,
                TimeSpan = TimeSpan.FromSeconds(60),
                QuotaPerTimeSpan = quota
            };

            _memoryDataSource.AddBucket(bucketKey, bucket);

            var dt = DateTime.UtcNow;

            var req = new ThrottlerRequest
            {
                Key = bucketKey,
                RequestedSlots = 1,
                App = "app",
                Owner = "owner",
                Instance = "instance",
                MinReadySlots = 1,
                Priority = 5
            };

            var list = new List<ReservationResult>();

            // Request 5 slots, which should be allowed as it's within the quota
            for (int i = 0; i < quota; i++)
            {
                req.Owner = "owner" + i;
                var result = _throttler.RequestSlots(req, dt);
                list.Add(result);
                Assert.AreEqual(Status.Granted, result.Status);
            }

            req.Owner = "owner6";

            // Request a 6th slot, which should result in a waiting status as the quota is exceeded
            var waitingResult = _throttler.RequestSlots(req, dt.AddSeconds(10));
            Assert.AreEqual(Status.Awaiting, waitingResult.Status);

            req.Owner = "owner7";
            var waitingResult2 = _throttler.RequestSlots(req, dt.AddSeconds(20));
            Assert.AreEqual(Status.Awaiting, waitingResult2.Status);

            // var dt2 = dt.AddMinutes(1.01);
            var releaseResult = _throttler.ReleaseSlots(list.First());

            var ck1 = _throttler.CheckAndUse(waitingResult, dt.AddSeconds(40));
            var ck2 = _throttler.CheckAndUse(waitingResult2, dt.AddSeconds(40));
            Assert.AreEqual(Status.Awaiting, ck1.Status);
            Assert.AreEqual(Status.Awaiting, ck2.Status);

            releaseResult = _throttler.ReleaseSlots(ck1);

            ck1 = _throttler.CheckAndUse(waitingResult, dt.AddSeconds(61));
            ck2 = _throttler.CheckAndUse(waitingResult2, dt.AddSeconds(61));
            Assert.AreEqual(Status.Released, ck1.Status);
            Assert.AreEqual(Status.Granted, ck2.Status);
        }

        [TestMethod]
        public void TestThrottlerMaxQuotaBy2()
        {
            // Set up a bucket with a quota of 5 requests per minute
            string bucketKey = "gpt4model";
            int quota = 5;
            var bucket = new BucketInfo
            {
                Key = bucketKey,
                Name = bucketKey,
                TimeSpan = TimeSpan.FromSeconds(60),
                QuotaPerTimeSpan = quota
            };

            _memoryDataSource.AddBucket(bucketKey, bucket);

            var req = new ThrottlerRequest
            {
                Key = bucketKey,
                RequestedSlots = 1,
                App = "app",
                Owner = "owner",
                Instance = "instance",
                MinReadySlots = 1,
                Priority = 5
            };

            var dt = DateTime.UtcNow;

            req.RequestedSlots = 4;
            req.Owner = "owner0";
            var result = _throttler.RequestSlots(req, dt);
            Assert.AreEqual(Status.Granted, result.Status);

            req.RequestedSlots = 2;
            req.MinReadySlots = 2;
            req.Owner = "owner1";
            var result1 = _throttler.RequestSlots(req, dt);
            Assert.AreEqual(Status.Awaiting, result1.Status);

            req.RequestedSlots = 1;
            req.MinReadySlots = 1;
            req.Owner = "owner2";
            var result2 = _throttler.RequestSlots(req, dt);
            Assert.AreEqual(Status.Awaiting, result2.Status);

            _throttler.ReleaseSlots(result1);

            var stat3 = _throttler.CheckAndUse(result2, dt);
            Assert.AreEqual(Status.Granted, stat3.Status);
        }

        [TestMethod]
        public void TestThrottlerMaxQuotaWithPri()
        {
            // Set up a bucket with a quota of 5 requests per minute
            string bucketKey = "gpt4model";
            int quota = 5;
            var bucket = new BucketInfo
            {
                Key = bucketKey,
                Name = bucketKey,
                TimeSpan = TimeSpan.FromSeconds(60),
                QuotaPerTimeSpan = quota
            };

            _memoryDataSource.AddBucket(bucketKey, bucket);

            var dt = DateTime.UtcNow;

            var req = new ThrottlerRequest
            {
                Key = bucketKey,
                RequestedSlots = 1,
                App = "app",
                Owner = "owner",
                Instance = "instance",
                MinReadySlots = 1,
                Priority = 5
            };

            req.RequestedSlots = 1;
            req.Owner = "owner1";
            var result1 = _throttler.RequestSlots(req, dt);
            Assert.AreEqual(Status.Granted, result1.Status);

            req.RequestedSlots = 4;
            req.Owner = "owner0";
            var result = _throttler.RequestSlots(req, dt.AddSeconds(30));
            Assert.AreEqual(Status.Granted, result.Status);

            req.RequestedSlots = 1;
            req.Owner = "owner2";
            var result2 = _throttler.RequestSlots(req, dt.AddSeconds(30));
            Assert.AreEqual(Status.Awaiting, result2.Status);

            req.RequestedSlots = 1;
            req.Owner = "owner3-hipri";
            req.Priority = 4;
            var result3 = _throttler.RequestSlots(req, dt.AddSeconds(30));
            Assert.AreEqual(Status.Awaiting, result3.Status);

            var stat2 = _throttler.CheckAndUse(result2, dt.AddSeconds(61));
            Assert.AreEqual(Status.Awaiting, stat2.Status);

            var stat3 = _throttler.CheckAndUse(result3, dt.AddSeconds(61));
            Assert.AreEqual(Status.Granted, stat3.Status);
        }

        [TestMethod]
        public void TestThrottlerWithOneRequestPerAppOwnerInstance()
        {
            // Set up a bucket with a quota of 5 requests per minute
            string bucketKey = "gpt4model";
            int quota = 5;
            var bucket = new BucketInfo
            {
                Key = bucketKey,
                Name = bucketKey,
                TimeSpan = TimeSpan.FromSeconds(60),
                QuotaPerTimeSpan = quota
            };

            _memoryDataSource.AddBucket(bucketKey, bucket);

            var dt = DateTime.UtcNow;

            var req = new ThrottlerRequest
            {
                Key = bucketKey,
                RequestedSlots = 1,
                App = "app",
                Owner = "owner",
                Instance = "instance",
                MinReadySlots = 1,
                Priority = 5
            };

            req.RequestedSlots = 1;
            req.Owner = "owner1";
            var result1 = _throttler.RequestSlots(req, dt);
            Assert.AreEqual(Status.Granted, result1.Status);

            req.RequestedSlots = 4;
            req.Owner = "owner0";
            var result = _throttler.RequestSlots(req, dt.AddSeconds(30));
            Assert.AreEqual(Status.Granted, result.Status);

            req.RequestedSlots = 1;
            req.Owner = "owner2";
            var result2 = _throttler.RequestSlots(req, dt.AddSeconds(30));
            Assert.AreEqual(Status.Awaiting, result2.Status);
            result2 = _throttler.RequestSlots(req, dt.AddSeconds(30));
            Assert.AreEqual(Status.Awaiting, result2.Status);
            result2 = _throttler.RequestSlots(req, dt.AddSeconds(31));
            Assert.AreEqual(Status.Awaiting, result2.Status);
            result2 = _throttler.RequestSlots(req, dt.AddSeconds(32));
            Assert.AreEqual(Status.Awaiting, result2.Status);
            result2 = _throttler.RequestSlots(req, dt.AddSeconds(33));
            Assert.AreEqual(Status.Awaiting, result2.Status);
            result2 = _throttler.RequestSlots(req, dt.AddSeconds(33));
            Assert.AreEqual(Status.Awaiting, result2.Status);
            result2 = _throttler.RequestSlots(req, dt.AddSeconds(33));
            Assert.AreEqual(Status.Awaiting, result2.Status);
            result2 = _throttler.RequestSlots(req, dt.AddSeconds(33));
            Assert.AreEqual(Status.Awaiting, result2.Status);
            result2 = _throttler.RequestSlots(req, dt.AddSeconds(33));
            Assert.AreEqual(Status.Awaiting, result2.Status);
            result2 = _throttler.RequestSlots(req, dt.AddSeconds(33));
            Assert.AreEqual(Status.Awaiting, result2.Status);
            result2 = _throttler.RequestSlots(req, dt.AddSeconds(33));
            Assert.AreEqual(Status.Awaiting, result2.Status);

            req.RequestedSlots = 1;
            req.Owner = "owner3";
            var result3 = _throttler.RequestSlots(req, dt.AddSeconds(40));

            req.Owner = "owner2";
            result2 = _throttler.RequestSlots(req, dt.AddSeconds(50));
            Assert.AreEqual(Status.Awaiting, result2.Status);
            result2 = _throttler.RequestSlots(req, dt.AddSeconds(50));
            Assert.AreEqual(Status.Awaiting, result2.Status);
            result2 = _throttler.RequestSlots(req, dt.AddSeconds(50));
            Assert.AreEqual(Status.Awaiting, result2.Status);
            result2 = _throttler.RequestSlots(req, dt.AddSeconds(50));
            Assert.AreEqual(Status.Awaiting, result2.Status);
            result2 = _throttler.RequestSlots(req, dt.AddSeconds(50));
            Assert.AreEqual(Status.Awaiting, result2.Status);

            var stat2 = _throttler.CheckAndUse(result2, dt.AddSeconds(61));
            var stat3 = _throttler.CheckAndUse(result3, dt.AddSeconds(61));

            Assert.AreEqual(Status.Granted, stat3.Status);
            Assert.AreEqual(Status.Awaiting, stat2.Status);
        }
    }
}
