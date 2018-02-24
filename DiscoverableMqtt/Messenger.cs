using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using uPLibrary.Networking.M2Mqtt;

namespace DiscoverableMqtt
{
    public class Messenger
    {
        public class MessengerPublisher
        {
            internal Messenger _Messenger { get; set; }
            public byte QosLevel { get; set; } = 1;
            public bool Retain { get; set; } = false;
            public string Topic { get; set; }
    
            public void Publish(string content)
            {
                if (_Messenger.IsConnected)
                {
                    content = MakePacketHeader() + content;
                    var bytes = Encoding.UTF8.GetBytes(content);
                    _Messenger._Client.Publish(Topic, bytes, QosLevel, Retain);
                }
            }

            public void Publish(byte[] content)
            {
                if (_Messenger.IsConnected)
                {
                    var header = MakePacketHeader();
                    var bytes = Encoding.UTF8.GetBytes(header).Concat(content).ToArray();
                    _Messenger._Client.Publish(Topic, bytes, QosLevel, Retain);
                }
            }

            private string MakePacketHeader()
            {
                return $"{_Messenger.Id} {DateTime.Now:yyMMddThhmmss} ";
            }
        }

        public IFactory Factory { get; set; } = new Factory();

        public string Id
        {
            get => _Id;
            set
            {
                if (_Id != value)
                {
                    Disconnect();
                    _Id = value;
                    Connect();
                }
            }
        }
        private string _Id = "";

        public string ServerAddress
        {
            get => _ServerAddress;
            set
            {
                if (_ServerAddress != value)
                {
                    Disconnect();
                    _ServerAddress = value;
                    Connect();
                }
            }
        }
        private string _ServerAddress = "";

        public bool IsConnected => _Client?.IsConnected ?? false;


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
                _Messenger = this,
                Topic = topic,
                QosLevel = qosLevel,
            };
        }

        public void Connect()
        {
            if (!String.IsNullOrEmpty(ServerAddress) && !string.IsNullOrEmpty(Id))
            {
                _Client = Factory.CreateMqttClientWrapper(ServerAddress);
                //_Client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                //_Client.MqttMsgSubscribed += Client_MqttMsgSubscribed;
                //_Client.MqttMsgUnsubscribed += Client_MqttMsgUnsubscribed;

                _Client.Connect(Id);
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                _Client.Disconnect();
            }
            //_Client.MqttMsgPublishReceived -= Client_MqttMsgPublishReceived;
            //_Client.MqttMsgSubscribed -= Client_MqttMsgSubscribed;
            //_Client.MqttMsgUnsubscribed -= Client_MqttMsgUnsubscribed;
        }
    }
}
