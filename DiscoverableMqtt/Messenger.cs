using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoverableMqtt
{
    public interface IMessenger
    {
        IFactory Factory { get; set; }
        int Id { get; set; }
        bool IsConnected { get; }
        string ServerAddress { get; set; }

        void Connect();
        void Disconnect();
        IMessengerPublisher GetPublisher(string topic = "", byte qosLevel = 1);
        void PrintDebugInfo();
    }

    public class Messenger : IMessenger
    {
        internal object MessengerLock = new object();
        
        /// <summary>
        /// A factory to be used for producing needed components, like an MqttClientWrapper.
        /// Probably only needed for unit tests.
        /// </summary>
        public IFactory Factory { get; set; } = new Factory();

        public int Id
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
        private int _Id = int.MinValue;

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
                    return Client?.IsConnected ?? false;
                }
                catch (Exception ex)
                {
                    ConsoleExtensions.WriteLine($"Exception while querying Mqtt about connection status : {ex.Message}");
                    return false;
                }
            }
        }

        private IMqttClientWrapper Client
        {
            get => _Client;
            set
            {
                if (value != _Client)
                {
                    _Client = value;
                    _Publishers.ForEach(x => x.Client = _Client);
                    _Listeners.ForEach(x => x.Client = _Client);
                }
            }
        }
        private IMqttClientWrapper _Client;

        #region  Constructors
        public Messenger() { }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public Messenger(string serverAddress, int id)
        {
            Id = id;
            ServerAddress = serverAddress;
        }
        #endregion

        public IMessengerPublisher GetPublisher(string topic = "", byte qosLevel = 1)
        {
            var messenger = Factory.CreateMessengerPublisher(_Client, _Id);
            messenger.Topic = topic;
            messenger.QosLevel = qosLevel;
            _Publishers.Add(messenger);
            messenger.Disposed += (s, e) => _Publishers.Remove(s as IMessengerPublisher);
            return messenger;
        }
        private List<IMessengerPublisher> _Publishers = new List<IMessengerPublisher>();

        public IMessengerListener GetListener(string topic = "", byte qosLevel = 1)
        {
            var listener = Factory.CreateMessengerListener(Client);
            listener.Topic = topic;
            listener.QosLevel = qosLevel;

            _Listeners.Add(listener);
            listener.Disposed += (s, e) => _Listeners.Remove(s as IMessengerListener);
            return listener;
        }
        private List<IMessengerListener> _Listeners = new List<IMessengerListener>();

        public void PrintDebugInfo()
        {
            ConsoleExtensions.WriteDebugLocation($"Connected to broker: {IsConnected}", 1);
        }

        public void Connect()
        {
            if (!String.IsNullOrEmpty(ServerAddress))
            {
                Client = Factory.CreateMqttClientWrapper(ServerAddress);
                Task.Run(() =>
                {
                    try
                    {
                        Client.Connect(Id.ToString());
                        ConsoleExtensions.WriteLine("Successfully connected to the broker.");
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
                Client.Disconnect();
                ConsoleExtensions.WriteLine("Successfully disconnected from the broker.");
            }
        }
    }
}
