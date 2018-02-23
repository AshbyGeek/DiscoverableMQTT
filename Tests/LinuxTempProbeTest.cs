using DiscoverableMqtt.Probes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoverableMqtt.Tests
{
    [TestClass]
    public class LinuxTempProbeTest
    {
        const string FAKE_DEV1_NAME = "28_12345";
        const string FAKE_DEV2_NAME = "29_45678";
        const string FAKE_DEV3_NAME = "30_09877";

        string tempPath = "";

        public LinuxTempProbe probe = null;

        [TestInitialize]
        public void TestInit()
        {
            probe = new LinuxTempProbe();

            // Create a fake linux 1 wire device folder structure
            tempPath = Path.Combine(Path.GetTempPath(), nameof(DiscoverableMqtt) + "_" + nameof(LinuxTempProbeTest));
            var path28 = Path.Combine(tempPath, FAKE_DEV1_NAME);
            var path29 = Path.Combine(tempPath, FAKE_DEV2_NAME);
            var path30 = Path.Combine(tempPath, FAKE_DEV3_NAME);
            var pathMaster = Path.Combine(tempPath, "w1_bus_master");
            
            Directory.CreateDirectory(path28);
            Directory.CreateDirectory(path29);
            Directory.CreateDirectory(path30);
            Directory.CreateDirectory(pathMaster);

            File.WriteAllText(Path.Combine(path28, "w1_slave"), Resources.FakeTempData1);
            File.WriteAllText(Path.Combine(path29, "w1_slave"), Resources.FakeTempData2);

            probe.OneWireDevicesPath = tempPath;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            probe.Stop();
            Directory.Delete(tempPath, true);
        }

        [TestMethod]
        public void LinuxTempProbe_DefaultDevicesPath()
        {
            var defaultProbe = new LinuxTempProbe();

            //Compare without any final slashes on the path, since they wouldn't matter anyway
            var trimChars = @"/\".ToCharArray();
            Assert.AreEqual("/sys/bus/w1/devices".TrimEnd(trimChars), defaultProbe.OneWireDevicesPath.TrimEnd(trimChars));
        }

        [TestMethod]
        public void LinuxTempProbe_GetOneWireDeviceNames()
        {
            var names = probe.GetOneWireDeviceNames();

            Assert.AreEqual(2, names.Count());
            Assert.IsTrue(names.Contains(FAKE_DEV1_NAME));
            Assert.IsTrue(names.Contains(FAKE_DEV2_NAME));

            // This one shouldn't show up since it doesn't have the requisite file
            Assert.IsFalse(names.Contains(FAKE_DEV3_NAME));
        }

        [TestMethod]
        public void LinuxTempProbe_RunTest()
        {
            probe.OneWireDeviceName = FAKE_DEV1_NAME;
            probe.MeasureInterval = 50;

            probe.Start();
            Task.Delay(75).Wait();
            probe.Stop();

            var probeVal = probe.GetCurrentData();
            Assert.IsTrue(Math.Abs(80.7166f - probeVal) < 0.0075);
        }
    }
}
