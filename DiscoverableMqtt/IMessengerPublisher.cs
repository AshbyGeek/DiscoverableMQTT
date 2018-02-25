namespace DiscoverableMqtt
{
    public interface IMessengerPublisher
    {
        byte QosLevel { get; set; }
        bool Retain { get; set; }
        string Topic { get; set; }

        void Publish(byte[] content);
        void Publish(string content);
    }
}