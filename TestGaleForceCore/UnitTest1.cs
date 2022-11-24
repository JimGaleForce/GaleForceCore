using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GaleForceCore.Helpers;
using GaleForceCore.Logger;
using GaleForceCore.Writers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestGaleForceCore
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestTemplateWriter()
        {
            var writer = new TemplateWriter("one {two} three {four} five {six} seven");
            writer.Add("two", "too");
            writer.Add("two", "2");
            var result = writer.ToString();
            Assert.AreEqual("one 2 three  five  seven", result);
        }

        [TestMethod]
        public void TestTemplateWriterEscapeChars()
        {
            var writer = new TemplateWriter("one {two} three {{{four}}} five {six} seven");
            writer.Add("two", "too");
            writer.Add("two", "2");
            var result = writer.ToString();
            Assert.AreEqual("one 2 three {} five  seven", result);
        }

        [TestMethod]
        public void TestLogger()
        {
            var logger = new StageLogger().AddCollector();
            using (var stagelog = logger.Stage("id1", "outer"))
            {
                logger.Log("test-in-stage");
                using (var steplog = logger.Step("id2", "inner"))
                {
                    logger.Log("test-in-step");
                    Thread.Sleep(20);
                    stagelog.AddMetric("key", 1);
                }
                logger.Log("test-out-step");
                Thread.Sleep(5);
            }

            Assert.AreEqual(7, logger.Collector.Items.Count);

            var timing2 = logger.Collector.GetDuration("id2");
            var timing1 = logger.Collector.GetDuration("id1");
            Assert.IsTrue(timing1 > timing2);
            Assert.IsTrue(timing2 >= 20);
            Assert.IsTrue(timing1 >= 25);
            var item = logger.Collector.Items.First(l => l.ChangeItem != null && l.ChangeItem.Id == "id1").ChangeItem;
            Assert.AreEqual(1, item.Metrics.Count);
            Assert.AreEqual(1, item.Metrics["key"]);
        }

        [TestMethod]
        public void TestLoggerOrdering()
        {
            var logger = new StageLogger().AddCollector();
            var items = new List<StageSectionUpdate>();
            logger.StageChangeCallback = c =>
            {
                items.Add(c);
                return 1;
            };

            using (var stagelog = logger.Stage("id1", "outer"))
            {
                stagelog.AddEvent("event1", "value1");
                logger.Log("test-in-stage");
                using (var steplog = logger.Step("id2", "inner"))
                {
                    logger.Log("test-in-step");
                    Thread.Sleep(20);
                    stagelog.AddMetric("key", 1);
                }

                logger.Log("test-out-step");
            }

            var test = logger.Collector.ToLogString();

            var item = logger.Collector.Items.First(l => l.ChangeItem != null && l.ChangeItem.Id == "id1").ChangeItem;
        }

        [TestMethod]
        public void TestLoggerNull()
        {
            StageLogger logger = null;
            var valid = false;
            using (var stagelog = logger?.Stage("id1"))
            {
                logger?.Log("test-in-stage");
                using (var itemlog = logger?.Step("id2"))
                {
                    itemlog?.AddMetric("key", 1);
                    logger?.Log("test-in-step");
                    valid = true;
                }
                logger?.Log("test-out-step");
            }

            Assert.AreEqual(null, logger);
            Assert.AreEqual(true, valid);
        }

        [TestMethod]
        public void TestShallowCopy()
        {
            var source = new Object1
            {
                SameId = 1,
                SameIdNull = 1,
                SameString = "String1",
                SameBool = true,
                SameDateTime = DateTime.Now,
                SameSubObject = new SubObject { Name = "SubObject" },
                DiffId1 = 2,
                DiffId1Null = 2,
                DiffString1 = "String2",
                DiffBool1 = true,
                DiffDateTime1 = DateTime.Now
            };

            var target = source.CopyShallow<Object2>();

            Assert.AreEqual(target.SameId, source.SameId);
            Assert.AreEqual(target.SameBool, source.SameBool);
            Assert.AreEqual(target.SameDateTime, source.SameDateTime);
            Assert.AreEqual(target.SameSubObject, source.SameSubObject);

            Assert.AreNotEqual(target.DiffId2, source.DiffId1);
        }
    }

    public class Object1
    {
        public int SameId { get; set; }

        public int DiffId1 { get; set; }

        public int? SameIdNull { get; set; }

        public int? DiffId1Null { get; set; }

        public string SameString { get; set; }

        public string DiffString1 { get; set; }

        public bool SameBool { get; set; }

        public bool DiffBool1 { get; set; }

        public bool? SameBoolNull { get; set; }

        public bool? DiffBool1Null { get; set; }

        public DateTime SameDateTime { get; set; }

        public DateTime DiffDateTime1 { get; set; }

        public DateTime? SameDateTimeNull { get; set; }

        public DateTime? DiffDateTime1Null { get; set; }

        public SubObject SameSubObject { get; set; }
    }

    public class Object2
    {
        public int SameId { get; set; }

        public int DiffId2 { get; set; }

        public int? SameIdNull { get; set; }

        public int? DiffId2Null { get; set; }

        public string SameString { get; set; }

        public string DiffString2 { get; set; }

        public bool SameBool { get; set; }

        public bool DiffBool2 { get; set; }

        public bool? SameBoolNull { get; set; }

        public bool? DiffBool2Null { get; set; }

        public DateTime SameDateTime { get; set; }

        public DateTime DiffDateTime2 { get; set; }

        public DateTime? SameDateTimeNull { get; set; }

        public DateTime? DiffDateTime2Null { get; set; }

        public SubObject SameSubObject { get; set; }
    }

    public class SubObject
    {
        public string Name { get; set; }
    }
}
