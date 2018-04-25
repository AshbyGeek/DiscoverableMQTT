using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiscoverableMqtt.Probes;

namespace DiscoverableMqtt.Tests.Probes
{
    [TestClass, System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class LinuxSoilMoistureProbeTest
    {
        [TestMethod]
        public void test()
        {
            var probe = new LinuxSoilMoistureProbe();
            var value = probe.GetNewVal();
            Assert.AreEqual(.75, value);
        }
    }
}
