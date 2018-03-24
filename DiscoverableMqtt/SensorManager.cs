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
        public IMessenger Messenger = null;
        public IMessengerPublisher Publisher = null;
        public Probes.AbstractTempProbe Probe = null;
        public AppSettings Settings;
        public IFactory Factory;

        public SensorManager(AppSettings settings, IFactory factory = null)
        {
            if (factory == null)
            {
                factory = new Factory();
            }
            Factory = factory;

            Probe = GetProbe(settings);
            Probe.DataChanged += Probe_DataChanged;

            UpdateFromSettings(settings);

            // Start the probe
            Probe.Start();
        }
        
        public void UpdateFromSettings(AppSettings settings)
        {
            if (settings.ApiId == int.MinValue)
            {
                settings.ApiId = RegisterWithHelen();
            }
            if (string.IsNullOrWhiteSpace(settings.BrokerUrl))
            {
                settings.BrokerUrl = GetConnectionInfoFromHelen();
            }

            Messenger = new Messenger(settings, new Factory());
            Publisher = Messenger.GetPublisher();
            Publisher.Topic = settings.ProbeTopic;

            Probe.MeasureInterval = settings.ProbeInterval;
            if (Probe is Probes.LinuxTempProbe)
            {
                var lprobe = Probe as Probes.LinuxTempProbe;
                lprobe.OneWireDeviceName = settings.ProbeDeviceName;
            }

            ConsoleExtensions.WriteDebugLocationEnabled = settings.DebugMode;
        }

        public void Dispose()
        {
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
            Publisher?.Publish(data);
            ConsoleExtensions.WriteDebugLocation(data, 0);
            Messenger.PrintDebugInfo();
        }

        private static Probes.AbstractTempProbe GetProbe(AppSettings settings)
        {
            Probes.AbstractTempProbe probe;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var linuxProbe = new Probes.LinuxTempProbe();
                linuxProbe.OneWireDeviceName = settings.ProbeDeviceName;
                probe = linuxProbe;
            }
            else
            {
                probe = new Probes.FakeTempProbe();
            }

            probe.MeasureInterval = settings.ProbeInterval;
            return probe;
        }

        private static HttpClient client = new HttpClient();

        private static string GetConnectionInfoFromHelen()
        {
            return "192.168.1.49";
        }

        private static int RegisterWithHelen()
        {
            client.BaseAddress = new Uri("http://localhost:51412/api/");

            HttpResponseMessage response;

            do
            {
                var route = "DeviceList/RegisterDevice/";
                route += HttpUtility.UrlEncode("DannyProbe");
                response = client.PostAsJsonAsync(route, new JValue("")).Result;
            } while (!response.IsSuccessStatusCode);

            var msg = response.Content.ReadAsStringAsync().Result;
            var indx1 = msg.IndexOf(':') + 1;
            var indx2 = msg.IndexOf("Please", indx1);
            var idStr = msg.Substring(indx1, indx2 - indx1).Trim();
            var id = int.Parse(idStr);

            return id;
        }
    }
}
