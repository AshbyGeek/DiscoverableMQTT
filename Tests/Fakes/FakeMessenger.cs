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
            Setup(x => x.Connect()).Raises(x => x.ConnectionStatusChanged += null, new GenericEventArgs<bool>(true));
            Setup(x => x.Disconnect()).Raises(x => x.ConnectionStatusChanged += null, new GenericEventArgs<bool>(false));
        }

        public Mock<IMessengerListener> Listener { get; set; }
        public Mock<IMessengerPublisher> Publisher { get; set; }
    }
}
