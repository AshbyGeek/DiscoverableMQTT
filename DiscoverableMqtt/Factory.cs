using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DiscoverableMqtt
{
    public interface IFactory
    {
        IMqttClientWrapper CreateMqttClientWrapper(string brokerHostName);
        Probes.IAbstractTempProbe CreateTempProbe(AppSettings settings);

        IMessenger CreateMessenger(AppSettings settings);
        IMessengerListener CreateMessengerListener(IMqttClientWrapper client);
        IMessengerPublisher CreateMessengerPublisher(IMqttClientWrapper client, int id);
    }

    public class Factory : IFactory
    {
        public IMqttClientWrapper CreateMqttClientWrapper(string brokerHostName)
        {
            if (!brokerHostName.Contains(@"//"))
            {
                brokerHostName = "mqtt://" + brokerHostName;
            }
            var uri = new Uri(brokerHostName);
            if (uri.IsDefaultPort)
            {
                return new MqttClientWrapper(uri.Host);
            }
            else
            {
                return new MqttClientWrapper(uri.Host, uri.Port, false, null, null, uPLibrary.Networking.M2Mqtt.MqttSslProtocols.None);
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public IMessenger CreateMessenger(AppSettings settings)
        {
            return new Messenger(settings, this);
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public IMessengerListener CreateMessengerListener(IMqttClientWrapper client)
        {
            return new MessengerListener(client);
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public IMessengerPublisher CreateMessengerPublisher(IMqttClientWrapper client, int id)
        {
            return new MessengerPublisher(client, id);
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public Probes.IAbstractTempProbe CreateTempProbe(AppSettings settings)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new Probes.LinuxTempProbe();
            }
            else
            {
                return new Probes.FakeTempProbe();
            }
        }
    }
}
