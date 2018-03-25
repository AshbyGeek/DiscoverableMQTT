using System;
using System.Collections.Generic;
using System.Text;
using Moq;

namespace DiscoverableMqtt.Tests.Fakes
{
    public class FakeMessenger : Mock<IMessenger>
    {
        public FakeMessenger(bool useDefaults)
        {
            if (useDefaults)
            {
                Listener = new Mock<IMessengerListener>();
                Publisher = new Mock<IMessengerPublisher>();
            }

            Setup(x => x.GetListener(It.IsAny<string>(), It.IsAny<QosLevel>()))
                .Returns((string topic, QosLevel qos) => Listener.Object);
            Setup(x => x.GetPublisher(It.IsAny<string>(), It.IsAny<QosLevel>()))
                .Returns((string topic, QosLevel qos) => Publisher.Object);
        }

        public Mock<IMessengerListener> Listener { get; set; }
        public Mock<IMessengerPublisher> Publisher { get; set; }
    }
}
