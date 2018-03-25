using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace DiscoverableMqtt.Tests
{
    [TestClass, System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class MessengerListenerTest
    {
        public const string SERV_ADDR = "localhost";
        public static readonly int CLIENT_ID = int.MinValue;


        public Fakes.FakeFactory moqFactory;

        public MessengerListener Listener;

        [TestInitialize]
        public void TestInitialize()
        {
            moqFactory = new Fakes.FakeFactory(true);
            moqFactory.Client.Setup(x => x.IsConnected).Returns(true);

            Listener = new MessengerListener(moqFactory.Client.Object);
        }

        [TestMethod]
        public void MessengerListener_SubscribeUnsubscribe()
        {
            const QosLevel QOS = QosLevel.ExactlyOnce;
            const string TOPIC = "Test/+/blah";
            Listener.Topic = TOPIC;
            Listener.QosLevel = QOS;

            moqFactory.Client.Verify(x => x.Subscribe(new[] { TOPIC }, new byte[] { (byte)QOS }));

            Listener.Topic = "";
            moqFactory.Client.Verify(x => x.Unsubscribe(new[] { TOPIC }));
        }

        [DataTestMethod]
        [DataRow("rooms/#", "rooms/stuff", true)]
        [DataRow("rooms/#", "rooms/stuff/bogus", true)]
        [DataRow("rooms/#", "rooms/stuff2", true)]
        [DataRow("rooms/#", "rooms/stuff2/bogus", true)]
        [DataRow("rooms/#", "rooms/notstuff", true)]
        [DataRow("rooms/#", "config/notstuff", false)]
        [DataRow("rooms/#", "config", false)]
        [DataRow("rooms/#", "rooms", false)]
        [DataRow("+/testing", "rooms/testing", true)]
        [DataRow("+/testing", "config/testing", true)]
        [DataRow("+/testing", "stuff/testing", true)]
        [DataRow("+/testing", "rooms/testing/not", false)]
        [DataRow("+/testing", "config/testing/not", false)]
        [DataRow("+/testing", "rooms/testing/not", false)]
        [DataRow("+/testing", "rooms/notstuff", false)]
        [DataRow("+/testing", "config/notstuff", false)]
        public void MessengerListener_TopicsFiltered(string topicFilter, string publishedTopic, bool received)
        {
            bool eventReceived = false;
            Listener.MsgReceived += (s, e) => eventReceived = true;
            Listener.Topic = topicFilter;

            Assert.IsTrue(Listener.IsSubscribed);

            var args = new MqttMsgPublishEventArgs(publishedTopic, new byte[] { 20, 20 }, false, 1, false);

            moqFactory.Client.Raise(x => x.MqttMsgPublishReceived += null, args);
            Assert.AreEqual(received, eventReceived);
        }

        [TestMethod]
        public void MesengerListener_Dispose_EvokesEvent()
        {
            bool eventReceived = false;
            Listener.Disposed += (s, e) => eventReceived = true;

            Listener.Dispose();

            Assert.IsTrue(eventReceived);
        }
    }
}
