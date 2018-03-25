using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DiscoverableMqtt
{
    public interface IProbeCapability
    {
        List<string> Capabilities { get; }
        AppSettings Settings { get; set; }
        string Json { get; set; }
    }

    public class ProbeCapability
    {
        public ProbeCapability() { }
        public ProbeCapability(params string[] capabilities)
        {
            Capabilities.UnionWith(capabilities);
        }
        public ProbeCapability(AppSettings settings, params string[] capabilities)
        {
            Settings = settings;
            Capabilities.UnionWith(capabilities);
        }

        public AppSettings Settings { get; set; }
    
        public HashSet<string> Capabilities { get; private set; } = new HashSet<string>();

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
    }
}
