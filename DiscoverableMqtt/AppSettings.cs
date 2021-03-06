﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace DiscoverableMqtt
{
    public class AppSettings
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public int ApiId { get; set; } = int.MinValue;
        public string BrokerUrl { get; set; } = "";
        public bool DebugMode { get; set; } = false;
        public string ProbeDeviceName { get; set; } = "";
        public int MeasureInterval { get; set; } = 500;
        public string Room { get; set; } = "";
        public string HelenApiUrl { get; set; } = "http://localhost:51412/api/";

        public string Name => $"DannyProbe{Guid:B}";

        #region ------------------ Utilities/Unsaved properties and methods ------------------
        [JsonIgnore]
        public string FilePath { get; set; }

        /// <summary>
        /// Serializes this AppSettings into a Json string
        /// or attempts to set the values of this AppSettings using the given Json string.
        /// The setter is perfectly happy to take values for only some of the AppSettings
        /// Any serialization errors are caught, a message written to the Console, and otherwise ignored.
        /// </summary>
        [JsonIgnore]
        public string Json
        {
            get
            {
                try
                {
                    return JsonConvert.SerializeObject(this, Formatting.Indented);
                }
                catch (Exception ex)
                {
                    ConsoleExtensions.WriteLine($"Error generating json: {ex.Message}");
                    return "";
                }
            }
            set
            {
                try
                {
                    JsonConvert.PopulateObject(value, this);
                }
                catch (Exception ex)
                {
                    ConsoleExtensions.WriteLine($"Error updating object from json: {ex.Message}");
                }
            }
        }

        public void SaveSettings(string filePath = null)
        {
            if (filePath == null)
            {
                filePath = FilePath;
            }

            try
            {
                File.WriteAllText(filePath, Json);
            }
            catch (Exception ex)
            {
                ConsoleExtensions.WriteLine($"Could not write file: {ex.Message}");
            }
        }

        public void ResetToDefaults()
        {
            var otherSettings = new AppSettings();
            Json = otherSettings.Json;
        }

        public void ReadFromFile(string filePath = null)
        {
            if (filePath == null)
            {
                filePath = FilePath;
            }

            try
            {
                Json = File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                ConsoleExtensions.WriteLine($"Could not populate settings from {filePath}: {ex.Message}");
            }
        }
        #endregion
    }
}
