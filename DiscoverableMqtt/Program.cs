using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace DiscoverableMqtt
{
    class Program
    {
        private static string AppSettingsFilePath => Path.Combine(Directory.GetCurrentDirectory(), "appSettings.json");

        static void Main(string[] args)
        {
            var settings = AppSettings.GetSettings(AppSettingsFilePath);
            
            var messenger = new Messenger("192.168.1.49", settings.Name);
            messenger.Connect();
            var probe = new Probes.LinuxTempProbe()
            {
                MeasureInterval = 500,
            };

            var deviceName = probe.GetOneWireDeviceNames().First();
            probe.OneWireDeviceName = deviceName;
            Console.WriteLine($"Device: {deviceName}");

            var publisher = messenger.GetPublisher("test/Linux", 1);
            probe.DataChanged += (s, e) =>
            {
                var data = e.Data.ToString();
                publisher.Publish(data);
                Console.WriteLine(data);
            };


            probe.Start();

            string tmpLine = "";
            while (!tmpLine.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Type 'exit' to quit.");
                 = Console.ReadLine();
            }

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
