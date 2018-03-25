using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

using Moq;

namespace DiscoverableMqtt.Tests
{
    [TestClass, System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class MessengerTests
    {
        public const string SERV_ADDR = "localhost";
        public static readonly int CLIENT_ID = int.MinValue;
        public static readonly Guid guid = Guid.NewGuid();

        
        public Fakes.FakeFactory moqFactory;

        public Messenger messenger;

        [TestInitialize]
        public void TestInitialize()
        {
            moqFactory = new Fakes.FakeFactory(true);
            moqFactory.Client.Setup(x => x.IsConnected).Returns(true);
            
            var appSettings = new AppSettings()
            {
                ApiId = CLIENT_ID,
                BrokerUrl = SERV_ADDR,
                Guid = guid,
            };
            messenger = new Messenger(appSettings, moqFactory.Object);
        }

        [TestMethod]
        public void Messenger_Connect_CallsConnect()
        {
            moqFactory.Client.Setup(x => x.IsConnected).Returns(false);
            messenger.Connect();

            //Connect actually runs in a separate thread, so wee need to sleep a minute to give it time to run
            System.Threading.Thread.Sleep(20);

            moqFactory.Verify(x => x.CreateMqttClientWrapper(SERV_ADDR));
            moqFactory.Client.Verify(x => x.Connect(guid.ToString()));
        }

        [TestMethod]
        public void Messenger_GetPublisher_ValuesCorrect()
        {
            const string TOPIC = "TESTTOPIC";
            const QosLevel QOS_LEVEL = QosLevel.AtLeastOnce;

            using (var publisher = messenger.GetPublisher(TOPIC, QOS_LEVEL))
            {
                moqFactory.Verify(x => x.CreateMessengerPublisher(moqFactory.Client.Object, CLIENT_ID));
                Assert.AreEqual(moqFactory.Publisher.Object, publisher);
                moqFactory.Publisher.VerifySet(x => x.QosLevel = QOS_LEVEL);
                moqFactory.Publisher.VerifySet(x => x.Topic = TOPIC);
            }
        }

        [TestMethod]
        public void Messenger_GetListener_ValuesCorrect()
        {
            const string TOPIC = "TESTTOPIC";
            const QosLevel QOS_LEVEL = QosLevel.AtLeastOnce;

            using (var listener = messenger.GetListener(TOPIC, QOS_LEVEL))
            {
                moqFactory.Verify(x => x.CreateMessengerListener(moqFactory.Client.Object));
                Assert.AreEqual(moqFactory.Listener.Object, listener);
                moqFactory.Listener.VerifySet(x => x.QosLevel = QOS_LEVEL);
                moqFactory.Listener.VerifySet(x => x.Topic = TOPIC);
            }
        }

        [TestMethod]
        public void Messenger_IsConnected_CatchesExceptions()
        {
            moqFactory.Client.Setup(x => x.IsConnected).Throws<Exception>();

            bool connected = messenger.IsConnected;

            Assert.IsFalse(connected);
        }

        [TestMethod]
        public void Messenger_Disconnect()
        {
            messenger.Disconnect();

            moqFactory.Client.Verify(x => x.Disconnect());
        }
    }
}
