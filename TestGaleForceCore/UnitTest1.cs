using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    }
}
