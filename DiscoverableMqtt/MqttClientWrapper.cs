using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using uPLibrary.Networking.M2Mqtt;

namespace DiscoverableMqtt
{
    public interface IMqttClientWrapper
    {
        MqttSettings Settings { get; }
        MqttProtocolVersion ProtocolVersion { get; set; }
        string WillMessage { get; }
        string WillTopic { get; }
        byte WillQosLevel { get; }
        bool WillFlag { get; }
        bool CleanSession { get; }
        bool IsConnected { get; }
        string ClientId { get; }

        event MqttClient.MqttMsgPublishEventHandler MqttMsgPublishReceived;
        event MqttClient.ConnectionClosedEventHandler ConnectionClosed;
        event MqttClient.MqttMsgUnsubscribedEventHandler MqttMsgUnsubscribed;
        event MqttClient.MqttMsgSubscribedEventHandler MqttMsgSubscribed;
        event MqttClient.MqttMsgPublishedEventHandler MqttMsgPublished;

        byte Connect(string clientId, string username, string password, bool willRetain, byte willQosLevel, bool willFlag, string willTopic, string willMessage, bool cleanSession, ushort keepAlivePeriod);
        byte Connect(string clientId, string username, string password, bool cleanSession, ushort keepAlivePeriod);
        byte Connect(string clientId, string username, string password);
        byte Connect(string clientId);
        void Disconnect();

        ushort Publish(string topic, byte[] message);
        ushort Publish(string topic, byte[] message, byte qosLevel, bool retain);
        ushort Subscribe(string[] topics, byte[] qosLevels);
        ushort Unsubscribe(string[] topics);
    }

    public class MqttClientWrapper : IMqttClientWrapper
    {
        public MqttClient Client { get; set; }

        public MqttClientWrapper(MqttClient client) { Client = client; }

        public static implicit operator MqttClientWrapper(MqttClient client)
        {
            return new MqttClientWrapper(client);
        }

        public static implicit operator MqttClient(MqttClientWrapper wrapper)
        {
            return wrapper.Client;
        }

        #region Wrappings
        public MqttSettings Settings => Client.Settings;
        public MqttProtocolVersion ProtocolVersion
        {
            get => Client.ProtocolVersion;
            set => Client.ProtocolVersion = value;
        }
        public string WillMessage => Client.WillMessage;
        public string WillTopic => Client.WillTopic;
        public byte WillQosLevel => Client.WillQosLevel;
        public bool WillFlag => Client.WillFlag;
        public bool CleanSession => Client.CleanSession;
        public bool IsConnected => Client.IsConnected;
        public string ClientId => Client.ClientId;

        public MqttClientWrapper(string brokerHostName) { Client = new MqttClient(brokerHostName); }
        public MqttClientWrapper(string brokerHostName, int brokerPort, bool secure, X509Certificate caCert, X509Certificate clientCert, MqttSslProtocols sslProtocol)
        { Client = new MqttClient(brokerHostName, brokerPort, secure, caCert, clientCert, sslProtocol); }
        public MqttClientWrapper(string brokerHostName, int brokerPort, bool secure, MqttSslProtocols sslProtocol, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback)
        { Client = new MqttClient(brokerHostName, brokerPort, secure, sslProtocol, userCertificateValidationCallback, userCertificateSelectionCallback); }
        public MqttClientWrapper(string brokerHostName, int brokerPort, bool secure, X509Certificate caCert, X509Certificate clientCert, MqttSslProtocols sslProtocol, RemoteCertificateValidationCallback userCertificateValidationCallback)
        { Client = new MqttClient(brokerHostName, brokerPort, secure, caCert, clientCert, sslProtocol, userCertificateValidationCallback); }
        public MqttClientWrapper(string brokerHostName, int brokerPort, bool secure, X509Certificate caCert, X509Certificate clientCert, MqttSslProtocols sslProtocol, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback)
        { Client = new MqttClient(brokerHostName, brokerPort, secure, caCert, clientCert, sslProtocol, userCertificateValidationCallback, userCertificateSelectionCallback); }

        public event MqttClient.MqttMsgPublishEventHandler MqttMsgPublishReceived
        {
            add { Client.MqttMsgPublishReceived += value; }
            remove { Client.MqttMsgPublishReceived -= value; }
        }
        public event MqttClient.ConnectionClosedEventHandler ConnectionClosed
        {
            add { Client.ConnectionClosed += value; }
            remove { Client.ConnectionClosed -= value; }
        }
        public event MqttClient.MqttMsgUnsubscribedEventHandler MqttMsgUnsubscribed
        {
            add { Client.MqttMsgUnsubscribed += value; }
            remove { Client.MqttMsgUnsubscribed -= value; }
        }
        public event MqttClient.MqttMsgSubscribedEventHandler MqttMsgSubscribed
        {
            add { Client.MqttMsgSubscribed += value; }
            remove { Client.MqttMsgSubscribed -= value; }
        }
        public event MqttClient.MqttMsgPublishedEventHandler MqttMsgPublished
        {
            add { Client.MqttMsgPublished += value; }
            remove { Client.MqttMsgPublished -= value; }
        }

        public byte Connect(string clientId, string username, string password, bool willRetain, byte willQosLevel, bool willFlag, string willTopic, string willMessage, bool cleanSession, ushort keepAlivePeriod)
        { return Client.Connect(clientId, username, password, willRetain, willQosLevel, willFlag, willTopic, willMessage, cleanSession, keepAlivePeriod); }

        public byte Connect(string clientId, string username, string password, bool cleanSession, ushort keepAlivePeriod)
        { return Client.Connect(clientId, username, password, cleanSession, keepAlivePeriod); }

        public byte Connect(string clientId, string username, string password)
        { return Client.Connect(clientId, username, password); }

        public byte Connect(string clientId)
        { return Client.Connect(clientId); }

        public void Disconnect()
        { Client.Disconnect(); }

        public ushort Publish(string topic, byte[] message)
        { return Client.Publish(topic, message); }

        public ushort Publish(string topic, byte[] message, byte qosLevel, bool retain)
        { return Client.Publish(topic, message, qosLevel, retain); }

        public ushort Subscribe(string[] topics, byte[] qosLevels)
        { return Client.Subscribe(topics, qosLevels); }

        public ushort Unsubscribe(string[] topics)
        { return Client.Unsubscribe(topics); }
        #endregion
    }

}
