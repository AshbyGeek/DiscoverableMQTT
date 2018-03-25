using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace DiscoverableMqtt
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    class Program
    {
        private static string AppSettingsFilePath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "appSettings.json");

        static void Main(string[] args)
        {
            var settings = new AppSettings()
            {
                FilePath = AppSettingsFilePath,
            };
            settings.ReadFromFile();

            using (var sensorManager = new SensorManager(settings))
            {
                var continueLooping = true;
                while (continueLooping)
                {
                    ConsoleExtensions.Write(">");
                    continueLooping = HandleInput(settings, sensorManager);
                }
            }
        }

        private static bool HandleInput(AppSettings settings, SensorManager manager)
        {
            string input = Console.ReadLine();
            if (input.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            else if (input.Equals("help", StringComparison.InvariantCultureIgnoreCase))
            {
                PrintOptions(settings);
            }
            else if (input.Equals("clear", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.Clear();
            }
            else if (input.Equals("reset", StringComparison.InvariantCultureIgnoreCase))
            {
                ConsoleExtensions.WriteLine("Please confirm this action by typing 'yes please'");
                if (Console.ReadLine() == "yes please")
                {
                    settings.ResetToDefaults();
                    manager.UpdateFromSettings(settings);
                    settings.SaveSettings();
                }
            }
            else if (input.StartsWith("msg: "))
            {
                var msg = input.Substring(4).Trim();
                manager.ProbePublisher.Publish(msg);
            }
            else if (input.Equals("stop", StringComparison.InvariantCultureIgnoreCase))
            {
                manager.Probe.Stop();
                ConsoleExtensions.WriteDebugLocation("       ", 0);
            }
            else if (input.Equals("start", StringComparison.InvariantCultureIgnoreCase))
            {
                manager.Probe.Start();
            }
            else if (input.Equals("list probes", StringComparison.InvariantCultureIgnoreCase) && manager.Probe is Probes.LinuxTempProbe)
            {
                var lprobe = manager.Probe as Probes.LinuxTempProbe;
                foreach (var name in lprobe.GetOneWireDeviceNames())
                {
                    ConsoleExtensions.WriteLine("  " + name);
                }
            }
            else
            {
                if (ReadProperty(settings, input))
                {
                    manager.UpdateFromSettings(settings);
                    settings.SaveSettings();
                }
                else
                {
                    ConsoleExtensions.WriteLine("Unrecognized command");
                }
            }
            return true;
        }
        
        private static PropertyInfo[] AppSettingsProperties = typeof(AppSettings).GetProperties();

        private static void PrintOptions(AppSettings settings, bool showLinuxProbeOptions = false)
        {
            ConsoleExtensions.WriteLine("Main commands: ");
            ConsoleExtensions.WriteLine("  Msg: <blah> - sends a message to the broker (if connected)");
            ConsoleExtensions.WriteLine("  Start - starts the probe if it isn't already started");
            ConsoleExtensions.WriteLine("  Stop - stops the probe if it is running");
            ConsoleExtensions.WriteLine("  Reset - resets all options to defaults (option must be confirmed)");
            if (showLinuxProbeOptions)
            {
                ConsoleExtensions.WriteLine("  list probes - lists all available 1 wire probes");
            }
            ConsoleExtensions.WriteLine("  Clear - Clears the console screen");
            ConsoleExtensions.WriteLine("  Help - Displays this menu");
            ConsoleExtensions.WriteLine("  Exit - Exits the programe");

            ConsoleExtensions.WriteLine("\n\nOptions: ");
            ConsoleExtensions.WriteLine(settings.Json);
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
    }
}
