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
            settings.ApiId = HelenApi.GetApiId(settings);
            settings.BrokerUrl = HelenApi.GetBrokerUrl(settings);

            Messenger = Factory.CreateMessenger(settings);

            ProbePublisher?.Dispose();
            ProbePublisher = Messenger.GetPublisher(settings.ProbeTopic);

            Probe.MeasureInterval = settings.ProbeInterval;
            if (Probe is Probes.LinuxTempProbe)
            {
                var lprobe = Probe as Probes.LinuxTempProbe;
                lprobe.OneWireDeviceName = settings.ProbeDeviceName;
            }

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
            ProbePublisher?.Dispose();
            Probe.Stop();
            Messenger.Disconnect();
        }

        private void Probe_DataChanged(object sender, GenericEventArgs<float> e)
        {
            var data = e.Data.ToString();
            if (!Messenger.IsConnected)
            {
                Messenger.Connect();
            }
            ProbePublisher?.Publish(data);
            ConsoleExtensions.WriteDebugLocation(data, 0);
            Messenger.PrintDebugInfo();
        }
    }
}
