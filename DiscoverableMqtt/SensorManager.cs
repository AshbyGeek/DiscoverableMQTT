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

        public Probes.IAbstractTempProbe Probe;
        public IMessengerPublisher ProbePublisher;
        public IMessengerPublisher ConfigPublisher;
        public IMessengerListener ConfigListener;
        public IMessengerListener HelenApiLocationListener;

        public SensorManager(AppSettings settings, IFactory factory = null)
        {
            Settings = settings;

            if (factory == null)
            {
                factory = new Factory();
            }
            Factory = factory;
            
            Probe = Factory.CreateTempProbe(settings);
            Probe.DataChanged += Probe_DataChanged;

            UpdateFromSettings(settings);

            // Start the probe
            Probe.Start();
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

            HelenApi = Factory.CreateHelenApiInterface(settings.HelenApiUrl);

            settings.ApiId = HelenApi.GetApiId(settings.Name, settings.ApiId);
            settings.BrokerUrl = HelenApi.GetBrokerUrl(settings.BrokerUrl);
            capabilities.Settings = settings;

            Messenger = Factory.CreateMessenger(settings);

            Probe.MeasureInterval = settings.MeasureInterval;
            if (Probe is Probes.LinuxTempProbe)
            {
                var lprobe = Probe as Probes.LinuxTempProbe;
                lprobe.OneWireDeviceName = settings.ProbeDeviceName;
            }

            ProbePublisher?.Dispose();
            ProbePublisher = Messenger.GetPublisher(ProbeTopic);

            ConfigPublisher?.Dispose();
            ConfigPublisher = Messenger.GetPublisher(ConfigTopic);
            ConfigPublisher.Retain = true;
            Messenger.ConnectionStatusChanged += (s, e) => ConfigPublisher.Publish(capabilities.Json);

            ConfigListener?.Dispose();
            ConfigListener = Messenger.GetListener(DeviceConfigTopic);
            ConfigListener.MsgReceived += ConfigListener_MsgReceived;

            HelenApiLocationListener?.Dispose();
            HelenApiLocationListener = Messenger.GetListener(HelenApi.LocationMessageTopic);
            HelenApiLocationListener.MsgReceived += HelenApiLocationListener_MsgReceived;

            ConsoleExtensions.WriteDebugLocationEnabled = settings.DebugMode;
            Messenger.Connect();
        }

        private void HelenApiLocationListener_MsgReceived(object sender, MsgReceivedEventArgs e)
        {
            var result = HelenApi.GetApiLocationFromMessage(e.Message);
            if (result.HasValue && result.Value.apiId == Settings.ApiId)
            {
                Settings.Room = result.Value.deviceLocation;
                UpdateFromSettings(Settings);
            }
        }

        private void ConfigListener_MsgReceived(object sender, MsgReceivedEventArgs e)
        {
            Settings.Json = e.Message;
            UpdateFromSettings(Settings);
        }

        public void Dispose()
        {
            ProbePublisher?.Dispose();
            Probe.Stop();
            Messenger.Disconnect();
        }

        private void Probe_DataChanged(object sender, GenericEventArgs<float> e)
        {
            var data = e.Data.ToString();
            ProbePublisher?.Publish(HelenApi.CreateDataMessage(Settings.ApiId, data));
            ConsoleExtensions.WriteDebugLocation(data, 0);
            Messenger.PrintDebugInfo();
        }

        private string DeviceConfigTopic => "DeviceConfig/" + Settings.Name;
        private string ConfigTopic => String.IsNullOrEmpty(Settings.Room)? $"Devices/Unconfigured/{Settings.Name}" : $"Devices/Configured/{Settings.Name}";
        private string ProbeTopic => $"Rooms/{Settings.Room}/Temperature";

        private ProbeCapability capabilities = new ProbeCapability("Temperature");
    }
}
