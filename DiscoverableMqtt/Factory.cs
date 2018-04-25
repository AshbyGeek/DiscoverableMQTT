using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscoverableMqtt.Probes;

namespace DiscoverableMqtt
{
    public interface IFactory
    {
        IMqttClientWrapper CreateMqttClientWrapper(string brokerHostName);
        IAbstractProbe CreateTempProbe(AppSettings settings);
        IAbstractProbe CreateSoilMoistureProbe(AppSettings settings);

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
        public IAbstractProbe CreateTempProbe(AppSettings settings)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LinuxTempProbe();
            }
            else
            {
                return new FakeNumericProbe(60.0f,80.0f);
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public IAbstractProbe CreateSoilMoistureProbe(AppSettings settings)
        {
            if (RuntimeInformation.IsOSPlatform((OSPlatform.Linux)))
            {
                return new LinuxSoilMoistureProbe();
            }
            else
            {
                return new FakeNumericProbe(0,1);
            }
        }
    }
}
