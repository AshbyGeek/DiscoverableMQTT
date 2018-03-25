using System;
using System.Collections.Generic;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using System.Text.RegularExpressions;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace DiscoverableMqtt
{
    public interface IMessengerListener : IDisposable
    {
        event EventHandler<MsgReceivedEventArgs> MsgReceived;
        event EventHandler Disposed;

        IMqttClientWrapper Client { get; set; }

        bool IsSubscribed { get; }
        QosLevel QosLevel { get; set; }
        string Topic { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class MsgReceivedEventArgs : EventArgs
    {
        public string Topic;
        public string Message;

        public bool DupFlag { get; set; }
        public QosLevel QosLevel { get; set; }
        public bool Retain { get; set; }

        public static implicit operator MqttMsgPublishEventArgs(MsgReceivedEventArgs args)
        {
            return new MqttMsgPublishEventArgs(args.Topic,
                                               Encoding.UTF8.GetBytes(args.Message),
                                               args.DupFlag,
                                               (byte)args.QosLevel,
                                               args.Retain);
        }

        public static implicit operator MsgReceivedEventArgs(MqttMsgPublishEventArgs args)
        {
            return new MsgReceivedEventArgs()
            {
                Topic = args.Topic,
                Message = Encoding.UTF8.GetString(args.Message),
                DupFlag = args.DupFlag,
                QosLevel = (QosLevel)args.QosLevel,
                Retain = args.Retain
            };
        }
    }

    public class MessengerListener : IMessengerListener
    {
        public event EventHandler<MsgReceivedEventArgs> MsgReceived;
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

        public QosLevel QosLevel
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
        private QosLevel _QosLevel = QosLevel.AtLeastOnce;

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
                Client.Subscribe(new[] { Topic }, new byte[] { (byte)QosLevel });
                IsSubscribed = true;
            }
        }

        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            if (_TopicRegex.IsMatch(e.Topic))
            {
                MsgReceived?.Invoke(this, (MsgReceivedEventArgs)e);
            }
        }

        private static Regex GenerateRegexForTopic(string topic)
        {
            var pattern = "^" + topic.Replace(@"+", @"[^/]*").Replace("#", ".*") + "$";
            return new Regex(pattern);
        }
    }
}
