using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using uPLibrary.Networking.M2Mqtt;

namespace DiscoverableMqtt
{
    public class Messenger
    {
        public class MessengerPublisher
        {
            internal IMqttClientWrapper Client { get; set; }
            public byte QosLevel { get; internal set; } = 1;
            public bool Retain { get; internal set; } = false;
            public string Topic { get; internal set; }
    
            public void Publish(string content)
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                Client.Publish(Topic, bytes, QosLevel, Retain);
            }

            public void Publish(byte[] content)
            {
                Client.Publish(Topic, content, QosLevel, Retain);
            }
        }

        public IFactory Factory { get; set; } = new Factory();

        public string Id { get; }
        public string ServerAddress { get; }

        private IMqttClientWrapper _Client { get; set; }

        public Messenger(string serverAddress, string id)
        {
            Id = id;
            ServerAddress = serverAddress;
        }

        public MessengerPublisher GetPublisher(string topic, byte qosLevel = 2)
        {
            return new MessengerPublisher()
            {
                Client = _Client,
                Topic = topic,
                QosLevel = qosLevel,
            };
        }

        public void Connect()
        {
            _Client = Factory.CreateMqttClientWrapper(ServerAddress);
            _Client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            _Client.MqttMsgSubscribed += Client_MqttMsgSubscribed;
            _Client.MqttMsgUnsubscribed += Client_MqttMsgUnsubscribed;

            //TODO: Get Client ID from Helen's interface
            //For now, just generate a guid

            _Client.Connect(Id);
        }

        public void Disconnect()
        {
            _Client.Disconnect();
        }

        private void Client_MqttMsgUnsubscribed(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgUnsubscribedEventArgs e)
        {
            Debug.WriteLine($"Subscribed: {e.MessageId}");
        }

        private void Client_MqttMsgSubscribed(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgSubscribedEventArgs e)
        {
            Debug.WriteLine($"Unsubscribed: {e.MessageId}");
        }

        private void Client_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            var msg = System.Text.Encoding.UTF8.GetString(e.Message);
            Debug.WriteLine($"{e.Topic}: {msg}");
        }
    }
}
