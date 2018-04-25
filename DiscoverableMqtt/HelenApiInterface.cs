using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace DiscoverableMqtt
{
    public interface IHelenApiInterface
    {
        string GetBrokerUrl(AppSettings settings);
        int GetApiId(AppSettings settings);
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class HelenApiInterface : IHelenApiInterface
    {
        const int MAX_RETRIES = 3;
        private static HttpClient client;

        public string GetBrokerUrl(AppSettings settings)
        {
            try
            {
                RefreshClientIfUrlChanged(settings.HelenApiUrl);

                HttpResponseMessage response;
                int numTries = 0;
                do
                {
                    var route = "DeviceData/GetConnectionInfo/";
                    //route += HttpUtility.UrlEncode($"DannyProbe{settings.Guid:B}");
                    response = client.GetAsync(route).Result;
                    if (numTries++ > MAX_RETRIES)
                    {
                        throw new Exception("Connecting to Helen's server reached MAX_RETRIES");
                    }
                } while (!response.IsSuccessStatusCode);

                var msg = response.Content.ReadAsStringAsync().Result;
                msg = msg.Trim('\"', ' ');
                return msg;
            }
            catch (Exception)
            {
                ConsoleExtensions.WriteLine("Failed to connect to Helen's API, falling back to the value in settings.");
                return settings.BrokerUrl;
            }
        }

        public int GetApiId(AppSettings settings)
        {
            try
            {
                RefreshClientIfUrlChanged(settings.HelenApiUrl);

                HttpResponseMessage response;
                int numTries = 0;
                do
                {
                    var route = "DeviceList/RegisterDevice/";
                    route += HttpUtility.UrlEncode(settings.Name);
                    response = client.PostAsJsonAsync(route, new JValue("")).Result;
                    if (numTries++ > MAX_RETRIES)
                    {
                        throw new Exception("Connecting to Helen's server reached MAX_RETRIES");
                    }
                } while (!response.IsSuccessStatusCode);

                var msg = response.Content.ReadAsStringAsync().Result;
                var id = int.Parse(msg);

                return id;
            }
            catch (Exception)
            {
                ConsoleExtensions.WriteLine("Failed to connect to Helen's API, falling back to the value in settings.");
                return settings.ApiId;
            }
        }

        private void RefreshClientIfUrlChanged(string url)
        {
            var baseAddress = new Uri(url);
            if (client == null || client.BaseAddress != baseAddress)
            {
                client?.Dispose();
                client = new HttpClient();
                client.BaseAddress = baseAddress;
            }
        }
    }
}
