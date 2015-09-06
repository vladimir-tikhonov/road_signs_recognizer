using System.Windows.Media;
using Lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibTest
{
    [TestClass]
    public class LabColorTest
    {
        [TestMethod]
        public void TestXyzConvertion()
        {
            var source = new XyzColor(Color.FromRgb(10, 15, 20));
            var result = new LabColor(source);
            Assert.AreEqual(6.0901, result.L, 0.0001);
            Assert.AreEqual(-2.3789, result.A, 0.0001);
            Assert.AreEqual(-5.1494, result.B, 0.0001);
        }
    }
}
