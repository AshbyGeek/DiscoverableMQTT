using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiscoverableMqtt.Tests
{
    [TestClass]
    public class FactoryTest
    {
        [DataTestMethod]
        [DataRow("192.168.1.1", "192.168.1.1", 1883)]
        [DataRow("localhost:12345", "localhost", 12345)]
        [DataRow("test.mosquitto.org", "test.mosquitto.org", 1883)]
        public void Factory_CreateMqttClientWrapper(string url, string baseUrl, int port)
        {
            var factory = new Factory();
            var client = factory.CreateMqttClientWrapper(url);

            Assert.AreEqual(port, client.Settings.Port);

            var hostnameField = (client as MqttClientWrapper).Client.GetType().GetField("brokerHostName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance);
            var hostName = hostnameField.GetValue((client as MqttClientWrapper).Client);
            Assert.AreEqual(baseUrl, hostName);
        }
    }
}
