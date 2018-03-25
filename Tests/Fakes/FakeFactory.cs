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
                Messenger = new FakeMessenger(false);
                Messenger.Listener = new Mock<IMessengerListener>();
                Messenger.Publisher = new Mock<IMessengerPublisher>();
                Client = new Mock<IMqttClientWrapper>();
                Probe = new Mock<DiscoverableMqtt.Probes.IAbstractTempProbe>();
            }

            Setup(x => x.CreateMessenger(It.IsAny<AppSettings>()))
                .Returns((AppSettings settings) => Messenger.Object);
            Setup(x => x.CreateMessengerListener(It.IsAny<IMqttClientWrapper>()))
                .Returns((IMqttClientWrapper client) => Listener.Object);
            Setup(x => x.CreateMessengerPublisher(It.IsAny<IMqttClientWrapper>(), It.IsAny<int>()))
                .Returns((IMqttClientWrapper client, int id) => Publisher.Object);
            Setup(x => x.CreateMqttClientWrapper(It.IsAny<string>()))
                .Returns((string str) => Client.Object);
            Setup(x => x.CreateTempProbe(It.IsAny<AppSettings>()))
                .Returns((AppSettings settings) => Probe.Object);

            Listener.Setup(x => x.Dispose()).Raises(x => x.Disposed += null, EventArgs.Empty);
            Publisher.Setup(x => x.Dispose()).Raises(x => x.Disposed += null, EventArgs.Empty);
        }

        public FakeMessenger Messenger { get; set; }
        public Mock<IMessengerListener> Listener => Messenger.Listener;
        public Mock<IMessengerPublisher> Publisher => Messenger.Publisher;
        public Mock<IMqttClientWrapper> Client { get; set; }
        public Mock<DiscoverableMqtt.Probes.IAbstractTempProbe> Probe { get; set; }
    }
}
