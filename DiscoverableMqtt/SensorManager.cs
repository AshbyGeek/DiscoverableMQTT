using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;

namespace DiscoverableMqtt
{
    public class SensorManager : IDisposable
    {
        public AppSettings Settings;
        public IFactory Factory;
        public IHelenApiInterface HelenApi;
        public IMessenger Messenger;

        public Probes.IAbstractProbe TempProbe;
        public IMessengerPublisher TempProbePublisher;

        public Probes.IAbstractProbe MoistureProbe;
        public IMessengerPublisher MoisturePublisher;

        public IMessengerListener ConfigListener;

        public SensorManager(AppSettings settings, IFactory factory = null, IHelenApiInterface helenApi = null)
        {
            Settings = settings;

            if (factory == null)
            {
                factory = new Factory();
            }
            Factory = factory;

            if (helenApi == null)
            {
                helenApi = new HelenApiInterface();
            }
            HelenApi = helenApi;

            TempProbe?.Dispose();
            TempProbe = Factory.CreateTempProbe(settings);
            TempProbe.DataChanged += TempProbe_DataChanged;

            MoistureProbe?.Dispose();
            MoistureProbe = Factory.CreateSoilMoistureProbe(settings);
            MoistureProbe.DataChanged += MoistureProbe_DataChanged;

            UpdateFromSettings(settings);

            // Start the probe
            TempProbe.Start();
            MoistureProbe.Start();
        }

        public void UpdateFromSettings(AppSettings settings = null)
        {
            if (settings == null)
            {
                settings = Settings;
            }
            else
            {
                Settings = settings;
            }
            settings.ApiId = HelenApi.GetApiId(settings);
            settings.BrokerUrl = HelenApi.GetBrokerUrl(settings);

            Messenger = Factory.CreateMessenger(settings);

            // Set up temperature measurements and publishing
            TempProbePublisher?.Dispose();
            TempProbePublisher = Messenger.GetPublisher(settings.TemperatureTopic);
            TempProbe.MeasureInterval = settings.ProbeInterval;
            if (TempProbe is Probes.LinuxTempProbe)
            {
                var lprobe = TempProbe as Probes.LinuxTempProbe;
                lprobe.OneWireDeviceName = settings.ProbeDeviceName;
            }

            // Set up soil moisture measurements and publishing
            MoisturePublisher?.Dispose();
            MoisturePublisher = Messenger.GetPublisher(settings.MoistureTopic);
            MoistureProbe.MeasureInterval = settings.ProbeInterval;

            ConfigListener?.Dispose();
            ConfigListener = Messenger.GetListener("DeviceConfig/" + settings.Name);
            ConfigListener.MsgReceived += ConfigListener_MsgReceived;

            ConsoleExtensions.WriteDebugLocationEnabled = settings.DebugMode;
        }

        private void ConfigListener_MsgReceived(object sender, MsgReceivedEventArgs e)
        {
            Settings.Json = e.Message;
            UpdateFromSettings(Settings);
        }

        public void Dispose()
        {
            TempProbePublisher?.Dispose();
            TempProbe?.Stop();
            TempProbe?.Dispose();
            MoisturePublisher?.Dispose();
            MoistureProbe?.Stop();
            MoistureProbe?.Dispose();
            Messenger.Disconnect();
        }

        private void TempProbe_DataChanged(object sender, GenericEventArgs<float> e)
        {
            var data = e.Data.ToString();
            if (!Messenger.IsConnected)
            {
                Messenger.Connect();
            }
            TempProbePublisher?.Publish(data);
            ConsoleExtensions.WriteDebugLocation(data, 0);
            Messenger.PrintDebugInfo();
        }

        private void MoistureProbe_DataChanged(object sender, GenericEventArgs<float> e)
        {
            var data = e.Data.ToString();
            if (!Messenger.IsConnected)
            {
                Messenger.Connect();
            }
            MoisturePublisher?.Publish(data);
            ConsoleExtensions.WriteDebugLocation(data, 1);
            Messenger.PrintDebugInfo();
        }
    }
}
