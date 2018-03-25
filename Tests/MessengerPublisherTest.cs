using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

using Moq;

namespace DiscoverableMqtt.Tests
{
    [TestClass, System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class MessengerPublisherTest
    {
        public const string SERV_ADDR = "localhost";
        public static readonly int CLIENT_ID = int.MinValue;


        public Fakes.FakeFactory moqFactory;

        public MessengerPublisher pub;

        [TestInitialize]
        public void TestInitialize()
        {
            moqFactory = new Fakes.FakeFactory(true);
            moqFactory.Client.Setup(x => x.IsConnected).Returns(true);

            pub = new MessengerPublisher(moqFactory.Client.Object, CLIENT_ID);
        }

        [DataTestMethod]
        [DataRow((QosLevel)0, false, "Testing/Gnitset/#")]
        [DataRow((QosLevel)1, true, "/#")]
        [DataRow((QosLevel)2, true, "/+/stuff")]
        public void MessengerPublisher_Publish_FlagsTest(QosLevel qosLevel, bool retain, string topic)
        {
            const string MSG = "Bogus Sugob";
            byte[] MSG_bytes = Encoding.UTF8.GetBytes(MSG);

            pub.QosLevel = qosLevel;
            pub.Retain = retain;
            pub.Topic = topic;

            pub.PublishWithoutHeader(MSG);

            // Publish is done asynchronously with a task, wait to make sure it completes
            System.Threading.Thread.Sleep(20);

            moqFactory.Client.Verify(x => x.Publish(topic, It.IsAny<byte[]>(), (byte)qosLevel, retain));
        }

        [TestMethod]
        public void MessengerPublisher_Publish_WithoutHeader()
        {
            const string MSG = "TESTING MEssage testING. 1234";
            byte[] MSG_bytes = Encoding.UTF8.GetBytes(MSG);

            pub.PublishWithoutHeader(MSG);

            // Publish is done asynchronously with a task, wait to make sure it completes
            System.Threading.Thread.Sleep(20);

            moqFactory.Client.Verify(x => x.Publish(It.IsAny<string>(), MSG_bytes, It.IsAny<byte>(), It.IsAny<bool>()));
        }

        [TestMethod]
        public void MessengerPublisher_Publish_WithHeader()
        {
            const string MSG = "TESTING MEssage testING. 1234";
            const int ID = 1;
            byte[] MSG_bytes = Encoding.UTF8.GetBytes(ID + " " + MSG);

            pub.Id = ID;

            pub.Publish(MSG);

            // Publish is done asynchronously with a task, wait to make sure it completes
            System.Threading.Thread.Sleep(20);

            moqFactory.Client.Verify(x => x.Publish(It.IsAny<string>(), MSG_bytes, It.IsAny<byte>(), It.IsAny<bool>()));
        }

        [TestMethod]
        public void MessengerPublisher_Dispose_FiresEvent()
        {
            bool eventFired = false;
            pub.Disposed += (s, e) => eventFired = true;

            pub.Dispose();

            Assert.IsTrue(eventFired);
        }
    }
}
