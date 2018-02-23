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
            return new MqttClientWrapper(brokerHostName);
        }
    }
}
