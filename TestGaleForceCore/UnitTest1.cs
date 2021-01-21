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
    }
}
