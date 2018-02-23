using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace DiscoverableMqtt
{
    public class AppSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        private string FilePath { get; set; }

        public string Name
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
        private string _Name = $"UnNamed-{Guid.NewGuid()}";
        
        
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
                    Console.WriteLine($"Failed to parse appSettings file from: {filePath}");
                }
            }

            return new AppSettings()
            {
                FilePath = filePath,
            };
        }
    }
}
