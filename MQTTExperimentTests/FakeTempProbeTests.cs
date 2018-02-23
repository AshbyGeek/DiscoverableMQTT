using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace MQTTExperiment.Tests
{
    [TestClass]
    public class FakeTempProbeTests
    {
        private FakeTempProbe _probe;

        [TestInitialize]
        public void TestInitialize()
        {
            _probe = new FakeTempProbe();
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
            System.Threading.Thread.Sleep(30);
            _probe.Stop();

            Assert.IsTrue(triggered);
        }

        [TestMethod]
        public void FakeTempProbe_Interval()
        {
            const int GOAL_COUNT = 5;
            const int INTERVAL = 50;
            int triggerCount = 0;

            _probe.DataChanged += (s, e) => triggerCount += 1;
            _probe.MeasureInterval = INTERVAL;

            _probe.Start();
            int sleepTime = (INTERVAL * GOAL_COUNT) + INTERVAL / 2;   //time enough for N and a half intervals, so that the last interval has some time
            System.Threading.Thread.Sleep(sleepTime);
            _probe.Start();

            Assert.IsTrue(triggerCount == GOAL_COUNT);
        }
    }
}
