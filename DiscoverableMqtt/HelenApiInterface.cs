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
        private static HttpClient client = new HttpClient();

        public string GetBrokerUrl(AppSettings settings)
        {
            try
            {
                client.BaseAddress = new Uri(settings.HelenApiUrl);

                HttpResponseMessage response;
                int numTries = 0;
                do
                {
                    var route = "DeviceData/GetConnectionInfo/";
                    route += HttpUtility.UrlEncode($"DannyProbe{settings.Guid:B}");
                    response = client.GetAsync(route).Result;
                    if (numTries++ > MAX_RETRIES)
                    {
                        throw new Exception("Connecting to Helen's server reached MAX_RETRIES");
                    }
                } while (!response.IsSuccessStatusCode);

                var msg = response.Content.ReadAsStringAsync().Result;
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
                client.BaseAddress = new Uri(settings.HelenApiUrl);

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
                var indx1 = msg.IndexOf(':') + 1;
                var indx2 = msg.IndexOf("Please", indx1);
                var idStr = msg.Substring(indx1, indx2 - indx1).Trim();
                var id = int.Parse(idStr);

                return id;
            }
            catch (Exception)
            {
                ConsoleExtensions.WriteLine("Failed to connect to Helen's API, falling back to the value in settings.");
                return settings.ApiId;
            }
        }
    }
}
