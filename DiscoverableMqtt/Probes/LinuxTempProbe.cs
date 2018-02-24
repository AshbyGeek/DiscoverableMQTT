using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DiscoverableMqtt.Probes
{
    public class LinuxTempProbe : AbstractTempProbe
    {
        /// <summary>
        /// The location where most linux distros keep their 1 wire devices
        /// Assuming, of course, that the distro has a 1 wire driver up and going
        /// </summary>
        private const string DFLT_1WIRE_DEVS_PATH = "/sys/bus/w1/devices";

        private const string W1_FILE_NAME = "w1_slave";

        /// <summary>
        /// The path where this class will look for potential one wire devices to read from
        /// </summary>
        public string OneWireDevicesPath { get; set; } = DFLT_1WIRE_DEVS_PATH;
        
        public LinuxTempProbe() : base()
        {
        }

        /// <summary>
        /// Gets a list of available one wire devices, so that a user might be able to pick one.
        /// </summary>
        /// <returns>a list of available 1 wire devices</returns>
        public IEnumerable<string> GetOneWireDeviceNames()
        {
            IEnumerable<string> folders;
            try
            {
                folders = Directory.EnumerateDirectories(OneWireDevicesPath);
            }
            catch (Exception ex)
            {
                throw new Exception("Error enumerating directories OneWireDevicesPath folder", ex);
            }

            var filteredFolders = folders.Select(f => Path.GetFileName(f)).Where(folder => !folder.Contains("bus_master") && File.Exists(Path.Combine(OneWireDevicesPath, folder, W1_FILE_NAME)));

            return filteredFolders;
        }

        public string OneWireDeviceName { get; set; } = "";

        private string FilePath => Path.Combine(OneWireDevicesPath, OneWireDeviceName, W1_FILE_NAME);
        
        protected override float GetNewVal()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    var text = File.ReadAllText(FilePath);
                    Console.WriteLine("----------------  Raw Content ----------------");
                    Console.Write(text);
                    Console.WriteLine("----------------------------------------------");

                    var indexOfYes = text.LastIndexOf("yes", StringComparison.InvariantCultureIgnoreCase);
                    var indexOfTEqualsAfterYes = text.IndexOf("T=", indexOfYes, StringComparison.InvariantCultureIgnoreCase);
                    var indexOfSecondNewline = text.IndexOf('\n', indexOfTEqualsAfterYes + 2);
                    if (indexOfSecondNewline == -1)
                    {
                        indexOfSecondNewline = text.Length;
                    }

                    var stringVal = text.Substring(indexOfTEqualsAfterYes+2, indexOfSecondNewline - indexOfTEqualsAfterYes-2);
                    var floatVal = FixedPointConversion(stringVal, 2);
                    var farenheit = CelciusToFarenheit(floatVal);
                    return farenheit;
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Failed to get a reading, using the previous reading.");
                }
            }
            return GetCurrentData();
        }

        private float FixedPointConversion(string stringVal, int numFixedDigitsLeftOfDecimal)
        {
            double intVal = int.Parse(stringVal);
            double divisor = Math.Pow(10, stringVal.Length - numFixedDigitsLeftOfDecimal);
            float floatVal = (float)(intVal / divisor);
            return floatVal;
        }

        private float CelciusToFarenheit(float tempCelcius)
        {
            return tempCelcius * 9.0f / 5.0f + 32.0f;
        }
    }
}
