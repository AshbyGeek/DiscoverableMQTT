using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoverableMqtt
{
    public interface IMessengerPublisher : IDisposable
    {
        event EventHandler Disposed;

        int Id { get; set; }

        QosLevel QosLevel { get; set; }
        bool Retain { get; set; }
        string Topic { get; set; }

        void Publish(string content);
        void PublishWithoutHeader(string content);
    }

    public class MessengerPublisher : IMessengerPublisher
    {
        public event EventHandler Disposed;

        public MessengerPublisher(IMqttClientWrapper client, int id)
        {
            Client = client;
            Id = id;
        }

        public IMqttClientWrapper Client { get; }
        public int Id { get; set; }

        public QosLevel QosLevel { get; set; } = QosLevel.AtLeastOnce;
        public bool Retain { get; set; } = false;
        public string Topic { get; set; }

        public void Publish(string content) => PublishWithoutHeader(PacketHeader + content);

        public void PublishWithoutHeader(string content)
        {
            byte[] newContent = Encoding.UTF8.GetBytes(content);

            if (Client.IsConnected)
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                try
                {
                    Client.Publish(Topic, bytes, (byte)QosLevel, Retain);
                }
                catch (Exception ex)
                {
                    ConsoleExtensions.WriteLine("Failed to publish data: " + ex.Message);
                }
            }
        }

        public void Dispose()
        {
            if (Retain)
            {
                //Clear out any retained messages
                PublishWithoutHeader("");
            }
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        private string PacketHeader => Id.ToString() + " ";
    }
}
