using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpBench.Tests
{
    [TestClass]
    public class HttpSettingsParserTest
    {

        [TestMethod]
        public void 空()
        {
            var instance = HttpSettingsParser.Parse();

            Assert.IsNull(instance);
        }

        [TestMethod]
        public void 空2()
        {
            var instance = HttpSettingsParser.Parse(new string[0]);

            Assert.IsNull(instance);
        }

        [TestMethod]
        public void 無効URLのみ()
        {
            var instance = HttpSettingsParser.Parse("localhost");

            Assert.IsFalse(instance.IsValid);
        }

        [TestMethod]
        public void 空URLのみ()
        {
            var instance = HttpSettingsParser.Parse("");

            Assert.IsFalse(instance.IsValid);
        }

        [TestMethod]
        public void 有効URLのみ()
        {
            var instance = HttpSettingsParser.Parse("http://localhost");
            var url = new Uri("http://localhost");

            Assert.AreEqual(instance.Url,url);
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void パラメータURL()
        {
            var instance = HttpSettingsParser.Parse("U","http://localhost");
            var url = new Uri("http://localhost");

            Assert.AreEqual(instance.Url, url);
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void ハイフン無しnとc()
        {
            dynamic instance = HttpSettingsParser.Parse("n",100,"c",100);

            Assert.IsNotNull(instance);
            Assert.AreEqual(instance.n, 100);
            Assert.AreEqual(instance.c, 100);
        }

        [TestMethod]
        public void ハイフン無し文字列値のnとc()
        {
            dynamic instance = HttpSettingsParser.Parse("n", "100", "c", "100");

            Assert.IsNotNull(instance);
            Assert.AreEqual(instance.n, 100);
            Assert.AreEqual(instance.c, 100);
        }

        [TestMethod]
        public void ハイフン有りnとc()
        {
            dynamic instance = HttpSettingsParser.Parse("-n", 100, "-c", 100);

            Assert.IsNotNull(instance);
            Assert.AreEqual(instance.n, 100);
            Assert.AreEqual(instance.c, 100);
        }

        [TestMethod]
        public void ハイフン有り文字列値のnとc()
        {
            dynamic instance = HttpSettingsParser.Parse("-n", "100", "-c", "100");

            Assert.IsNotNull(instance);
            Assert.AreEqual(instance.n, 100);
            Assert.AreEqual(instance.c, 100);
        }
    }
}
