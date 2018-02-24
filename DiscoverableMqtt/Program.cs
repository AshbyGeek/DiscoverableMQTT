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
        private static string AppSettingsFilePath => Path.Combine(Directory.GetCurrentDirectory(), "appSettings.json");

        static void Main(string[] args)
        {
            Console.WriteLine($"Getting AppSettings from: {AppSettingsFilePath}");
            var settings = AppSettings.GetSettings(AppSettingsFilePath);

            // Initialize a unique name if we don't already have one
            if (string.IsNullOrEmpty(settings.Id))
            {
                settings.Id = Guid.NewGuid().ToString();
            }

            var messenger = new Messenger(settings.BrokerUrl, settings.Id);
            settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(settings.BrokerUrl))
                {
                    messenger.ServerAddress = settings.BrokerUrl;
                }
            };
            
            
            var publisher = messenger.GetPublisher(settings.ProbeTopic, 1);
            settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(settings.ProbeTopic))
                {
                    publisher.Topic = settings.ProbeTopic;
                }
            };

            var probe = GetProbe(settings);
            probe.DataChanged += (s, e) =>
            {
                var data = e.Data.ToString();
                publisher.Publish(data);
                if (settings.DebugMode)
                {
                    ConsoleExtensions.WriteDebugLocation(data);
                }
            };

            if (settings.DebugMode)
            {
                SetDebugModeDisplaySettings();
            }
            settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(settings.DebugMode))
                {
                    if (settings.DebugMode)
                    {
                        SetDebugModeDisplaySettings();
                    }
                }
            };


            probe.Start();

            var properties = settings.GetType().GetProperties();
            while (true)
            {
                var tmpLine = Console.ReadLine();
                if (settings.DebugMode)
                {
                    Console.Clear();
                    Console.SetCursorPosition(0, Console.WindowHeight - 2);
                }

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
                    Console.SetCursorPosition(0, Console.WindowHeight-1);
                }
                else if (tmpLine.Equals("reset", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("Please confirm this action by typing 'yes please'");
                    if (Console.ReadLine() == "yes please")
                    {
                        settings.ResetToDefualts();
                    }
                }


                ReadProperty(settings, tmpLine);
            }

            probe.Stop();
            messenger.Disconnect();
        }

        private static void SetDebugModeDisplaySettings()
        {
            Console.SetWindowPosition(0, 0);
            Console.BufferHeight = Console.WindowHeight;
            Console.BufferWidth = Console.WindowWidth;
            Console.Clear();
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
        }

        private static PropertyInfo[] AppSettingsProperties = typeof(AppSettings).GetProperties();

        private static void PrintOptions(AppSettings settings)
        {
            Console.WriteLine("Main commands: ");
            Console.WriteLine("\tClear - Clears the console screen");
            Console.WriteLine("\tHelp - Displays this menu");
            Console.WriteLine("\tExit - Exits the programe");

            Console.WriteLine("\n\nOptions: ");
            foreach(var property in AppSettingsProperties)
            {
                Console.WriteLine($"\t{property.Name}: {property.PropertyType.ToString()}");
            }
            if (settings.DebugMode)
            {
                Console.WriteLine("\n");
            }
        }

        private static void ReadProperty(AppSettings settings, string line)
        {
            var parts = line.Split(":", 2);
            if (parts.Length != 2)
                return;

            foreach (var property in AppSettingsProperties)
            {
                if (parts[0].Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
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
                }
            }
        }

        private static Probes.AbstractTempProbe GetProbe(AppSettings settings)
        {
            Probes.AbstractTempProbe probe;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var linuxProbe = new Probes.LinuxTempProbe();
                linuxProbe.OneWireDeviceName = settings.ProbeDeviceName;
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
            
            probe.MeasureInterval = settings.ProbeInterval;
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
