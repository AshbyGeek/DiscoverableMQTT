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
        public Guid Id
        {
            get => _Id;
            set
            {
                if (_Id != value)
                {
                    _Id = value;
                    OnPropertyChanged();
                }
            }
        }
        private Guid _Id = Guid.NewGuid();

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
        private string _ProbeTopic = "test/linux";

        #region ------------------ INotifyPropertyChanged ------------------
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            SaveSettings();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ------------------ Utilities/Unsaved properties and methods ------------------
        [JsonIgnore]
        public string FilePath { get; set; }

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
