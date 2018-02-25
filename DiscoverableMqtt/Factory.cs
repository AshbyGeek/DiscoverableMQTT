using System;
using System.Collections.Generic;
using System.Text;

namespace DiscoverableMqtt
{
    public interface IFactory
    {
        IMqttClientWrapper CreateMqttClientWrapper(string brokerHostName);
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
    }
}
