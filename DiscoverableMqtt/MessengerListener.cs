using System;
using System.Collections.Generic;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using System.Text.RegularExpressions;

namespace DiscoverableMqtt
{
    public interface IMessengerListener : IDisposable
    {
        event MqttClient.MqttMsgPublishEventHandler MsgReceived;
        event EventHandler Disposed;

        IMqttClientWrapper Client { get; set; }

        bool IsSubscribed { get; }
        byte QosLevel { get; set; }
        string Topic { get; set; }
    }

    public class MsgReceivedEventArgs
    {
        public string Topic;
        public string Message;
    }

    public class MessengerListener : IMessengerListener
    {
        public event MqttClient.MqttMsgPublishEventHandler MsgReceived;
        public event EventHandler Disposed;

        public MessengerListener(IMqttClientWrapper client)
        {
            Client = client;
        }
        
        public IMqttClientWrapper Client
        {
            get => _Client;
            set
            {
                if (_Client != value)
                {
                    Unsubscribe();
                    _Client = value;
                    Subscribe();
                }
            }
        }
        private IMqttClientWrapper _Client;

        public byte QosLevel
        {
            get => _QosLevel;
            set
            {
                if (value != _QosLevel)
                {
                    Unsubscribe();
                    _QosLevel = value;
                    Subscribe();
                }
            }
        }
        private byte _QosLevel = 1;

        public string Topic
        {
            get => _Topic;
            set
            {
                if (value != _Topic)
                {
                    Unsubscribe();
                    _Topic = value;
                    _TopicRegex = GenerateRegexForTopic(value);
                    Subscribe();
                }
            }
        }
        private string _Topic = "";
        private Regex _TopicRegex;

        public bool IsSubscribed { get; private set; }

        public void Dispose()
        {
            Unsubscribe();
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        private void Unsubscribe()
        {
            if (IsSubscribed && !String.IsNullOrWhiteSpace(Topic))
            {
                Client.MqttMsgPublishReceived -= Client_MqttMsgPublishReceived;
                Client.Unsubscribe(new[] { Topic });
                IsSubscribed = false;
            }
        }

        private void Subscribe()
        {
            if (!IsSubscribed && !String.IsNullOrWhiteSpace(Topic))
            {
                Client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                Client.Subscribe(new[] { Topic }, new[] { QosLevel });
                IsSubscribed = true;
            }
        }

        private void Client_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            if (_TopicRegex.IsMatch(e.Topic))
            {
                MsgReceived?.Invoke(this, e);
            }
        }

        private static Regex GenerateRegexForTopic(string topic)
        {
            var pattern = "^" + topic.Replace(@"+", @"[^/]*").Replace("#", ".*") + "$";
            return new Regex(pattern);
        }
    }
}
