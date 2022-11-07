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
            using (logger.Stage("id1", "outer"))
            {
                logger.Log("test-in-stage");
                using (logger.Step("id2", "inner"))
                {
                    logger.Log("test-in-step");
                    Thread.Sleep(20);
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
        }

        [TestMethod]
        public void TestLoggerNull()
        {
            StageLogger logger = null;
            var valid = false;
            using (logger?.Stage("id1"))
            {
                logger?.Log("test-in-stage");
                using (logger?.Step("id2"))
                {
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
