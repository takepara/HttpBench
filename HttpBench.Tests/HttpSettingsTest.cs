using System;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpBench.Tests
{
    [TestClass]
    public class HttpSettingsTest
    {
        [TestMethod]
        public void インスタンス生成()
        {
            dynamic instance = new HttpSettings();

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void ダメインスタンス()
        {
            var instance = new HttpSettings();

            Assert.IsNotNull(instance);
            Assert.IsFalse(instance.IsValid);
        }

        [TestMethod]
        public void プロパティU()
        {
            dynamic instance = new HttpSettings();
            var url = new Uri("http://localhost");
            instance.U = url;

            Assert.IsNotNull(instance);
            Assert.AreEqual(instance.U, url);
        }

        [TestMethod]
        public void プロパティStringU()
        {
            dynamic instance = new HttpSettings();
            var url = new Uri("http://localhost");
            instance.U = "http://localhost";

            Assert.IsNotNull(instance);
            Assert.AreEqual(instance.U, url);
        }

        [TestMethod]
        public void プロパティn()
        {
            dynamic instance = new HttpSettings();
            instance.n = 100;

            Assert.IsNotNull(instance);
            Assert.AreEqual(instance.n, 100);
        }

        [TestMethod]
        public void プロパティc()
        {
            dynamic instance = new HttpSettings();
            instance.c = 100;

            Assert.IsNotNull(instance);
            Assert.AreEqual(instance.c, 100);
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void 存在しないプロパティX()
        {
            dynamic instance = new HttpSettings();
            instance.X = 100;
        }
    }
}
