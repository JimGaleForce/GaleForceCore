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
            using (logger.Stage("id1"))
            {
                logger.Log("test-in-stage");
                using (logger.Step("id2"))
                {
                    logger.Log("test-in-step");
                }
                logger.Log("test-out-step");
            }

            Assert.AreEqual(7, logger.Collector.Items.Count);
        }
    }
}
