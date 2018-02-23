using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

using Moq;

namespace MQTTExperiment.Tests
{
    [TestClass]
    public class MessengerTests
    {
        public const string SERV_ADDR = "localhost";
        public const string CLIENT_ID = "TEST_TEST_TEST";

        
        public Mock<IFactory> moqFactory;
        public Mock<IMqttClientWrapper> moqClientWrapper;

        public Messenger messenger;

        [TestInitialize]
        public void TestInitialize()
        {
            moqFactory = new Mock<IFactory>();
            moqClientWrapper = new Mock<IMqttClientWrapper>();
            messenger = new Messenger(SERV_ADDR, CLIENT_ID);
            messenger.Factory = moqFactory.Object;

            moqFactory.Setup(x => x.CreateMqttClientWrapper(It.IsAny<string>()))
                .Returns(moqClientWrapper.Object);
        }

        [TestMethod]
        public void Messenger_Connect_CallsConnect()
        {
            messenger.Connect();

            moqFactory.Verify(x => x.CreateMqttClientWrapper(SERV_ADDR));
            moqClientWrapper.Verify(x => x.Connect(CLIENT_ID));
        }

        [TestMethod]
        public void Messenger_GetPublisher_ValuesCorrect()
        {
            const string TOPIC = "TESTTOPIC";
            const int QOS_LEVEL = 1;

            var publisher = messenger.GetPublisher(TOPIC, QOS_LEVEL);

            Assert.AreEqual(TOPIC, publisher.Topic);
            Assert.AreEqual(QOS_LEVEL, publisher.QosLevel);
        }

        [TestMethod]
        public void MessengerPublisher_Publish_WritesString()
        {
            const string TOPIC = "TESTTOPIC";
            const string MSG = "TESTING MEssage testING. 1234";
            const int QOS_LEVEL = 1;
            byte[] msgEncoded = Encoding.UTF8.GetBytes(MSG);

            messenger.Connect();
            var publisher = messenger.GetPublisher(TOPIC, QOS_LEVEL);
            publisher.Publish(MSG);
            publisher.Publish(msgEncoded);

            moqClientWrapper.Verify(x => x.Publish(TOPIC, msgEncoded, QOS_LEVEL, false));
        }
    }
}
