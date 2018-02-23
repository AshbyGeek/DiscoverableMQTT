using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace DiscoverableMqtt
{
    class Program
    {
        private static string AppSettingsFilePath => Path.Combine(Directory.GetCurrentDirectory(), "appSettings.json");

        static void Main(string[] args)
        {
            var settings = AppSettings.GetSettings(AppSettingsFilePath);

            var id = GetID(settings.Name);
            var messenger = new Messenger("192.168.1.49", id);
            messenger.Connect();
            var probe = new Probes.LinuxTempProbe()
            {
                MeasureInterval = 500,
            };

            var publisher = messenger.GetPublisher("test/Linux", 1);
            probe.DataChanged += (s, e) => publisher.Publish(e.Data.ToString());

            probe.Start();
            Thread.Sleep(1000*10);
            probe.Stop();
            messenger.Disconnect();
        }

        /// <summary>
        /// A stand in for Helen's interface call, whenever she gets around to it
        /// </summary>
        /// <param name="name">The name to pass to Helen</param>
        /// <returns>The database ID to use</returns>
        private static string GetID(string name)
        {
            return name;
        }
    }
}
