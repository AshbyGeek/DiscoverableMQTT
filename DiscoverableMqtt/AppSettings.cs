using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace DiscoverableMqtt
{
    public class AppSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        private string FilePath { get; set; }

        public string Id
        {
            get => _Name;
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _Name = "";

        public string BrokerUrl
        {
            get => _BrokerUrl;
            set
            {
                if (_BrokerUrl != value)
                {
                    _BrokerUrl = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _BrokerUrl = "";
        
        public bool DebugMode
        {
            get => _DebugMode;
            set
            {
                if (_DebugMode != value)
                {
                    _DebugMode = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _DebugMode = false;

        public string ProbeDeviceName
        {
            get => _ProbeDeviceName;
            set
            {
                if (_ProbeDeviceName != value)
                {
                    _ProbeDeviceName = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _ProbeDeviceName = "";

        public int ProbeInterval
        {
            get => _ProbeInterval;
            set
            {
                if (_ProbeInterval != value)
                {
                    _ProbeInterval = value;
                    OnPropertyChanged();
                }
            }
        }
        private int _ProbeInterval = 500;

        public string ProbeTopic
        {
            get => _ProbeTopic;
            set
            {
                if (_ProbeTopic != value)
                {
                    _ProbeTopic = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _ProbeTopic = "test/Linux";


        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            SaveSettings();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SaveSettings(string filePath = null)
        {
            if (filePath == null)
            {
                filePath = FilePath;
            }

            if (filePath != null)
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
        }

        public void ResetToDefualts()
        {
            var otherSettings = new AppSettings();
            foreach (var property in GetType().GetProperties())
            {
                if (property.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0)
                {
                    break;
                }

                var otherVal = property.GetValue(otherSettings);
                property.SetValue(this, otherVal);
            }
        }

        public static AppSettings GetSettings(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                try
                {
                    var appSettings = JsonConvert.DeserializeObject<AppSettings>(json);
                    appSettings.FilePath = filePath;
                    return appSettings;
                }
                catch (JsonException ex)
                {
                    ConsoleExtensions.WriteDebugLocation($"Failed to parse appSettings file from: {filePath}");
                }
            }

            var settings = new AppSettings()
            {
                FilePath = filePath,
            };
            settings.SaveSettings();

            return settings;
        }
    }
}
