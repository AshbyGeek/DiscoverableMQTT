using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoverableMqtt
{
    public class Messenger
    {
        private class MessengerPublisher : IMessengerPublisher
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
                    Task.Run(() =>
                    {
                        try
                        {
                            _Messenger._Client.Publish(Topic, bytes, QosLevel, Retain);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to publish data: " + ex.Message);
                        }
                    });
                }
            }

            public void Publish(byte[] content)
            {
                if (_Messenger.IsConnected)
                {
                    var header = MakePacketHeader();
                    var bytes = Encoding.UTF8.GetBytes(header).Concat(content).ToArray();
                    Task.Run(() =>
                    {
                        try
                        {
                            _Messenger._Client.Publish(Topic, bytes, QosLevel, Retain);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to publish data: " + ex.Message);
                        }
                    });
                }
            }

            private string MakePacketHeader()
            {
                return $"{_Messenger.Id} {DateTime.Now:yyMMddThhmmss} ";
            }
        }

        private class MessengerListener
        {
            internal Messenger _Messenger { get; set; }
            public string Topic
            {
                get => _Topic;
                set
                {

                }
            }
            private string _Topic = "";

        }

        internal object MessengerLock = new object();
        
        /// <summary>
        /// A factory to be used for producing needed components, like an MqttClientWrapper.
        /// Probably only needed for unit tests.
        /// </summary>
        public IFactory Factory { get; set; } = new Factory();

        public Guid Id
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
        private Guid _Id = Guid.NewGuid();

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

        public bool IsConnected
        {
            get
            {
                try
                {
                    return _Client?.IsConnected ?? false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while querying Mqtt about connection status : {ex.Message}");
                    return false;
                }
            }
        }


        private IMqttClientWrapper _Client { get; set; }

        #region  Constructors
        public Messenger() { }

        public Messenger(string serverAddress, Guid id)
        {
            Id = id;
            ServerAddress = serverAddress;
        }
        #endregion

        public IMessengerPublisher GetPublisher(string topic = "", byte qosLevel = 1)
        {
            return new MessengerPublisher()
            {
                _Messenger = this,
                Topic = topic,
                QosLevel = qosLevel,
            };
        }

        public void PrintDebugInfo()
        {
            ConsoleExtensions.WriteDebugLocation($"Connected to broker: {IsConnected}", 1);
        }

        public void Connect()
        {
            if (!String.IsNullOrEmpty(ServerAddress))
            {
                _Client = Factory.CreateMqttClientWrapper(ServerAddress);
                Task.Run(() =>
                {
                    try
                    {
                        _Client.Connect(Id.ToString());
                        Console.WriteLine("Successfully connected to the broker.");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to connect to broker: {ex.Message}");
                    }
                    PrintDebugInfo();
                });
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                _Client.Disconnect();
                Console.WriteLine("Successfully disconnected from the broker.");
            }
        }
    }
}
