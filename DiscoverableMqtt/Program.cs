using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace DiscoverableMqtt
{
    class Program
    {
        private static string AppSettingsFilePath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "appSettings.json");

        static void Main(string[] args)
        {
            var settings = new AppSettings()
            {
                FilePath = AppSettingsFilePath
            };

            // Set up bindings
            var messenger = new Messenger();
            settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(settings.BrokerUrl))
                {
                    messenger.ServerAddress = settings.BrokerUrl;
                    Console.WriteLine($"Broker Address set to: {messenger.ServerAddress}");
                }
                if (e.PropertyName == nameof(settings.Id))
                {
                    messenger.Id = settings.Id;
                    Console.WriteLine($"Messenger ID set to: {messenger.Id}");
                }
            };
            
            var publisher = messenger.GetPublisher();
            settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(settings.ProbeTopic))
                {
                    publisher.Topic = settings.ProbeTopic;
                    Console.WriteLine($"Publisher Topic set to: {publisher.Topic}");
                }
            };

            var probe = GetProbe(settings);
            probe.DataChanged += (s, e) =>
            {
                var data = e.Data.ToString();
                publisher.Publish(data);
                ConsoleExtensions.WriteDebugLocation(data, 0);
                messenger.PrintDebugInfo();
            };

            settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(settings.DebugMode))
                {
                    ConsoleExtensions.WriteDebugLocationEnabled = settings.DebugMode;
                    Console.WriteLine($"Debug option set to: {settings.DebugMode.ToString()}");
                }
            };

            // Populate the settings (which should trigger a whole bunch of changes above)
            settings.ReadFromFile();

            // Start the probe
            probe.Start();

            // Read user input
            var properties = settings.GetType().GetProperties();
            while (true)
            {
                Console.Write(">");
                var tmpLine = Console.ReadLine();

                if (tmpLine.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
                else if (tmpLine.Equals("help", StringComparison.InvariantCultureIgnoreCase))
                {
                    PrintOptions(settings);
                }
                else if (tmpLine.Equals("clear", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.Clear();
                }
                else if (tmpLine.Equals("reset", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("Please confirm this action by typing 'yes please'");
                    if (Console.ReadLine() == "yes please")
                    {
                        settings.ResetToDefaults();
                    }
                }
                else if (tmpLine.StartsWith("msg: "))
                {
                    var msg = tmpLine.Substring(4).Trim();
                    publisher.Publish(msg);
                }
                else if (!ReadProperty(settings, tmpLine))
                {
                    Console.WriteLine("Unrecognized command");
                }
            }

            probe.Stop();
            messenger.Disconnect();
        }
        
        private static PropertyInfo[] AppSettingsProperties = typeof(AppSettings).GetProperties();

        private static void PrintOptions(AppSettings settings)
        {
            Console.WriteLine("Main commands: ");
            Console.WriteLine("  Msg: <blah> - sends a message to the broker (if connected)");
            Console.WriteLine("  Clear - Clears the console screen");
            Console.WriteLine("  Help - Displays this menu");
            Console.WriteLine("  Exit - Exits the programe");

            Console.WriteLine("\n\nOptions: ");
            Console.WriteLine(settings.Json);
        }

        private static bool ReadProperty(AppSettings settings, string line)
        {
            var parts = line.Split(":", 2);
            if (parts.Length != 2)
                return false;

            foreach (var property in AppSettingsProperties)
            {
                if (parts[0].Trim().Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        object val = parts[1].Trim();
                        if (property.PropertyType != typeof(string))
                        {
                            val = JsonConvert.DeserializeObject(parts[1].Trim(), property.PropertyType);
                        }
                        property.SetValue(settings, val);
                        Console.WriteLine($"Successfully set {property.Name} to {val.ToString()}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Couldn't parse your value for {property.Name}: {ex.Message}");
                    }
                    return true;
                }
            }

            return false;
        }

        private static Probes.AbstractTempProbe GetProbe(AppSettings settings)
        {
            Probes.AbstractTempProbe probe;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var linuxProbe = new Probes.LinuxTempProbe();
                settings.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(settings.ProbeDeviceName))
                    {
                        linuxProbe.OneWireDeviceName = settings.ProbeDeviceName;
                    }
                };
                probe = linuxProbe;
            }
            else
            {
                probe = new Probes.FakeTempProbe();
            }
            
            settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(settings.ProbeInterval))
                {
                    probe.MeasureInterval = settings.ProbeInterval;
                }
            };

            return probe;
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
