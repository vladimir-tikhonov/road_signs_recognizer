using System.Windows.Media;
using Lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibTest
{
    [TestClass]
    public class XyzColorTest
    {
        [TestMethod]
        public void TestRgbConvertion()
        {
            var source = Color.FromRgb(10, 15, 20);
            var result = new XyzColor(source);
            Assert.AreEqual(0.0058, result.X, 0.0001);
            Assert.AreEqual(0.0067, result.Y, 0.0001);
            Assert.AreEqual(0.0110, result.Z, 0.0001);
        }
    }
}
