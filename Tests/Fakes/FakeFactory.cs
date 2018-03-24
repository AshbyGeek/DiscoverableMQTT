using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscoverableMqtt.Tests.Fakes
{
    public class FakeFactory : Mock<IFactory>
    {
        public FakeFactory(bool useDefaults)
        {
            if (useDefaults)
            {
                Listener = new Mock<IMessengerListener>();
                Publisher = new Mock<IMessengerPublisher>();
                Client = new Mock<IMqttClientWrapper>();
            }

            Setup(x => x.CreateMqttClientWrapper(It.IsAny<string>()))
                .Returns((string str) => Client.Object);
            Setup(x => x.CreateMessengerListener(It.IsAny<IMqttClientWrapper>()))
                .Returns((IMqttClientWrapper client) => Listener.Object);
            Setup(x => x.CreateMessengerPublisher(It.IsAny<IMqttClientWrapper>(), It.IsAny<int>()))
                .Returns((IMqttClientWrapper client, int id) => Publisher.Object);

            Listener.Setup(x => x.Dispose()).Raises(x => x.Disposed += null, EventArgs.Empty);
            Publisher.Setup(x => x.Dispose()).Raises(x => x.Disposed += null, EventArgs.Empty);
        }

        public Mock<IMessengerListener> Listener { get; set; }
        public Mock<IMessengerPublisher> Publisher { get; set; }
        public Mock<IMqttClientWrapper> Client { get; set; }
    }
}
