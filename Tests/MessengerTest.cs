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

        
        public Fakes.FakeFactory moqFactory;

        public Messenger messenger;

        [TestInitialize]
        public void TestInitialize()
        {
            moqFactory = new Fakes.FakeFactory(true);

            messenger = new Messenger()
            {
                Factory = moqFactory.Object, // This must come first
                Id = CLIENT_ID,
                ServerAddress = SERV_ADDR,
            };

            moqFactory.Client.Setup(x => x.IsConnected).Returns(true);
        }

        [TestMethod]
        public void Messenger_Connect_CallsConnect()
        {
            messenger.Connect();

            moqFactory.Verify(x => x.CreateMqttClientWrapper(SERV_ADDR));
            moqFactory.Client.Verify(x => x.Connect(CLIENT_ID.ToString()));
        }

        [DataTestMethod]
        [DataRow(5, "", false, false, false)]
        [DataRow(6, "", true, true, false)]
        [DataRow(2, "localhost", true, true, true)]
        [DataRow(4, "localhost", false, false, true)]
        public void Messenger_Id_ConnectDisconnectTests(int id, string serverAddress, bool isconnected, bool disconnects, bool connects)
        {
            moqFactory.Client.Setup(x => x.IsConnected).Returns(isconnected);
            messenger.ServerAddress = serverAddress;
            moqFactory.Client.ResetCalls();

            messenger.Id = id;

            if (disconnects)
            {
                moqFactory.Client.Verify(x => x.Disconnect());
            }
            if (connects)
            {
                moqFactory.Client.Verify(x => x.Connect(id.ToString()));
            }
        }

        [TestMethod]
        public void Messenger_GetPublisher_ValuesCorrect()
        {
            const string TOPIC = "TESTTOPIC";
            const int QOS_LEVEL = 1;

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
            const int QOS_LEVEL = 1;

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
    }
}
