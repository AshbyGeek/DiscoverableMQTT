using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using DiscoverableMqtt.Probes;

namespace DiscoverableMqtt.Tests.Probes
{
    [TestClass, System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class FakeTempProbeTest
    {
        private FakeNumericProbe _probe;

        [TestInitialize]
        public void TestInitialize()
        {
            _probe = new FakeNumericProbe(60.0f, 80.0f);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _probe?.Stop();
        }

        [TestMethod]
        public void FakeTempProbe_DataChanged_Fires()
        {
            bool triggered = false;

            _probe.DataChanged += (s, e) => triggered = true;
            _probe.MeasureInterval = 10;

            _probe.Start();
            System.Threading.Thread.Sleep(100);
            _probe.Stop();

            Assert.IsTrue(triggered);
        }

        [TestMethod]
        public void FakeTempProbe_Interval()
        {
            const int GOAL_COUNT = 10;
            const int INTERVAL = 75;
            int triggerCount = 0;

            _probe.DataChanged += (s, e) => triggerCount += 1;
            _probe.MeasureInterval = INTERVAL;

            _probe.Start();
            int sleepTime = (INTERVAL * GOAL_COUNT);
            System.Threading.Thread.Sleep(sleepTime);
            _probe.Start();

            //Fuzzy match because of processing times and things
            Assert.IsTrue(triggerCount >= GOAL_COUNT-1 && triggerCount <= GOAL_COUNT + 1);
        }
    }
}
