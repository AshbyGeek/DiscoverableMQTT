﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoverableMqtt
{
    public interface IMessenger
    {
        bool IsConnected { get; }

        void Connect();
        void Disconnect();
        IMessengerPublisher GetPublisher(string topic = "", QosLevel qosLevel = QosLevel.AtLeastOnce);
        IMessengerListener GetListener(string topic = "", QosLevel qosLevel = QosLevel.AtLeastOnce);
        void PrintDebugInfo();
    }

    public enum QosLevel
    {
        AtMostOnce = 0,
        AtLeastOnce = 1,
        ExactlyOnce = 2
    }

    public class Messenger : IMessenger
    {
        private bool connectInProgress = false;

        private int ApiId;
        private Guid Guid;
        private IFactory Factory;
        private IMqttClientWrapper Client;

        #region  Constructors
        public Messenger(AppSettings settings, IFactory factory)
        {
            ApiId = settings.ApiId;
            Guid = settings.Guid;
            Factory = factory;

            try
            {
                Client = Factory.CreateMqttClientWrapper(settings.BrokerUrl);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion
        
        public bool IsConnected
        {
            get
            {
                try
                {
                    return Client.IsConnected;
                }
                catch (Exception ex)
                {
                    ConsoleExtensions.WriteLine($"Exception while querying Mqtt about connection status : {ex.Message}");
                    return false;
                }
            }
        }
        
        public IMessengerPublisher GetPublisher(string topic = "", QosLevel qosLevel = QosLevel.AtLeastOnce)
        {
            var messenger = Factory.CreateMessengerPublisher(Client, ApiId);
            messenger.Topic = topic;
            messenger.QosLevel = qosLevel;
            return messenger;
        }

        public IMessengerListener GetListener(string topic = "", QosLevel qosLevel = QosLevel.AtLeastOnce)
        {
            var listener = Factory.CreateMessengerListener(Client);
            listener.Topic = topic;
            listener.QosLevel = qosLevel;
            return listener;
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void PrintDebugInfo()
        {
            if (connectInProgress)
            {
                ConsoleExtensions.WriteDebugLocation($"Connected to broker: ...", 3);
            }
            else
            {
                ConsoleExtensions.WriteDebugLocation($"Connected to broker: {IsConnected}", 3);
            }
        }

        public void Connect()
        {
            if (!IsConnected && !connectInProgress)
            {
                connectInProgress = true;
                Task.Run(() =>
                {
                    try
                    {
                        Client.Connect(Guid.ToString());
                    }
                    catch (Exception)
                    {
                        ConsoleExtensions.WriteDebugLocation($"Connected to broker: failed", 5);
                    }
                    finally
                    {
                        connectInProgress = false;
                    }
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
